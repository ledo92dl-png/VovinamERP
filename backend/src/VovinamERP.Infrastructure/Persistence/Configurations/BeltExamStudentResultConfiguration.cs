using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.BeltExams;
using VovinamERP.Domain.Students;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class BeltExamStudentResultConfiguration
    : IEntityTypeConfiguration<BeltExamStudentResult>
{
    public void Configure(EntityTypeBuilder<BeltExamStudentResult> builder)
    {
        builder.ToTable("belt_exam_student_results");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.BeltExamId)
            .IsRequired();

        builder.Property(x => x.StudentId)
            .IsRequired();

        builder.Property(x => x.UnitName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.SourceResult)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Result)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.TotalScore)
            .HasPrecision(10, 2);

        builder.Property(x => x.Ranking);

        builder.Property(x => x.Note)
            .HasMaxLength(1024);

        builder.HasOne<BeltExam>()
            .WithMany()
            .HasForeignKey(x => x.BeltExamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.BeltExamId,
            x.StudentId
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.StudentId
        });

        builder.HasIndex(x => x.BeltExamId);
        builder.HasIndex(x => x.StudentId);
    }
}