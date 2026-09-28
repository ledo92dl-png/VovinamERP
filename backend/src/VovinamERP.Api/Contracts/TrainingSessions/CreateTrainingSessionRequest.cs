namespace VovinamERP.Api.Contracts.TrainingSessions;

public sealed record CreateTrainingSessionRequest(
    Guid TenantId,
    Guid TrainingClassId,
    DateOnly SessionDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? LessonPlan,
    string? CoachNote);