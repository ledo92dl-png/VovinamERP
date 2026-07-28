namespace VovinamERP.Application.Attendance.GetCrossLocationAttendanceDetails;

public sealed record GetCrossLocationAttendanceDetailsResult(
    IReadOnlyList<CrossLocationAttendanceDetailItem> Items);