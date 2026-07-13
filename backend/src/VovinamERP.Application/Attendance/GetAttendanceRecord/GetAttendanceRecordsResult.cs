namespace VovinamERP.Application.Attendance.GetAttendanceRecords;

public sealed record GetAttendanceRecordsResult(
    IReadOnlyList<AttendanceRecordListItem> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);