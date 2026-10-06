namespace VovinamERP.Application.Attendance.Scoring;

public sealed record StudentAttendanceScoreCalculationResult(
    int RequiredSessionCount,
    int ActualAttendedSessionCount,
    int PresentCount,
    int LateCount,
    int ExcusedCount,
    int AbsentCount,
    int CrossLocationAttendanceCount,
    int ExtraAttendanceCount,
    decimal? AttendanceRate,
    decimal? Score);
