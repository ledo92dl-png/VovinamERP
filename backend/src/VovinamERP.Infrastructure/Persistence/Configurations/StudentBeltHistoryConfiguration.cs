using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.Belts;
using VovinamERP.Domain.Students;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class StudentBeltHistoryConfiguration
    : IEntityTypeConfiguration<StudentBeltHistory>
{
    public void Configure(EntityTypeBuilder<StudentBeltHistory> builder)
    {
        builder.ToTable("student_belt_histories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Result)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ExamDate)
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

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.StudentId,
            x.ExamDate
        });

        builder.HasIndex(x => x.StudentId);
        builder.HasIndex(x => x.BeltRankId);
    }
}