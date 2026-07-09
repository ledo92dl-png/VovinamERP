using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.Belts;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class BeltRankConfiguration : IEntityTypeConfiguration<BeltRank>
{
    public void Configure(EntityTypeBuilder<BeltRank> builder)
    {
        builder.ToTable("belt_ranks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BeltCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.BeltName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.BeltCode).IsUnique();
    }
}
