namespace VovinamERP.Application.Dashboard.GetAttendanceTrend;

public sealed record GetAttendanceTrendResult(
    IReadOnlyList<AttendanceTrendItem> Items,
    DateOnly FromDate,
    DateOnly ToDate);