namespace VovinamERP.Application.Dashboard.GetStudentsByBelt;

public sealed record StudentsByBeltItem(
    Guid? BeltRankId,
    string BeltCode,
    string BeltName,
    int StudentCount,
    decimal Percentage);