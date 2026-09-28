using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class TuitionInvoiceConfiguration
    : IEntityTypeConfiguration<TuitionInvoice>
{
    public void Configure(EntityTypeBuilder<TuitionInvoice> builder)
    {
        builder.ToTable("TuitionInvoices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SpecialDiscountAmount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m);
        
        
builder.Property(x => x.ReallocatedAmount)
    .HasColumnType("decimal(18,2)")
    .HasDefaultValue(0m);

        builder.Property(x => x.SpecialDiscountType)
            .HasConversion<int?>();

        builder.Property(x => x.SpecialDiscountValue)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.SpecialDiscountReason)
            .HasMaxLength(500);

        builder.Property(x => x.SpecialDiscountApprovedByUserId);

        builder.Property(x => x.SpecialDiscountApprovedAtUtc);

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.StudentId,
            x.Year,
            x.Month
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.InvoiceNumber
        })
        .IsUnique();
    }
}