namespace VovinamERP.Application.Attendance.GetCrossLocationByOrganizationReport;

public sealed record CrossLocationOrganizationItem(
    Guid OrganizationId,
    string OrganizationName,
    int TotalAttendances,
    int CrossLocationAttendances,
    decimal CrossLocationRate);