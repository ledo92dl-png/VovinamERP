using FluentValidation;

namespace VovinamERP.Application.Attendance.GetCrossLocationAttendanceDetails;

public sealed class GetCrossLocationAttendanceDetailsValidator
    : AbstractValidator<GetCrossLocationAttendanceDetailsQuery>
{
    private static readonly string[] AllowedSortFields =
    {
        "sessionDate",
        "markedAt",
        "fullName",
        "memberNumber",
        "trainingOrganizationName"
    };

    public GetCrossLocationAttendanceDetailsValidator()
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

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.SortBy)
            .Must(sortBy =>
                string.IsNullOrWhiteSpace(sortBy) ||
                AllowedSortFields.Contains(
                    sortBy.Trim(),
                    StringComparer.OrdinalIgnoreCase))
            .WithMessage(
                "SortBy must be one of: sessionDate, markedAt, fullName, memberNumber, trainingOrganizationName.");
    }
}