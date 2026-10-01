using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.BeltExams;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class BeltExamScoreConfiguration
    : IEntityTypeConfiguration<BeltExamScore>
{
    public void Configure(EntityTypeBuilder<BeltExamScore> builder)
    {
        builder.ToTable("belt_exam_scores");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.BeltExamStudentResultId)
            .IsRequired();

        builder.Property(x => x.BeltExamSubjectId)
            .IsRequired();

        builder.Property(x => x.Score)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.HasOne<BeltExamStudentResult>()
            .WithMany()
            .HasForeignKey(x => x.BeltExamStudentResultId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BeltExamSubject>()
            .WithMany()
            .HasForeignKey(x => x.BeltExamSubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.BeltExamStudentResultId,
            x.BeltExamSubjectId
        })
        .IsUnique();

        builder.HasIndex(x => x.BeltExamStudentResultId);
        builder.HasIndex(x => x.BeltExamSubjectId);
    }
}