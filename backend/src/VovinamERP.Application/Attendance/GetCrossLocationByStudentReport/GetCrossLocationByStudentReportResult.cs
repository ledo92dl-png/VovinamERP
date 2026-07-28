namespace VovinamERP.Application.Attendance.GetCrossLocationByStudentReport;

public sealed record GetCrossLocationByStudentReportResult(
    IReadOnlyList<CrossLocationStudentItem> Items);