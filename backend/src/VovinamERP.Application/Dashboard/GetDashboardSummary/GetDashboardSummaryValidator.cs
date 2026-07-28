using FluentValidation;

namespace VovinamERP.Application.Dashboard.GetDashboardSummary;

public sealed class GetDashboardSummaryValidator
    : AbstractValidator<GetDashboardSummaryQuery>
{
    public GetDashboardSummaryValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.ReportDate)
            .NotEqual(default(DateOnly));
    }
}