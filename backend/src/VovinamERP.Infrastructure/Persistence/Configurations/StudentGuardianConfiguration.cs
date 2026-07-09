using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.Guardians;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class StudentGuardianConfiguration : IEntityTypeConfiguration<StudentGuardian>
{
    public void Configure(EntityTypeBuilder<StudentGuardian> builder)
    {
        builder.ToTable("student_guardians");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Relationship)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Note).HasMaxLength(1024);

        builder.HasIndex(x => new { x.TenantId, x.StudentId, x.GuardianId }).IsUnique();
        builder.HasIndex(x => x.StudentId);
        builder.HasIndex(x => x.GuardianId);
        builder.HasIndex(x => new { x.TenantId, x.StudentId, x.IsPrimary });
    }
}
