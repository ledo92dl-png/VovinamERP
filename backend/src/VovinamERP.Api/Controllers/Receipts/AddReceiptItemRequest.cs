using VovinamERP.Domain.Finance;

namespace VovinamERP.Api.Controllers.Receipts;

public sealed record AddReceiptItemRequest(
    Guid TenantId,
    Guid? StudentId,
    ReceiptItemType ItemType,
    Guid? ReferenceId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    string? Note);