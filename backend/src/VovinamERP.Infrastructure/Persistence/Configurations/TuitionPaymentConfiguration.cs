using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class TuitionPaymentConfiguration
    : IEntityTypeConfiguration<TuitionPayment>
{
    public void Configure(
        EntityTypeBuilder<TuitionPayment> builder)
    {
        builder.ToTable("TuitionPayments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.PaymentNumber)
            .IsRequired();

        builder.HasIndex(x => x.TuitionInvoiceId);

        builder.HasIndex(x => x.ReceiptItemId)
            .IsUnique();

        builder.HasOne<TuitionInvoice>()
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.TuitionInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}