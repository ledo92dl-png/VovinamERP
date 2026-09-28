namespace VovinamERP.Application.Finance.GetReceipts;

public sealed record GetReceiptsResult(
    IReadOnlyCollection<ReceiptListItemDto> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);