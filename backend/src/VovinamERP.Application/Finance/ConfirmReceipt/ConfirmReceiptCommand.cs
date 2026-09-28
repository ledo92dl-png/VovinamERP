using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.ConfirmReceipt;

public sealed record ConfirmReceiptCommand(
    Guid TenantId,
    Guid ReceiptId)
    : IRequest<Result>;