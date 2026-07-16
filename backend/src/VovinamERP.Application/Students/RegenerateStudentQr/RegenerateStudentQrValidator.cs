using FluentValidation;

namespace VovinamERP.Application.Students.RegenerateStudentQr;

public sealed class RegenerateStudentQrValidator
    : AbstractValidator<RegenerateStudentQrCommand>
{
    public RegenerateStudentQrValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.StudentId)
            .NotEmpty();

        RuleFor(x => x.RegeneratedByUserId)
            .NotEmpty();
    }
}