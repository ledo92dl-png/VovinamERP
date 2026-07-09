using VovinamERP.Domain.Belts;

namespace VovinamERP.Api.Contracts.BeltRanks;

public sealed record BeltRankResponse(
    Guid Id,
    string BeltCode,
    string BeltName,
    int Level,
    string? Description,
    bool IsActive)
{
    public static BeltRankResponse FromDomain(BeltRank beltRank)
    {
        return new BeltRankResponse(
            beltRank.Id,
            beltRank.BeltCode,
            beltRank.BeltName,
            beltRank.Level,
            beltRank.Description,
            beltRank.IsActive);
    }
}
