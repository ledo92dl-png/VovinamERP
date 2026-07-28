namespace VovinamERP.Application.Attendance.GetCrossLocationAttendanceReport;

public sealed record GetCrossLocationAttendanceReportResult(
    int TotalAttendances,
    int CrossLocationAttendances,
    int NormalAttendances,
    decimal CrossLocationRate);