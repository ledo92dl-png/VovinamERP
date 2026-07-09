using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Training;

public sealed class TrainingSession : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid TrainingClassId { get; private set; }
    public DateOnly SessionDate { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public TrainingSessionStatus Status { get; private set; }
    public string? LessonPlan { get; private set; }
    public string? CoachNote { get; private set; }

    private TrainingSession() { }

    private TrainingSession(
        Guid tenantId,
        Guid trainingClassId,
        DateOnly sessionDate,
        TimeOnly startTime,
        TimeOnly endTime,
        string? lessonPlan,
        string? coachNote)
    {
        TenantId = tenantId;
        TrainingClassId = trainingClassId;
        SessionDate = sessionDate;
        StartTime = startTime;
        EndTime = endTime;
        LessonPlan = lessonPlan?.Trim();
        CoachNote = coachNote?.Trim();
        Status = TrainingSessionStatus.Planned;

        RaiseDomainEvent(new TrainingSessionCreatedEvent(Id, TrainingClassId, SessionDate));
    }

    public static Result<TrainingSession> Create(
        Guid tenantId,
        Guid trainingClassId,
        DateOnly sessionDate,
        TimeOnly startTime,
        TimeOnly endTime,
        string? lessonPlan,
        string? coachNote)
    {
        if (tenantId == Guid.Empty)
            return Result<TrainingSession>.Failure(TrainingErrors.TenantRequired);

        if (trainingClassId == Guid.Empty)
            return Result<TrainingSession>.Failure(TrainingErrors.ClassRequired);

        if (sessionDate == default)
            return Result<TrainingSession>.Failure(TrainingErrors.InvalidDate);

        if (endTime <= startTime)
            return Result<TrainingSession>.Failure(TrainingErrors.InvalidTimeRange);

        return Result<TrainingSession>.Success(
            new TrainingSession(tenantId, trainingClassId, sessionDate, startTime, endTime, lessonPlan, coachNote));
    }

    public Result Open()
    {
        if (IsArchived)
            return Result.Failure(TrainingErrors.AlreadyArchived);

        if (Status == TrainingSessionStatus.Closed)
            return Result.Failure(TrainingErrors.SessionAlreadyClosed);

        Status = TrainingSessionStatus.Opened;
        MarkUpdated(null);
        RaiseDomainEvent(new TrainingSessionOpenedEvent(Id));

        return Result.Success();
    }

    public Result Close()
    {
        if (IsArchived)
            return Result.Failure(TrainingErrors.AlreadyArchived);

        Status = TrainingSessionStatus.Closed;
        MarkUpdated(null);
        RaiseDomainEvent(new TrainingSessionClosedEvent(Id));

        return Result.Success();
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        Status = TrainingSessionStatus.Archived;
        base.Archive(userId);
    }
}
