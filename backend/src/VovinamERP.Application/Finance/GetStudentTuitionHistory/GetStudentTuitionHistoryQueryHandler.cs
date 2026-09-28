using MediatR;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Finance.Common;
using VovinamERP.Application.Students.Common;
using VovinamERP.Domain.Finance;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.GetStudentTuitionHistory;

public sealed class GetStudentTuitionHistoryQueryHandler
    : IRequestHandler<
        GetStudentTuitionHistoryQuery,
        Result<GetStudentTuitionHistoryResult>>
{
    private readonly ITuitionInvoiceRepository _tuitionInvoiceRepository;
    private readonly IStudentCreditRepository _studentCreditRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IStudentRepository _studentRepository;

    public GetStudentTuitionHistoryQueryHandler(
        ITuitionInvoiceRepository tuitionInvoiceRepository,
        IStudentCreditRepository studentCreditRepository,
        IAttendanceRepository attendanceRepository,
        IStudentRepository studentRepository)
    {
        _tuitionInvoiceRepository = tuitionInvoiceRepository;
        _studentCreditRepository = studentCreditRepository;
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<GetStudentTuitionHistoryResult>> Handle(
        GetStudentTuitionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            return Result<GetStudentTuitionHistoryResult>.Failure(
                new Error(
                    "FIN_050",
                    "Tenant is required."));
        }

        if (request.StudentId == Guid.Empty)
        {
            return Result<GetStudentTuitionHistoryResult>.Failure(
                new Error(
                    "FIN_051",
                    "Student is required."));
        }

        if (request.Months < 3 || request.Months > 6)
        {
            return Result<GetStudentTuitionHistoryResult>.Failure(
                new Error(
                    "FIN_052",
                    "Months must be between 3 and 6."));
        }

        var student = await _studentRepository.GetByIdAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

        if (student is null)
        {
            return Result<GetStudentTuitionHistoryResult>.Failure(
                new Error(
                    "FIN_053",
                    "Student was not found."));
        }

        var invoices =
            await _tuitionInvoiceRepository.GetByStudentAsync(
                request.TenantId,
                request.StudentId,
                cancellationToken);

        var selectedInvoices = invoices
            .Take(request.Months)
            .ToList();

        var creditTransactions =
            await _studentCreditRepository.GetTransactionsAsync(
                request.TenantId,
                request.StudentId,
                cancellationToken);

        var debitTransactions = creditTransactions
            .Where(x =>
                x.TransactionType ==
                    StudentCreditTransactionType.Debit &&
                x.TuitionInvoiceId.HasValue)
            .ToList();

        var items =
            new List<StudentTuitionHistoryItem>();

        foreach (var invoice in selectedInvoices)
        {
            var attendanceCount =
                await _attendanceRepository
                    .CountStudentAttendancesByMonthAsync(
                        request.TenantId,
                        request.StudentId,
                        invoice.Year,
                        invoice.Month,
                        cancellationToken);

            var creditAppliedAmount = debitTransactions
                .Where(x =>
                    x.TuitionInvoiceId ==
                        invoice.Id)
                .Sum(x => x.Amount);

            var receiptPaidAmount = invoice.Payments
    .Sum(x => x.Amount);

            var finalPayableAmount =
    invoice.Amount
    - invoice.DiscountAmount
    - invoice.SpecialDiscountAmount;

            var totalSettledAmount =
    creditAppliedAmount +
    receiptPaidAmount;

           items.Add(
    new StudentTuitionHistoryItem(
        invoice.Id,
        invoice.InvoiceNumber,
        invoice.Year,
        invoice.Month,
        attendanceCount,
        invoice.Amount,
        invoice.DiscountAmount,
        invoice.SpecialDiscountType,
        invoice.SpecialDiscountValue,
        invoice.SpecialDiscountAmount,
        finalPayableAmount,
        creditAppliedAmount,
        receiptPaidAmount,
        totalSettledAmount,
        invoice.BalanceAmount,
        invoice.Status,
        invoice.SpecialDiscountReason,
        invoice.SpecialDiscountApprovedByUserId,
        invoice.SpecialDiscountApprovedAtUtc,
        invoice.Note));
        }

        return Result<GetStudentTuitionHistoryResult>.Success(
            new GetStudentTuitionHistoryResult(
                request.StudentId,
                request.Months,
                items));
    }
}