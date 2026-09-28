using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class ReceiptConfiguration
    : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("receipts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.CollectedByUserId)
            .IsRequired();

        builder.Property(x => x.ReceiptNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ReceiptDate)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.TransactionReference)
            .HasMaxLength(100);

        builder.Property(x => x.EvidenceImageUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.ReceiptNumber
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.ReceiptDate
        });

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.CollectedByUserId
        });

        builder.Navigation(x => x.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}