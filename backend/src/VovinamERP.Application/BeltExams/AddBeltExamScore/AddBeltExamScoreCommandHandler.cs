using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.BeltExams;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltExams.AddBeltExamScore;

public sealed class AddBeltExamScoreCommandHandler
    : IRequestHandler<AddBeltExamScoreCommand, Result<Guid>>
{
    private readonly IRepository<BeltExamStudentResult> _resultRepository;
    private readonly IRepository<BeltExamSubject> _subjectRepository;
    private readonly IRepository<BeltExamScore> _scoreRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddBeltExamScoreCommandHandler(
        IRepository<BeltExamStudentResult> resultRepository,
        IRepository<BeltExamSubject> subjectRepository,
        IRepository<BeltExamScore> scoreRepository,
        IUnitOfWork unitOfWork)
    {
        _resultRepository = resultRepository;
        _subjectRepository = subjectRepository;
        _scoreRepository = scoreRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        AddBeltExamScoreCommand request,
        CancellationToken cancellationToken)
    {
        var studentResult = await _resultRepository.GetByIdAsync(
            request.BeltExamStudentResultId,
            cancellationToken);

        if (studentResult is null ||
            studentResult.IsArchived ||
            studentResult.TenantId != request.TenantId)
        {
            return Result<Guid>.Failure(
                AddBeltExamScoreErrors.StudentResultNotFound);
        }

        var subject = await _subjectRepository.GetByIdAsync(
            request.BeltExamSubjectId,
            cancellationToken);

        if (subject is null ||
            subject.IsArchived ||
            subject.TenantId != request.TenantId)
        {
            return Result<Guid>.Failure(
                AddBeltExamScoreErrors.SubjectNotFound);
        }

        if (studentResult.BeltExamId != subject.BeltExamId)
        {
            return Result<Guid>.Failure(
                AddBeltExamScoreErrors.ExamMismatch);
        }

        if (subject.MaximumScore.HasValue &&
            request.Score > subject.MaximumScore.Value)
        {
            return Result<Guid>.Failure(
                AddBeltExamScoreErrors.ScoreExceedsMaximum);
        }

        var duplicateExists = await _scoreRepository.ExistsAsync(
            x =>
                x.TenantId == request.TenantId &&
                x.BeltExamStudentResultId ==
                    request.BeltExamStudentResultId &&
                x.BeltExamSubjectId ==
                    request.BeltExamSubjectId,
            cancellationToken);

        if (duplicateExists)
        {
            return Result<Guid>.Failure(
                AddBeltExamScoreErrors.ScoreAlreadyExists);
        }

        var createResult = BeltExamScore.Create(
            request.TenantId,
            request.BeltExamStudentResultId,
            request.BeltExamSubjectId,
            request.Score,
            request.UserId);

        if (createResult.IsFailure)
        {
            return Result<Guid>.Failure(createResult.Error);
        }

        var score = createResult.Value;

        await _scoreRepository.AddAsync(
            score,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(score.Id);
    }
}

public static class AddBeltExamScoreErrors
{
    public static readonly Error StudentResultNotFound =
        new(
            "BELT_EXAM_SCORE_APP_001",
            "Student result was not found for the specified tenant.");

    public static readonly Error SubjectNotFound =
        new(
            "BELT_EXAM_SCORE_APP_002",
            "Belt exam subject was not found for the specified tenant.");

    public static readonly Error ExamMismatch =
        new(
            "BELT_EXAM_SCORE_APP_003",
            "Student result and subject do not belong to the same belt exam.");

    public static readonly Error ScoreExceedsMaximum =
        new(
            "BELT_EXAM_SCORE_APP_004",
            "Score cannot exceed the subject maximum score.");

    public static readonly Error ScoreAlreadyExists =
        new(
            "BELT_EXAM_SCORE_APP_005",
            "A score already exists for this student result and subject.");
}
