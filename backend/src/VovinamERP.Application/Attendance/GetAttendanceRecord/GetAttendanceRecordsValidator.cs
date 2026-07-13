using FluentValidation;

namespace VovinamERP.Application.Attendance.GetAttendanceRecords;

public sealed class GetAttendanceRecordsValidator
    : AbstractValidator<GetAttendanceRecordsQuery>
{
    public GetAttendanceRecordsValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}