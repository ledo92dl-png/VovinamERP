using FluentValidation;

namespace VovinamERP.Application.Finance.AddReceiptItem;

public sealed class AddReceiptItemCommandValidator
    : AbstractValidator<AddReceiptItemCommand>
{
    public AddReceiptItemCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.ReceiptId)
            .NotEmpty();

        RuleFor(x => x.ItemType)
            .IsInEnum();

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x)
            .Must(x =>
                x.DiscountAmount <=
                x.Quantity * x.UnitPrice)
            .WithMessage(
                "Discount amount cannot exceed gross amount.");

        RuleFor(x => x.Note)
            .MaximumLength(500);
    }
}