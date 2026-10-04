namespace VovinamERP.Application.Attendance.GetStudentAttendanceScore;

public sealed record GetStudentAttendanceScoreResult(
    Guid StudentId,
    DateOnly FromDate,
    DateOnly ToDate,
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
