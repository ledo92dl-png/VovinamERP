
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class TuitionRefundConfiguration
    : IEntityTypeConfiguration<TuitionRefund>
{
    public void Configure(
        EntityTypeBuilder<TuitionRefund> builder)
    {
        builder.ToTable("TuitionRefunds");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.StudentId)
            .IsRequired();

        builder.Property(x => x.TuitionInvoiceId)
            .IsRequired();

        builder.Property(x => x.TuitionAdjustmentId)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.PaymentMethod)
            .HasConversion<int?>();

        builder.Property(x => x.TransactionReference)
            .HasMaxLength(200);

        builder.Property(x => x.EvidenceImageUrl)
            .HasMaxLength(2000);

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(500);

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.StudentId,
            x.Status
        });

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.TuitionInvoiceId
        });

        // Một lần điều chỉnh chỉ được tạo một phiếu hoàn tiền.
        builder.HasIndex(x => x.TuitionAdjustmentId)
            .IsUnique();
    }
}
