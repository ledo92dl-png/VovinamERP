using FluentValidation;

namespace VovinamERP.Application.Attendance.CreateAttendanceRecord;

public sealed class CreateAttendanceRecordValidator
    : AbstractValidator<CreateAttendanceRecordCommand>
{
    public CreateAttendanceRecordValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.TrainingSessionId)
            .NotEmpty();

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty();
    }
}