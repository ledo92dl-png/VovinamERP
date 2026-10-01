using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.BeltRecognitions;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class BeltRankDocumentConfiguration
    : IEntityTypeConfiguration<BeltRankDocument>
{
    public void Configure(
        EntityTypeBuilder<BeltRankDocument> builder)
    {
        builder.ToTable("belt_rank_documents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.BeltRankRecognitionId)
            .IsRequired();

        builder.Property(x => x.DocumentType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.DocumentNumber)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.SignedDate)
            .IsRequired();

        builder.Property(x => x.ScanUrl)
            .HasMaxLength(2000);

        builder.Property(x => x.Note)
            .HasMaxLength(1024);

        builder.HasOne<BeltRankRecognition>()
            .WithMany()
            .HasForeignKey(x => x.BeltRankRecognitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.BeltRankRecognitionId
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.DocumentNumber
        });

        builder.HasIndex(x => x.BeltRankRecognitionId);
    }
}