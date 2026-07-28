using FluentValidation;

namespace VovinamERP.Application.Dashboard.GetStudentsByBelt;

public sealed class GetStudentsByBeltValidator
    : AbstractValidator<GetStudentsByBeltQuery>
{
    public GetStudentsByBeltValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}