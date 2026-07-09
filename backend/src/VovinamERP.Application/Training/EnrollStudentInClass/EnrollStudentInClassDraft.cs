using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Training.EnrollStudentInClass;

public sealed record EnrollStudentInClassDraft(
    StudentClassEnrollment Enrollment,
    EnrollStudentInClassResponse Response);
