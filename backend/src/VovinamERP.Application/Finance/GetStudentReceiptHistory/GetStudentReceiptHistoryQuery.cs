using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.GetStudentReceiptHistory;

public sealed record GetStudentReceiptHistoryQuery(
    Guid TenantId,
    Guid StudentId,
    DateOnly? FromDate,
    DateOnly? ToDate,
    string? ItemType,
    string? ReceiptStatus,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<Result<GetStudentReceiptHistoryResult>>;