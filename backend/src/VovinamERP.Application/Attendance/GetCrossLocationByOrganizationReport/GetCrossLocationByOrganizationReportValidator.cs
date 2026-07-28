using FluentValidation;

namespace VovinamERP.Application.Attendance.GetCrossLocationByOrganizationReport;

public sealed class GetCrossLocationByOrganizationReportValidator
    : AbstractValidator<GetCrossLocationByOrganizationReportQuery>
{
    public GetCrossLocationByOrganizationReportValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x)
            .Must(x =>
                !x.FromDate.HasValue ||
                !x.ToDate.HasValue ||
                x.FromDate.Value <= x.ToDate.Value)
            .WithMessage(
                "FromDate must be earlier than or equal to ToDate.");
    }
}