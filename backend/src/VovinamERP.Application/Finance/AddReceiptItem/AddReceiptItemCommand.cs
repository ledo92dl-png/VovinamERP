using MediatR;
using VovinamERP.Domain.Finance;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.AddReceiptItem;

public sealed record AddReceiptItemCommand(
    Guid TenantId,
Guid ReceiptId,
Guid? StudentId,
ReceiptItemType ItemType,
    Guid? ReferenceId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    string? Note)
    : IRequest<Result<AddReceiptItemResult>>;