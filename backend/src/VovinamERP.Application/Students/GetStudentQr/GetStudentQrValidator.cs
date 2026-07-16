using FluentValidation;

namespace VovinamERP.Application.Students.GetStudentQr;

public sealed class GetStudentQrValidator
    : AbstractValidator<GetStudentQrQuery>
{
    public GetStudentQrValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.StudentId)
            .NotEmpty();
    }
}