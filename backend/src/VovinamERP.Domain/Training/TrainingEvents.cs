using VovinamERP.SharedKernel.Common;

namespace VovinamERP.Domain.Training;

public sealed record TrainingClassCreatedEvent(
    Guid TrainingClassId,
    Guid TenantId,
    string Code,
    string Name
) : DomainEvent;

public sealed record StudentEnrolledInClassEvent(
    Guid EnrollmentId,
    Guid TrainingClassId,
    Guid StudentId
) : DomainEvent;

public sealed record StudentEnrollmentStatusChangedEvent(
    Guid EnrollmentId,
    Guid StudentId,
    Guid TrainingClassId,
    EnrollmentStatus OldStatus,
    EnrollmentStatus NewStatus,
    string? Reason
) : DomainEvent;

public sealed record StudentEnrollmentEndedEvent(
    Guid EnrollmentId,
    Guid StudentId,
    Guid TrainingClassId,
    EnrollmentStatus EndStatus,
    DateOnly EndDate,
    string? Reason
) : DomainEvent;

public sealed record TrainingSessionCreatedEvent(
    Guid TrainingSessionId,
    Guid TrainingClassId,
    DateOnly SessionDate
) : DomainEvent;

public sealed record TrainingSessionOpenedEvent(
    Guid TrainingSessionId
) : DomainEvent;

public sealed record TrainingSessionClosedEvent(
    Guid TrainingSessionId
) : DomainEvent;

public sealed record SessionInstructorAssignedEvent(
    Guid SessionInstructorId,
    Guid TrainingSessionId,
    Guid InstructorId
) : DomainEvent;

public sealed record AttendanceRecordCreatedEvent(
    Guid AttendanceRecordId,
    Guid TrainingSessionId
) : DomainEvent;

public sealed record AttendanceDetailMarkedEvent(
    Guid AttendanceDetailId,
    Guid AttendanceRecordId,
    Guid StudentId,
    AttendanceStatus Status
) : DomainEvent;
