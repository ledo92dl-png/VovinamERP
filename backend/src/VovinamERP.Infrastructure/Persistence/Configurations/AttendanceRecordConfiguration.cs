using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.Training;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class AttendanceRecordConfiguration
    : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("attendance_records");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.TrainingSessionId)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.TrainingSessionId
        })
        .IsUnique();

        builder.HasMany(x => x.Details)
            .WithOne()
            .HasForeignKey(x => x.AttendanceRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Details)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Property(x => x.Status)
    .HasConversion<string>()
    .HasMaxLength(32)
    .IsRequired();

builder.Property(x => x.CompletedAt);

builder.Property(x => x.CompletedByUserId);
    }
}