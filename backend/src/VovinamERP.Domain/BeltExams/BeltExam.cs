using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.BeltExams;

public sealed class BeltExam : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid TargetBeltRankId { get; private set; }

    public DateOnly ExamDate { get; private set; }
    public string SessionName { get; private set; } = default!;
    public string Location { get; private set; } = default!;
    public string SourceBeltName { get; private set; } = default!;
    public string? Note { get; private set; }

    private BeltExam()
    {
    }

    private BeltExam(
        Guid tenantId,
        Guid targetBeltRankId,
        DateOnly examDate,
        string sessionName,
        string location,
        string sourceBeltName,
        string? note,
        Guid? userId)
    {
        TenantId = tenantId;
        TargetBeltRankId = targetBeltRankId;
        ExamDate = examDate;
        SessionName = sessionName.Trim();
        Location = location.Trim();
        SourceBeltName = sourceBeltName.Trim();
        Note = note?.Trim();
        CreatedBy = userId;
    }

    public static Result<BeltExam> Create(
        Guid tenantId,
        Guid targetBeltRankId,
        DateOnly examDate,
        string sessionName,
        string location,
        string sourceBeltName,
        string? note,
        Guid? userId)
    {
        if (tenantId == Guid.Empty)
            return Result<BeltExam>.Failure(
                BeltExamErrors.TenantRequired);

        if (targetBeltRankId == Guid.Empty)
            return Result<BeltExam>.Failure(
                BeltExamErrors.TargetBeltRankRequired);

        if (examDate == default)
            return Result<BeltExam>.Failure(
                BeltExamErrors.ExamDateRequired);

        if (string.IsNullOrWhiteSpace(sessionName))
            return Result<BeltExam>.Failure(
                BeltExamErrors.SessionNameRequired);

        if (string.IsNullOrWhiteSpace(location))
            return Result<BeltExam>.Failure(
                BeltExamErrors.LocationRequired);

        if (string.IsNullOrWhiteSpace(sourceBeltName))
            return Result<BeltExam>.Failure(
                BeltExamErrors.SourceBeltNameRequired);

        return Result<BeltExam>.Success(
            new BeltExam(
                tenantId,
                targetBeltRankId,
                examDate,
                sessionName,
                location,
                sourceBeltName,
                note,
                userId));
    }
}