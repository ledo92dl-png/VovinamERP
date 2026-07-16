using FluentValidation;

namespace VovinamERP.Application.Attendance.ScanStudentQr;

public sealed class ScanStudentQrValidator
    : AbstractValidator<ScanStudentQrCommand>
{
    public ScanStudentQrValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.AttendanceRecordId)
            .NotEmpty();

        RuleFor(x => x.MarkedByUserId)
            .NotEmpty();

        RuleFor(x => x.QrContent)
            .NotEmpty()
            .MaximumLength(256);
    }
}