namespace VovinamERP.Application.Attendance.GetStudentMonthlyAttendance;

public sealed record GetStudentMonthlyAttendanceResult(
    Guid StudentId,
    int Year,
    int Month,
    int AttendanceCount);