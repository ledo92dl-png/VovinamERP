namespace VovinamERP.Application.Dashboard.GetDashboardSummary;

public sealed record GetDashboardSummaryResult(
    Guid TenantId,
    DateOnly ReportDate,
    int ActiveStudents,
    int ActiveInstructors,
    int ActiveOrganizations,
    int TrainingSessions,
    int AttendanceCount,
    int CrossLocationAttendanceCount);