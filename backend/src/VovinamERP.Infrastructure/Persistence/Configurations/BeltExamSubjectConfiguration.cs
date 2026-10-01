using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.BeltExams;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class BeltExamSubjectConfiguration
    : IEntityTypeConfiguration<BeltExamSubject>
{
    public void Configure(EntityTypeBuilder<BeltExamSubject> builder)
    {
        builder.ToTable("belt_exam_subjects");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.BeltExamId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.MaximumScore)
            .HasPrecision(10, 2);

        builder.HasOne<BeltExam>()
            .WithMany()
            .HasForeignKey(x => x.BeltExamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.BeltExamId,
            x.DisplayOrder
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.BeltExamId,
            x.Name
        })
        .IsUnique();

        builder.HasIndex(x => x.BeltExamId);
    }
}