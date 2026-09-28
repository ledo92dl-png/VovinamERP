
using FluentValidation;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.AdjustPaidTuition;

public sealed class AdjustPaidTuitionCommandValidator
    : AbstractValidator<AdjustPaidTuitionCommand>
{
    public AdjustPaidTuitionCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.TuitionInvoiceId)
            .NotEmpty();

        RuleFor(x => x.NewSpecialDiscountAmount)
            .GreaterThan(0);

        RuleFor(x => x.SettlementType)
            .Must(x => Enum.IsDefined(x))
            .WithMessage("Invalid settlement type.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.ApprovedByUserId)
            .NotEmpty();
    }
}