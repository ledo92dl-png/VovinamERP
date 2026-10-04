using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.BeltRecognitions;

public sealed class BeltRankRecognition : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid BeltRankId { get; private set; }

    public DateOnly RecognitionDate { get; private set; }

    public BeltRankRecognitionSource Source { get; private set; }

    public Guid? BeltExamStudentResultId { get; private set; }

    public string? Note { get; private set; }

    private BeltRankRecognition()
    {
    }

    private BeltRankRecognition(
        Guid tenantId,
        Guid studentId,
        Guid beltRankId,
        DateOnly recognitionDate,
        BeltRankRecognitionSource source,
        Guid? beltExamStudentResultId,
        string? note)
    {
        TenantId = tenantId;
        StudentId = studentId;
        BeltRankId = beltRankId;
        RecognitionDate = recognitionDate;
        Source = source;
        BeltExamStudentResultId = beltExamStudentResultId;
        Note = note?.Trim();
    }

    public static Result<BeltRankRecognition> Create(
        Guid tenantId,
        Guid studentId,
        Guid beltRankId,
        DateOnly recognitionDate,
        BeltRankRecognitionSource source,
        Guid? beltExamStudentResultId,
        string? note)
    {
        if (tenantId == Guid.Empty)
            return Result<BeltRankRecognition>.Failure(
                BeltRankRecognitionErrors.TenantRequired);

        if (studentId == Guid.Empty)
            return Result<BeltRankRecognition>.Failure(
                BeltRankRecognitionErrors.StudentRequired);

        if (beltRankId == Guid.Empty)
            return Result<BeltRankRecognition>.Failure(
                BeltRankRecognitionErrors.BeltRankRequired);

        if (recognitionDate == default)
            return Result<BeltRankRecognition>.Failure(
                BeltRankRecognitionErrors.RecognitionDateRequired);

        if (!Enum.IsDefined(source))
            return Result<BeltRankRecognition>.Failure(
                BeltRankRecognitionErrors.InvalidSource);

        if (source == BeltRankRecognitionSource.BeltExam &&
            !beltExamStudentResultId.HasValue)
        {
            return Result<BeltRankRecognition>.Failure(
                BeltRankRecognitionErrors.BeltExamStudentResultRequired);
        }

        if (source != BeltRankRecognitionSource.BeltExam &&
            beltExamStudentResultId.HasValue)
        {
            return Result<BeltRankRecognition>.Failure(
                BeltRankRecognitionErrors.BeltExamStudentResultNotAllowed);
        }

        return Result<BeltRankRecognition>.Success(
            new BeltRankRecognition(
                tenantId,
                studentId,
                beltRankId,
                recognitionDate,
                source,
                beltExamStudentResultId,
                note));
    }
}
