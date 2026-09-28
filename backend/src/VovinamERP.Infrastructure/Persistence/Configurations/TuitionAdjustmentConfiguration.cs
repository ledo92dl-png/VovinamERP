
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class TuitionAdjustmentConfiguration
    : IEntityTypeConfiguration<TuitionAdjustment>
{
    public void Configure(
        EntityTypeBuilder<TuitionAdjustment> builder)
    {
        builder.ToTable("TuitionAdjustments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.StudentId)
            .IsRequired();

        builder.Property(x => x.TuitionInvoiceId)
            .IsRequired();

        builder.Property(x => x.PreviousPayableAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.NewPayableAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.AdjustmentAmount)
            .HasPrecision(18, 2)
            .IsRequired();

            
builder.Property(x => x.SettlementAmount)
    .HasColumnType("decimal(18,2)")
    .HasDefaultValue(0m)
    .IsRequired();

        builder.Property(x => x.SettlementType)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ApprovedByUserId)
            .IsRequired();

        builder.Property(x => x.ApprovedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.StudentId,
            x.ApprovedAtUtc
        });

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.TuitionInvoiceId
        });
    }
}