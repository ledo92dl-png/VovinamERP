namespace VovinamERP.Application.Attendance.Scoring;

public sealed record AttendanceScorePolicy(
    decimal PresentWeight,
    decimal LateWeight,
    int MaxRequiredSessionsPerWeek)
{
    public static AttendanceScorePolicy Default { get; } = new(
        PresentWeight: 1m,
        LateWeight: 1m,
        MaxRequiredSessionsPerWeek: 3);
}
