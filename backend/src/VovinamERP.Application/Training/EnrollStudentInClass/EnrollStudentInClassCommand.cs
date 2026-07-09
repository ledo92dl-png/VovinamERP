using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Training.EnrollStudentInClass;

public sealed record EnrollStudentInClassCommand(
    Guid TenantId,
    Guid StudentId,
    Guid TrainingClassId,
    DateOnly StartDate,
    EnrollmentStatus Status,
    string? Note);
