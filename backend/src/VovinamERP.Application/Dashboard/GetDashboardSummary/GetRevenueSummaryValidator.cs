using FluentValidation;

namespace VovinamERP.Application.Dashboard.GetRevenueSummary;

public sealed class GetRevenueSummaryValidator
    : AbstractValidator<GetRevenueSummaryQuery>
{
    public GetRevenueSummaryValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.FromDate)
            .NotEqual(default(DateOnly));

        RuleFor(x => x.ToDate)
            .NotEqual(default(DateOnly));

        RuleFor(x => x)
            .Must(x => x.FromDate <= x.ToDate)
            .WithMessage(
                "FromDate must be earlier than or equal to ToDate.");

        RuleFor(x => x)
            .Must(x =>
                x.ToDate.DayNumber - x.FromDate.DayNumber <= 366)
            .WithMessage(
                "The report date range must not exceed 366 days.");
    }
}