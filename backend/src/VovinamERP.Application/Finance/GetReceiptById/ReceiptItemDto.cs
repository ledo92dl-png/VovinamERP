namespace VovinamERP.Application.Finance.GetReceiptById;

public sealed record ReceiptItemDto(
    Guid Id,
    string ItemType,
    Guid? ReferenceId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TotalAmount,
    string? Note);