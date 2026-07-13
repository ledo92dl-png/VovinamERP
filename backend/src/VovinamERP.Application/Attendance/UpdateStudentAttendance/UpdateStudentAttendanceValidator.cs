using FluentValidation;

namespace VovinamERP.Application.Attendance.UpdateStudentAttendance;

public sealed class UpdateStudentAttendanceValidator
    : AbstractValidator<UpdateStudentAttendanceCommand>
{
    public UpdateStudentAttendanceValidator()
    {
        RuleFor(x => x.AttendanceRecordId)
            .NotEmpty();

        RuleFor(x => x.StudentId)
            .NotEmpty();

        RuleFor(x => x.MarkedByUserId)
            .NotEmpty();

        RuleFor(x => x.Note)
            .MaximumLength(500);
    }
}