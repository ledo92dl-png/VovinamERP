namespace VovinamERP.Api.Contracts.BeltRanks;

public sealed record CreateBeltRankRequest(
    string BeltCode,
    string BeltName,
    int Level,
    string? Description);
