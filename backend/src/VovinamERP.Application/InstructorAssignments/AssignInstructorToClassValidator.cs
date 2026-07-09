using FluentValidation;

namespace VovinamERP.Application.InstructorAssignments.AssignInstructorToClass;

public sealed class AssignInstructorToClassValidator
    : AbstractValidator<AssignInstructorToClassCommand>
{
    public AssignInstructorToClassValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.TrainingClassId)
            .NotEmpty();

        RuleFor(x => x.InstructorId)
            .NotEmpty();

        RuleFor(x => x.StartDate)
            .NotEmpty();

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue);
    }
}