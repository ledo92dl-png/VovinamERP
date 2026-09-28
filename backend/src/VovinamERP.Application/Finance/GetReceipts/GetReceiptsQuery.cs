using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.GetReceipts;

public sealed record GetReceiptsQuery(
    Guid TenantId,
    DateOnly? FromDate,
    DateOnly? ToDate,
    string? Status,
    string? PaymentMethod,
    Guid? CollectedByUserId,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<Result<GetReceiptsResult>>;