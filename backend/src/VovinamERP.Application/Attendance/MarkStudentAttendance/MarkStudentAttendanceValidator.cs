using FluentValidation;

namespace VovinamERP.Application.Attendance.MarkStudentAttendance;

public sealed class MarkStudentAttendanceValidator
    : AbstractValidator<MarkStudentAttendanceCommand>
{
    public MarkStudentAttendanceValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.AttendanceRecordId)
            .NotEmpty();

        RuleFor(x => x.StudentId)
            .NotEmpty();

        RuleFor(x => x.MarkedByUserId)
            .NotEmpty();

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Method)
            .IsInEnum();

        RuleFor(x => x.Source)
            .IsInEnum();

        RuleFor(x => x.Note)
            .MaximumLength(500);
    }
}