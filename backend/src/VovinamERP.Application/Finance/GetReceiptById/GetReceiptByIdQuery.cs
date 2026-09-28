using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.GetReceiptById;

public sealed record GetReceiptByIdQuery(
    Guid TenantId,
    Guid ReceiptId)
    : IRequest<Result<ReceiptDetailsDto>>;