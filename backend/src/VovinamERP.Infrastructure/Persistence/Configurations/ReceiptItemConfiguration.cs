using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class ReceiptItemConfiguration
    : IEntityTypeConfiguration<ReceiptItem>
{
    public void Configure(EntityTypeBuilder<ReceiptItem> builder)
    {
        builder.ToTable("receipt_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.ReceiptId)
            .IsRequired();

        builder.Property(x => x.StudentId);

        builder.Property(x => x.ReferenceId);

        builder.Property(x => x.ItemType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.Quantity)
    .HasPrecision(18, 2)
    .IsRequired();

        builder.Property(x => x.UnitPrice)
    .HasPrecision(18, 2)
    .IsRequired();

        builder.Property(x => x.DiscountAmount)
    .HasPrecision(18, 2)
    .HasDefaultValue(0);

        builder.Ignore(x => x.GrossAmount);

        builder.Ignore(x => x.TotalAmount);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.HasIndex(x => x.ReceiptId);

        builder.HasIndex(x => new
{
    x.TenantId,
    x.ReceiptId
});

        builder.HasIndex(x => new
        {
         x.ReceiptId,
          x.ItemType
        });

        builder.HasIndex(x => x.StudentId);

builder.HasIndex(x => new
{
    x.TenantId,
    x.StudentId
});

    }
}