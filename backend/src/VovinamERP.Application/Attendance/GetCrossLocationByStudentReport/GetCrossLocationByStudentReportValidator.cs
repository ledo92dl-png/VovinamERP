using FluentValidation;

namespace VovinamERP.Application.Attendance.GetCrossLocationByStudentReport;

public sealed class GetCrossLocationByStudentReportValidator
    : AbstractValidator<GetCrossLocationByStudentReportQuery>
{
    public GetCrossLocationByStudentReportValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x)
            .Must(x =>
                !x.FromDate.HasValue ||
                !x.ToDate.HasValue ||
                x.FromDate <= x.ToDate)
            .WithMessage(
                "FromDate must be earlier than or equal to ToDate.");
    }
}