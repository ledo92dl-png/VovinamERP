namespace VovinamERP.Application.Attendance.GetCrossLocationByOrganizationReport;

public sealed record GetCrossLocationByOrganizationReportResult(
    IReadOnlyList<CrossLocationOrganizationItem> Items);