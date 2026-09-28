using MediatR;
using VovinamERP.Application.Finance.Common;
using VovinamERP.Application.Students.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.GetStudentCreditHistory;

public sealed class GetStudentCreditHistoryQueryHandler
    : IRequestHandler<
        GetStudentCreditHistoryQuery,
        Result<GetStudentCreditHistoryResult>>
{
    private readonly IStudentCreditRepository _studentCreditRepository;
    private readonly IStudentRepository _studentRepository;

    public GetStudentCreditHistoryQueryHandler(
        IStudentCreditRepository studentCreditRepository,
        IStudentRepository studentRepository)
    {
        _studentCreditRepository = studentCreditRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<GetStudentCreditHistoryResult>> Handle(
        GetStudentCreditHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            return Result<GetStudentCreditHistoryResult>.Failure(
                new Error(
                    "FIN_054",
                    "Tenant is required."));
        }

        if (request.StudentId == Guid.Empty)
        {
            return Result<GetStudentCreditHistoryResult>.Failure(
                new Error(
                    "FIN_055",
                    "Student is required."));
        }

        var student = await _studentRepository.GetByIdAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

        if (student is null)
        {
            return Result<GetStudentCreditHistoryResult>.Failure(
                new Error(
                    "FIN_056",
                    "Student was not found."));
        }

        var creditBalance =
            await _studentCreditRepository.GetBalanceAsync(
                request.TenantId,
                request.StudentId,
                cancellationToken);

        var transactions =
            await _studentCreditRepository.GetTransactionsAsync(
                request.TenantId,
                request.StudentId,
                cancellationToken);

        var items = transactions
            .Select(x =>
                new StudentCreditHistoryItem(
                    x.Id,
                    x.TransactionType,
                    x.Amount,
                    x.TransactionDate,
                    x.ReceiptId,
                    x.ReceiptItemId,
                    x.TuitionInvoiceId,
                    x.Description))
            .ToList();

        return Result<GetStudentCreditHistoryResult>.Success(
            new GetStudentCreditHistoryResult(
                request.StudentId,
                creditBalance,
                items));
    }
}