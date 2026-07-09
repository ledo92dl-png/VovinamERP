namespace VovinamERP.Application.Students.RegisterStudent;

public sealed record RegisterStudentResponse(
    Guid PersonId,
    Guid StudentId,
    string PersonCode,
    string MemberNumber,
    string FullName,
    DateOnly EnrollmentDate);
