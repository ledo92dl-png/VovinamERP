using MediatR;

namespace VovinamERP.Application.Dashboard.GetRevenueSummary;

public sealed class GetRevenueSummaryQueryHandler
    : IRequestHandler<
        GetRevenueSummaryQuery,
        GetRevenueSummaryResult>
{
    public Task<GetRevenueSummaryResult> Handle(
        GetRevenueSummaryQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<RevenueSummaryItem> items =
            Array.Empty<RevenueSummaryItem>();

        var result = new GetRevenueSummaryResult(
            items,
            TotalRevenue: 0m,
            TotalPayments: 0,
            request.FromDate,
            request.ToDate);

        return Task.FromResult(result);
    }
}