namespace VovinamERP.Application.Finance.AddReceiptItem;

public sealed record AddReceiptItemResult(
    Guid ReceiptId,
    Guid ReceiptItemId,
    decimal ItemTotalAmount,
    decimal ReceiptTotalAmount);