using FluentValidation;

namespace VovinamERP.Application.Attendance.CompleteAttendanceRecord;

public sealed class CompleteAttendanceRecordValidator
    : AbstractValidator<CompleteAttendanceRecordCommand>
{
    public CompleteAttendanceRecordValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.AttendanceRecordId)
            .NotEmpty();

        RuleFor(x => x.CompletedByUserId)
            .NotEmpty();
    }
}