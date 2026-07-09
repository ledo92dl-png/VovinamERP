namespace VovinamERP.Api.Contracts.BeltRanks;

public sealed record UpdateBeltRankRequest(
    string BeltName,
    int Level,
    string? Description);
