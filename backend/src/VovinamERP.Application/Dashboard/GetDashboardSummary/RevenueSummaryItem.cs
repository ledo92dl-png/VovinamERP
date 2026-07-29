namespace VovinamERP.Application.Dashboard.GetRevenueSummary;

public sealed record RevenueSummaryItem(
    string Period,
    decimal Revenue,
    int PaymentCount);