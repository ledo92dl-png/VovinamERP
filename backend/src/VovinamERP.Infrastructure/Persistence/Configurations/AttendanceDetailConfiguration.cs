using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.Training;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class AttendanceDetailConfiguration
    : IEntityTypeConfiguration<AttendanceDetail>
{
    public void Configure(EntityTypeBuilder<AttendanceDetail> builder)
    {
        builder.ToTable("attendance_details");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.AttendanceRecordId)
            .IsRequired();

        builder.Property(x => x.StudentId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Method)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.MarkedAt)
            .IsRequired();

        builder.Property(x => x.MarkedByUserId)
            .IsRequired();

        builder.Property(x => x.IsBackfilled)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.HasIndex(x => new
        {
            x.AttendanceRecordId,
            x.StudentId
        })
        .IsUnique();

        builder.HasIndex(x => x.TenantId);

        builder.HasIndex(x => x.StudentId);

        builder.HasIndex(x => x.MarkedAt);
    }
}