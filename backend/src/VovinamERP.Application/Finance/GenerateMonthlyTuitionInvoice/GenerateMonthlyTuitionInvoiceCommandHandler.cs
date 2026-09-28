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

   public GenerateMonthlyTuitionInvoiceCommandHandler(
    ITuitionInvoiceRepository tuitionInvoiceRepository,
    IStudentCreditRepository studentCreditRepository,
    IAttendanceRepository attendanceRepository,
    IStudentRepository studentRepository,
    IUnitOfWork unitOfWork)
{
    _tuitionInvoiceRepository = tuitionInvoiceRepository;
    _studentCreditRepository = studentCreditRepository;
    _attendanceRepository = attendanceRepository;
    _studentRepository = studentRepository;
    _unitOfWork = unitOfWork;
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

        var student = await _studentRepository.GetByIdAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

        if (student is null)
        {
            throw new InvalidOperationException(
                $"Student '{request.StudentId}' was not found.");
        }

        if (await _tuitionInvoiceRepository.ExistsForStudentMonthAsync(
    request.TenantId,
    request.StudentId,
    request.Year,
    request.Month,
    cancellationToken))
{
    throw new ConflictException(
        $"A tuition invoice already exists for student " +
        $"'{request.StudentId}' in {request.Month:D2}/{request.Year}.");
}

        if (await _tuitionInvoiceRepository.ExistsByInvoiceNumberAsync(
    request.TenantId,
    request.InvoiceNumber,
    cancellationToken))
{
    throw new ConflictException(
        $"Tuition invoice number '{request.InvoiceNumber}' already exists.");
}

        var attendanceCount =
            await _attendanceRepository.CountStudentAttendancesByMonthAsync(
                request.TenantId,
                request.StudentId,
                request.Year,
                request.Month,
                cancellationToken);

        var calculation = TuitionCalculator.Calculate(
            attendanceCount,
            request.StandardAmount,
            TuitionCalculationPolicy.Default);

        Result<TuitionInvoice> invoiceResult =
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
            throw new InvalidOperationException(
                invoiceResult.Error.Message);
        }

       var invoice = invoiceResult.Value;

// Tự động sử dụng tiền dư hiện có của Môn sinh
// cho hóa đơn học phí mới.
if (invoice.BalanceAmount > 0)
{
    var creditBalance =
        await _studentCreditRepository.GetBalanceAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

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
            throw new InvalidOperationException(
                applyCreditResult.Error.Message);
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
                $"Tự động sử dụng tiền dư cho học phí tháng {request.Month:D2}/{request.Year}");

        if (creditTransactionResult.IsFailure ||
            creditTransactionResult.Value is null)
        {
            throw new InvalidOperationException(
                creditTransactionResult.Error.Message);
        }

        await _studentCreditRepository.AddAsync(
            creditTransactionResult.Value,
            cancellationToken);
    }
}

await _tuitionInvoiceRepository.AddAsync(
    invoice,
    cancellationToken);

// Invoice + Debit Credit được lưu cùng một lần.
await _unitOfWork.SaveChangesAsync(
    cancellationToken);

        return new GenerateMonthlyTuitionInvoiceResult(
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
    }
}