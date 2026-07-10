using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Training;

public sealed class AttendanceRecord : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid TrainingSessionId { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    private readonly List<AttendanceDetail> _details = new();

    public IReadOnlyCollection<AttendanceDetail> Details => _details.AsReadOnly();

    private AttendanceRecord()
    {
    }

    private AttendanceRecord(
        Guid tenantId,
        Guid trainingSessionId,
        Guid createdByUserId)
    {
        TenantId = tenantId;
        TrainingSessionId = trainingSessionId;
        CreatedByUserId = createdByUserId;

        RaiseDomainEvent(
            new AttendanceRecordCreatedEvent(
                Id,
                TrainingSessionId));
    }

    public static Result<AttendanceRecord> Create(
        Guid tenantId,
        Guid trainingSessionId,
        Guid createdByUserId)
    {
        if (tenantId == Guid.Empty)
            return Result<AttendanceRecord>.Failure(
                TrainingErrors.TenantRequired);

        if (trainingSessionId == Guid.Empty)
            return Result<AttendanceRecord>.Failure(
                TrainingErrors.SessionRequired);

        return Result<AttendanceRecord>.Success(
            new AttendanceRecord(
                tenantId,
                trainingSessionId,
                createdByUserId));
    }

    public Result UpdateStudentAttendance(
        Guid studentId,
        AttendanceStatus status,
        AttendanceMethod method,
        AttendanceSource source,
        Guid markedByUserId,
        bool isBackfilled,
        string? note)
    {
        if (IsArchived)
            return Result.Failure(
                TrainingErrors.AlreadyArchived);

        var detail = _details.FirstOrDefault(
            x => x.StudentId == studentId && !x.IsArchived);

        if (detail is null)
        {
            return Result.Failure(
                new Error(
                    "TRAINING_018",
                    "Student attendance detail was not found."));
        }

        return detail.UpdateStatus(
            status,
            method,
            source,
            markedByUserId,
            isBackfilled,
            note);
    }

    public Result<AttendanceDetail> MarkStudent(
        Guid studentId,
        AttendanceStatus status,
        AttendanceMethod method,
        AttendanceSource source,
        Guid markedByUserId,
        bool isBackfilled,
        string? note)
    {
        if (IsArchived)
            return Result<AttendanceDetail>.Failure(
                TrainingErrors.AlreadyArchived);

        if (studentId == Guid.Empty)
            return Result<AttendanceDetail>.Failure(
                TrainingErrors.StudentRequired);

        if (HasStudent(studentId))
        {
            return Result<AttendanceDetail>.Failure(
                new Error(
                    "TRAINING_017",
                    "Student has already been marked in this attendance record."));
        }

        var detailResult = AttendanceDetail.Create(
            TenantId,
            Id,
            studentId,
            status,
            method,
            source,
            markedByUserId,
            isBackfilled,
            note);

        if (detailResult.IsFailure || detailResult.Value is null)
            return Result<AttendanceDetail>.Failure(
                detailResult.Error);

        _details.Add(detailResult.Value);

        MarkUpdated(markedByUserId);

        return Result<AttendanceDetail>.Success(
            detailResult.Value);
    }

    public bool HasStudent(Guid studentId)
    {
        return _details.Any(
            x => x.StudentId == studentId && !x.IsArchived);
    }
}