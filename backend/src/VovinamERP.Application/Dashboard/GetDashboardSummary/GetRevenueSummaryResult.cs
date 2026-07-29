namespace VovinamERP.Application.Dashboard.GetRevenueSummary;

public sealed record GetRevenueSummaryResult(
    IReadOnlyList<RevenueSummaryItem> Items,
    decimal TotalRevenue,
    int TotalPayments,
    DateOnly FromDate,
    DateOnly ToDate);