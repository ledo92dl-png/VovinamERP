using MediatR;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Common.Exceptions;
using VovinamERP.Application.Finance.Common;
using VovinamERP.Application.Finance.TuitionCalculation;
using VovinamERP.Application.Students.Common;
using VovinamERP.Domain.Finance;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.GenerateMonthlyTuitionInvoice;

public sealed class GenerateMonthlyTuitionInvoiceCommandHandler
    : IRequestHandler<
        GenerateMonthlyTuitionInvoiceCommand,
        GenerateMonthlyTuitionInvoiceResult>
{
    private readonly ITuitionInvoiceRepository _tuitionInvoiceRepository;
private readonly IStudentCreditRepository _studentCreditRepository;
private readonly IAttendanceRepository _attendanceRepository;
private readonly IStudentRepository _studentRepository;
private readonly IUnitOfWork _unitOfWork;
private readonly IStudentCreditTransaction _studentCreditTransaction;

   public GenerateMonthlyTuitionInvoiceCommandHandler(
    ITuitionInvoiceRepository tuitionInvoiceRepository,
    IStudentCreditRepository studentCreditRepository,
    IAttendanceRepository attendanceRepository,
    IStudentRepository studentRepository,
    IUnitOfWork unitOfWork,
    IStudentCreditTransaction studentCreditTransaction)
{
    _tuitionInvoiceRepository = tuitionInvoiceRepository;
    _studentCreditRepository = studentCreditRepository;
    _attendanceRepository = attendanceRepository;
    _studentRepository = studentRepository;
    _unitOfWork = unitOfWork;
    _studentCreditTransaction = studentCreditTransaction;
}

    public async Task<GenerateMonthlyTuitionInvoiceResult> Handle(
    GenerateMonthlyTuitionInvoiceCommand request,
    CancellationToken cancellationToken)
{
    if (request.TenantId == Guid.Empty)
    {
        throw new InvalidOperationException(
            "TenantId is required.");
    }

    if (request.StudentId == Guid.Empty)
    {
        throw new InvalidOperationException(
            "StudentId is required.");
    }

    if (request.Year < 2000)
    {
        throw new InvalidOperationException(
            "Billing year is invalid.");
    }

    if (request.Month < 1 || request.Month > 12)
    {
        throw new InvalidOperationException(
            "Billing month must be between 1 and 12.");
    }

    if (request.StandardAmount <= 0)
    {
        throw new InvalidOperationException(
            "Standard tuition amount must be greater than zero.");
    }

    if (string.IsNullOrWhiteSpace(request.InvoiceNumber))
    {
        throw new InvalidOperationException(
            "Invoice number is required.");
    }

    var transactionResult =
        await _studentCreditTransaction.ExecuteWithStudentLockAsync(
            request.TenantId,
            request.StudentId,
            async ct =>
            {
                var student =
                    await _studentRepository.GetByIdAsync(
                        request.TenantId,
                        request.StudentId,
                        ct);

                if (student is null)
                {
                    return Result<GenerateMonthlyTuitionInvoiceResult>
                        .Failure(
                            new Error(
                                "Student.NotFound",
                                $"Student '{request.StudentId}' was not found."));
                }

                if (await _tuitionInvoiceRepository
                    .ExistsForStudentMonthAsync(
                        request.TenantId,
                        request.StudentId,
                        request.Year,
                        request.Month,
                        ct))
                {
                    return Result<GenerateMonthlyTuitionInvoiceResult>
                        .Failure(
                            new Error(
                                "TuitionInvoice.DuplicateMonth",
                                $"A tuition invoice already exists for student " +
                                $"'{request.StudentId}' in " +
                                $"{request.Month:D2}/{request.Year}."));
                }

                if (await _tuitionInvoiceRepository
                    .ExistsByInvoiceNumberAsync(
                        request.TenantId,
                        request.InvoiceNumber,
                        ct))
                {
                    return Result<GenerateMonthlyTuitionInvoiceResult>
                        .Failure(
                            new Error(
                                "TuitionInvoice.DuplicateNumber",
                                $"Tuition invoice number " +
                                $"'{request.InvoiceNumber}' already exists."));
                }

                var attendanceCount =
                    await _attendanceRepository
                        .CountStudentAttendancesByMonthAsync(
                            request.TenantId,
                            request.StudentId,
                            request.Year,
                            request.Month,
                            ct);

                var calculation =
                    TuitionCalculator.Calculate(
                        attendanceCount,
                        request.StandardAmount,
                        TuitionCalculationPolicy.Default);

                var invoiceResult =
                    TuitionInvoice.CreateMonthlyInvoice(
                        request.TenantId,
                        request.StudentId,
                        request.InvoiceNumber,
                        request.Year,
                        request.Month,
                        request.StandardAmount,
                        calculation.DiscountAmount,
                        request.Note);

                if (invoiceResult.IsFailure ||
                    invoiceResult.Value is null)
                {
                    return Result<GenerateMonthlyTuitionInvoiceResult>
                        .Failure(invoiceResult.Error);
                }

                var invoice = invoiceResult.Value;

                // Automatically apply available Student Credit
                // while the student row is locked.
                if (invoice.BalanceAmount > 0)
                {
                    var creditBalance =
                        await _studentCreditRepository.GetBalanceAsync(
                            request.TenantId,
                            request.StudentId,
                            ct);

                    var creditToApply =
                        Math.Min(
                            creditBalance,
                            invoice.BalanceAmount);

                    if (creditToApply > 0)
                    {
                        var applyCreditResult =
                            invoice.ApplyCredit(
                                creditToApply);

                        if (applyCreditResult.IsFailure)
                        {
                            return Result<GenerateMonthlyTuitionInvoiceResult>
                                .Failure(applyCreditResult.Error);
                        }

                        var creditTransactionResult =
                            StudentCreditTransaction.Create(
                                request.TenantId,
                                request.StudentId,
                                StudentCreditTransactionType.Debit,
                                creditToApply,
                                new DateOnly(
                                    request.Year,
                                    request.Month,
                                    1),
                                null,
                                null,
                                invoice.Id,
                                $"Automatically apply Student Credit " +
                                $"to tuition {request.Month:D2}/{request.Year}");

                        if (creditTransactionResult.IsFailure ||
                            creditTransactionResult.Value is null)
                        {
                            return Result<GenerateMonthlyTuitionInvoiceResult>
                                .Failure(
                                    creditTransactionResult.Error);
                        }

                        await _studentCreditRepository.AddAsync(
                            creditTransactionResult.Value,
                            ct);
                    }
                }

                await _tuitionInvoiceRepository.AddAsync(
                    invoice,
                    ct);

                // Save the invoice and Student Credit Debit
                // in the same database transaction.
                await _unitOfWork.SaveChangesAsync(ct);

                var result =
                    new GenerateMonthlyTuitionInvoiceResult(
                        invoice.Id,
                        invoice.TenantId,
                        invoice.StudentId,
                        invoice.InvoiceNumber,
                        invoice.Year,
                        invoice.Month,
                        calculation.AttendanceCount,
                        calculation.StandardAmount,
                        calculation.ChargeRate,
                        calculation.DiscountAmount,
                        calculation.PayableAmount,
                        invoice.PaidAmount,
                        invoice.BalanceAmount,
                        invoice.Status,
                        invoice.Note);

                return Result<GenerateMonthlyTuitionInvoiceResult>
                    .Success(result);
            },
            cancellationToken);

    if (transactionResult.IsFailure ||
        transactionResult.Value is null)
    {
        if (transactionResult.Error.Code ==
            "TuitionInvoice.DuplicateMonth" ||
            transactionResult.Error.Code ==
            "TuitionInvoice.DuplicateNumber")
        {
            throw new ConflictException(
                transactionResult.Error.Message);
        }

        throw new InvalidOperationException(
            transactionResult.Error.Message);
    }

    return transactionResult.Value;
}
}