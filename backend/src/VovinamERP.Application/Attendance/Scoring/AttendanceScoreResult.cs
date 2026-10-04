namespace VovinamERP.Application.Attendance.Scoring;

public sealed record AttendanceScoreResult(
    int PresentCount,
    int LateCount,
    int ExcusedCount,
    int AbsentCount,
    int EligibleSessionCount,
    int AttendedSessionCount,
    decimal AttendanceRate,
    decimal Score);
