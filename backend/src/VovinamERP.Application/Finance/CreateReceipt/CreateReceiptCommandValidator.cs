using FluentValidation;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.CreateReceipt;

public sealed class CreateReceiptCommandValidator
    : AbstractValidator<CreateReceiptCommand>
{
    public CreateReceiptCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant is required.");

        RuleFor(x => x.CollectedByUserId)
            .NotEmpty()
            .WithMessage("Collector is required.");

        RuleFor(x => x.ReceiptNumber)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage(
                "Receipt number is required and must not exceed 50 characters.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("Payment method is invalid.");

        RuleFor(x => x.ReceiptDate)
            .NotEqual(default(DateOnly))
            .WithMessage("Receipt date is required.");

        RuleFor(x => x.TransactionReference)
            .MaximumLength(100);

        RuleFor(x => x.EvidenceImageUrl)
            .MaximumLength(500);

        RuleFor(x => x.Note)
            .MaximumLength(500);

        When(
            x => x.PaymentMethod == PaymentMethod.BankTransfer,
            () =>
            {
                RuleFor(x => x.TransactionReference)
                    .NotEmpty()
                    .WithMessage(
                        "Transaction reference is required for bank transfer.");
            });
    }
}