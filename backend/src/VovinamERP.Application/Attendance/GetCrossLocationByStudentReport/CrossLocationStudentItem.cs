namespace VovinamERP.Application.Attendance.GetCrossLocationByStudentReport;

public sealed record CrossLocationStudentItem(
    Guid StudentId,
    string MemberNumber,
    string FullName,
    string HomeOrganizationName,
    int TotalAttendances,
    int CrossLocationAttendances,
    decimal CrossLocationRate);