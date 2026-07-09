using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Training.EnrollStudentInClass;

public sealed record EnrollStudentInClassResponse(
    Guid EnrollmentId,
    Guid StudentId,
    Guid TrainingClassId,
    DateOnly StartDate,
    EnrollmentStatus Status);
