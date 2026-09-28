namespace VovinamERP.Application.Finance.GetStudentReceiptHistory;

public sealed record StudentReceiptHistoryItemDto(
    Guid ReceiptId,
    Guid ReceiptItemId,
    string ReceiptNumber,
    DateOnly ReceiptDate,
    string ItemType,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TotalAmount,
    string PaymentMethod,
    string ReceiptStatus,
    Guid CollectedByUserId,
    string? Note);