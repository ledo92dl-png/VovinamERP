using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.Belts;
using VovinamERP.Domain.BeltExams;
using VovinamERP.Domain.BeltRecognitions;
using VovinamERP.Domain.Students;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class BeltRankRecognitionConfiguration
    : IEntityTypeConfiguration<BeltRankRecognition>
{
    public void Configure(
        EntityTypeBuilder<BeltRankRecognition> builder)
    {
        builder.ToTable("belt_rank_recognitions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.StudentId)
            .IsRequired();

        builder.Property(x => x.BeltRankId)
            .IsRequired();

        builder.Property(x => x.RecognitionDate)
            .IsRequired();

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(1024);

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BeltRank>()
            .WithMany()
            .HasForeignKey(x => x.BeltRankId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BeltExamStudentResult>()
            .WithMany()
            .HasForeignKey(x => x.BeltExamStudentResultId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.StudentId,
            x.RecognitionDate
        });

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.StudentId
        });

        builder.HasIndex(x => x.BeltRankId);

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.BeltExamStudentResultId
        })
        .IsUnique();
    }
}
