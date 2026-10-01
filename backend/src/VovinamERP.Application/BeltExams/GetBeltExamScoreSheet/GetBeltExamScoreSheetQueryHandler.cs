using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.BeltExams;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltExams.GetBeltExamScoreSheet;

public sealed class GetBeltExamScoreSheetQueryHandler
    : IRequestHandler<
        GetBeltExamScoreSheetQuery,
        Result<BeltExamScoreSheetResult>>
{
    private readonly IRepository<BeltExamStudentResult> _resultRepository;
    private readonly IRepository<BeltExamSubject> _subjectRepository;
    private readonly IRepository<BeltExamScore> _scoreRepository;

    public GetBeltExamScoreSheetQueryHandler(
        IRepository<BeltExamStudentResult> resultRepository,
        IRepository<BeltExamSubject> subjectRepository,
        IRepository<BeltExamScore> scoreRepository)
    {
        _resultRepository = resultRepository;
        _subjectRepository = subjectRepository;
        _scoreRepository = scoreRepository;
    }

    public async Task<Result<BeltExamScoreSheetResult>> Handle(
        GetBeltExamScoreSheetQuery request,
        CancellationToken cancellationToken)
    {
        var studentResult = await _resultRepository.GetByIdAsync(
            request.BeltExamStudentResultId,
            cancellationToken);

        if (studentResult is null ||
            studentResult.IsArchived ||
            studentResult.TenantId != request.TenantId ||
            studentResult.BeltExamId != request.BeltExamId)
        {
            return Result<BeltExamScoreSheetResult>.Failure(
                GetBeltExamScoreSheetErrors.StudentResultNotFound);
        }

        var subjects = await _subjectRepository.ListAsync(
            x =>
                x.TenantId == request.TenantId &&
                x.BeltExamId == request.BeltExamId &&
                !x.IsArchived,
            cancellationToken);

        var scores = await _scoreRepository.ListAsync(
            x =>
                x.TenantId == request.TenantId &&
                x.BeltExamStudentResultId ==
                    request.BeltExamStudentResultId &&
                !x.IsArchived,
            cancellationToken);

        var scoreBySubjectId = scores
            .GroupBy(x => x.BeltExamSubjectId)
            .ToDictionary(x => x.Key, x => x.First());

        var subjectResults = subjects
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(subject =>
            {
                var hasScore = scoreBySubjectId.TryGetValue(
                    subject.Id,
                    out var score);

                return new BeltExamScoreSheetSubjectResult(
                    subject.Id,
                    subject.Name,
                    subject.DisplayOrder,
                    subject.MaximumScore,
                    hasScore ? score!.Id : null,
                    hasScore ? score!.Score : null,
                    hasScore);
            })
            .ToList();

        var scoredSubjectCount =
            subjectResults.Count(x => x.IsScored);

        var totalSubjectCount = subjectResults.Count;

        var calculatedTotalScore = subjectResults
            .Where(x => x.Score.HasValue)
            .Sum(x => x.Score!.Value);

        var isComplete =
            totalSubjectCount > 0 &&
            scoredSubjectCount == totalSubjectCount;

        bool? hasTotalScoreMismatch = null;

        if (isComplete &&
            studentResult.TotalScore.HasValue)
        {
            hasTotalScoreMismatch =
                calculatedTotalScore !=
                studentResult.TotalScore.Value;
        }

        var result = new BeltExamScoreSheetResult(
            studentResult.Id,
            studentResult.TotalScore,
            calculatedTotalScore,
            scoredSubjectCount,
            totalSubjectCount,
            isComplete,
            hasTotalScoreMismatch,
            subjectResults);

        return Result<BeltExamScoreSheetResult>.Success(result);
    }
}

public static class GetBeltExamScoreSheetErrors
{
    public static readonly Error StudentResultNotFound =
        new(
            "BELT_EXAM_SCORE_SHEET_APP_001",
            "Student result was not found for the specified tenant and belt exam.");
}
