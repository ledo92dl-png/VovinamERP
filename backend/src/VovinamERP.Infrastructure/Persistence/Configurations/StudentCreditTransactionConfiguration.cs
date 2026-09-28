using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class StudentCreditTransactionConfiguration
    : IEntityTypeConfiguration<StudentCreditTransaction>
{
    public void Configure(
        EntityTypeBuilder<StudentCreditTransaction> builder)
    {
        builder.ToTable("StudentCreditTransactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.TransactionType)
            .IsRequired();

        builder.Property(x => x.TransactionDate)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.StudentId,
            x.TransactionDate
        });

        builder.HasIndex(x => x.ReceiptId);

        builder.HasIndex(x => x.ReceiptItemId)
    .IsUnique();

        builder.HasIndex(x => x.TuitionInvoiceId);

        
builder.Property(x => x.TuitionAdjustmentId)
    .IsRequired(false);

builder.HasIndex(x => x.TuitionAdjustmentId)
    .IsUnique()
    .HasFilter("\"TuitionAdjustmentId\" IS NOT NULL");
    }
}