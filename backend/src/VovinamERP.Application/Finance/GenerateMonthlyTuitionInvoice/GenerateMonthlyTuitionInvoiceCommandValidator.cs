using FluentValidation;

namespace VovinamERP.Application.Finance.GenerateMonthlyTuitionInvoice;

public sealed class GenerateMonthlyTuitionInvoiceCommandValidator
    : AbstractValidator<GenerateMonthlyTuitionInvoiceCommand>
{
    public GenerateMonthlyTuitionInvoiceCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.StudentId)
            .NotEmpty();

        RuleFor(x => x.InvoiceNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Year)
            .GreaterThanOrEqualTo(2000);

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12);

        RuleFor(x => x.StandardAmount)
            .GreaterThan(0);
    }
}