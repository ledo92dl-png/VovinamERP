namespace VovinamERP.Application.Dashboard.GetAttendanceTrend;

public sealed record AttendanceTrendItem(
    DateOnly ReportDate,
    int TrainingSessions,
    int AttendanceCount,
    int CrossLocationAttendanceCount);