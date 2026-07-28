namespace VovinamERP.Application.Dashboard.GetStudentsByBelt;

public sealed record GetStudentsByBeltResult(
    IReadOnlyList<StudentsByBeltItem> Items,
    int TotalStudents);