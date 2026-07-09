using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Training;

public sealed class StudentClassEnrollment : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid TrainingClassId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public string? Note { get; private set; }
    public string? EndReason { get; private set; }

    public bool IsActive => Status is EnrollmentStatus.Trial or EnrollmentStatus.Active;
    public bool HasEnded => EndDate.HasValue;

    private StudentClassEnrollment() { }

    private StudentClassEnrollment(
        Guid tenantId,
        Guid studentId,
        Guid trainingClassId,
        DateOnly startDate,
        EnrollmentStatus status,
        string? note)
    {
        TenantId = tenantId;
        StudentId = studentId;
        TrainingClassId = trainingClassId;
        StartDate = startDate;
        Status = status;
        Note = note?.Trim();

        RaiseDomainEvent(new StudentEnrolledInClassEvent(Id, TrainingClassId, StudentId));
    }

    public static Result<StudentClassEnrollment> Create(
        Guid tenantId,
        Guid studentId,
        Guid trainingClassId,
        DateOnly startDate,
        string? note)
    {
        return Create(tenantId, studentId, trainingClassId, startDate, EnrollmentStatus.Active, note);
    }

    public static Result<StudentClassEnrollment> Create(
        Guid tenantId,
        Guid studentId,
        Guid trainingClassId,
        DateOnly startDate,
        EnrollmentStatus status,
        string? note)
    {
        if (tenantId == Guid.Empty)
            return Result<StudentClassEnrollment>.Failure(TrainingErrors.TenantRequired);

        if (studentId == Guid.Empty)
            return Result<StudentClassEnrollment>.Failure(TrainingErrors.StudentRequired);

        if (trainingClassId == Guid.Empty)
            return Result<StudentClassEnrollment>.Failure(TrainingErrors.ClassRequired);

        if (startDate == default)
            return Result<StudentClassEnrollment>.Failure(TrainingErrors.InvalidDate);

        var enrollment = new StudentClassEnrollment(tenantId, studentId, trainingClassId, startDate, status, note);

        return Result<StudentClassEnrollment>.Success(enrollment);
    }

    public Result MarkActive(string? reason, Guid? userId)
    {
        return ChangeStatus(EnrollmentStatus.Active, reason, userId);
    }

    public Result Pause(string reason, Guid? userId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(TrainingErrors.EnrollmentReasonRequired);

        return ChangeStatus(EnrollmentStatus.Paused, reason, userId);
    }

    public Result Reserve(string reason, Guid? userId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(TrainingErrors.EnrollmentReasonRequired);

        return ChangeStatus(EnrollmentStatus.Reserved, reason, userId);
    }

    public Result EndEnrollment(DateOnly endDate, EnrollmentStatus status, string? reason)
    {
        return EndEnrollment(endDate, status, reason, null);
    }

    public Result EndEnrollment(DateOnly endDate, EnrollmentStatus status, string? reason, Guid? userId)
    {
        if (IsArchived)
            return Result.Failure(TrainingErrors.AlreadyArchived);

        if (HasEnded)
            return Result.Failure(TrainingErrors.EnrollmentAlreadyEnded);

        if (endDate < StartDate)
            return Result.Failure(TrainingErrors.InvalidDate);

        if (status is EnrollmentStatus.Trial or EnrollmentStatus.Active or EnrollmentStatus.Paused or EnrollmentStatus.Reserved)
            return Result.Failure(TrainingErrors.InvalidDate);

        EndDate = endDate;
        Status = status;
        EndReason = reason?.Trim();
        MarkUpdated(userId);

        RaiseDomainEvent(new StudentEnrollmentEndedEvent(Id, StudentId, TrainingClassId, Status, endDate, EndReason));

        return Result.Success();
    }

    public Result ChangeStatus(EnrollmentStatus newStatus, string? reason, Guid? userId)
    {
        if (IsArchived)
            return Result.Failure(TrainingErrors.AlreadyArchived);

        if (HasEnded)
            return Result.Failure(TrainingErrors.EnrollmentAlreadyEnded);

        if (Status == newStatus)
            return Result.Success();

        var oldStatus = Status;
        Status = newStatus;
        Note = reason?.Trim();
        MarkUpdated(userId);

        RaiseDomainEvent(new StudentEnrollmentStatusChangedEvent(
            Id,
            StudentId,
            TrainingClassId,
            oldStatus,
            newStatus,
            Note));

        return Result.Success();
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        Status = EnrollmentStatus.Archived;
        base.Archive(userId);
    }
}
