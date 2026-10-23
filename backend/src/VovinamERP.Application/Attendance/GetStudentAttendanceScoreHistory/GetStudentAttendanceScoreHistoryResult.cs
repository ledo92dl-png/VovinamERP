namespace VovinamERP.Application.Attendance.GetStudentAttendanceScoreHistory;

public sealed record GetStudentAttendanceScoreHistoryResult(
    Guid StudentId,
    DateOnly FromDate,
    DateOnly ToDate,
    IReadOnlyList<StudentAttendanceScoreMonthResult> Months);

public sealed record StudentAttendanceScoreMonthResult(
    int Year,
    int Month,
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
