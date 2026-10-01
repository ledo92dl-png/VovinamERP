using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.BeltExams;
using VovinamERP.Domain.Belts;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class BeltExamConfiguration
    : IEntityTypeConfiguration<BeltExam>
{
    public void Configure(EntityTypeBuilder<BeltExam> builder)
    {
        builder.ToTable("belt_exams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.TargetBeltRankId)
            .IsRequired();

        builder.Property(x => x.ExamDate)
            .IsRequired();

        builder.Property(x => x.SessionName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Location)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.SourceBeltName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(1024);

        builder.HasOne<BeltRank>()
            .WithMany()
            .HasForeignKey(x => x.TargetBeltRankId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.ExamDate
        });

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.TargetBeltRankId,
            x.ExamDate
        });

        builder.HasIndex(x => x.TargetBeltRankId);
    }
}