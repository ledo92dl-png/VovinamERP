using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Training;

public sealed class AttendanceDetail : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid AttendanceRecordId { get; private set; }
    public Guid StudentId { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public string? Note { get; private set; }
    public AttendanceMethod Method { get; private set; }

    public AttendanceSource Source { get; private set; }

    public DateTimeOffset MarkedAt { get; private set; }

    public Guid MarkedByUserId { get; private set; }

    public bool IsBackfilled { get; private set; }
    public bool IsCrossLocation { get; private set; }
    private AttendanceDetail() { }

    private AttendanceDetail(
    Guid tenantId,
    Guid attendanceRecordId,
    Guid studentId,
    AttendanceStatus status,
    AttendanceMethod method,
    AttendanceSource source,
    Guid markedByUserId,
    bool isBackfilled,
    bool isCrossLocation,
    string? note)
{
    TenantId = tenantId;
    AttendanceRecordId = attendanceRecordId;
    StudentId = studentId;
    Status = status;

    Method = method;
    Source = source;

    MarkedByUserId = markedByUserId;
    MarkedAt = DateTimeOffset.UtcNow;
    IsBackfilled = isBackfilled;
    IsCrossLocation = isCrossLocation;

    Note = note?.Trim();

    RaiseDomainEvent(
        new AttendanceDetailMarkedEvent(
            Id,
            AttendanceRecordId,
            StudentId,
            Status));

    }

    public static Result<AttendanceDetail> Create(
    Guid tenantId,
    Guid attendanceRecordId,
    Guid studentId,
    AttendanceStatus status,
    AttendanceMethod method,
    AttendanceSource source,
    Guid markedByUserId,
    bool isBackfilled,
    bool isCrossLocation,
    string? note)
    {
        if (tenantId == Guid.Empty)
            return Result<AttendanceDetail>.Failure(TrainingErrors.TenantRequired);

        if (attendanceRecordId == Guid.Empty)
            return Result<AttendanceDetail>.Failure(TrainingErrors.AttendanceRecordRequired);

        if (studentId == Guid.Empty)
            return Result<AttendanceDetail>.Failure(TrainingErrors.StudentRequired);

        return Result<AttendanceDetail>.Success(
    new AttendanceDetail(
        tenantId,
        attendanceRecordId,
        studentId,
        status,
        method,
        source,
        markedByUserId,
        isBackfilled,
        isCrossLocation,
        note));
    }

    public Result UpdateStatus(
    AttendanceStatus status,
    AttendanceMethod method,
    AttendanceSource source,
    Guid markedByUserId,
    bool isBackfilled,
    bool isCrossLocation,
    string? note)
{
    if (IsArchived)
        return Result.Failure(TrainingErrors.AlreadyArchived);

    Status = status;
    Method = method;
    Source = source;

    MarkedByUserId = markedByUserId;
    MarkedAt = DateTimeOffset.UtcNow;
    IsBackfilled = isBackfilled;
    IsCrossLocation = isCrossLocation;

    Note = note?.Trim();

    MarkUpdated(markedByUserId);

    RaiseDomainEvent(
        new AttendanceDetailUpdatedEvent(
            Id,
            AttendanceRecordId,
            StudentId,
            Status,
            Method,
            Source));

    return Result.Success();
}
}
