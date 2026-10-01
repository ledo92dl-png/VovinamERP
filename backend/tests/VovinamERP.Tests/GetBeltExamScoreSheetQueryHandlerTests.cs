using System.Linq.Expressions;
using Moq;
using VovinamERP.Application.BeltExams.GetBeltExamScoreSheet;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.BeltExams;
using VovinamERP.Domain.Students;

namespace VovinamERP.Tests;

public sealed class GetBeltExamScoreSheetQueryHandlerTests
{
    [Fact]
    public async Task Handle_MissingStudentResult_ShouldFail()
    {
        var tenantId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var studentResultId = Guid.NewGuid();

        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        var subjectRepository =
            new Mock<IRepository<BeltExamSubject>>();

        var scoreRepository =
            new Mock<IRepository<BeltExamScore>>();

        resultRepository
            .Setup(x => x.GetByIdAsync(
                studentResultId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BeltExamStudentResult?)null);

        var handler = new GetBeltExamScoreSheetQueryHandler(
            resultRepository.Object,
            subjectRepository.Object,
            scoreRepository.Object);

        var result = await handler.Handle(
            new GetBeltExamScoreSheetQuery(
                tenantId,
                examId,
                studentResultId),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_EXAM_SCORE_SHEET_APP_001",
            result.Error.Code);
    }

    [Fact]
    public async Task Handle_WrongTenant_ShouldFail()
    {
        var actualTenantId = Guid.NewGuid();
        var requestedTenantId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        var studentResult = CreateStudentResult(
            actualTenantId,
            examId,
            10m);

        var context = CreateContext(studentResult);

        var result = await context.Handler.Handle(
            new GetBeltExamScoreSheetQuery(
                requestedTenantId,
                examId,
                studentResult.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_EXAM_SCORE_SHEET_APP_001",
            result.Error.Code);
    }

    [Fact]
    public async Task Handle_WrongExam_ShouldFail()
    {
        var tenantId = Guid.NewGuid();
        var actualExamId = Guid.NewGuid();
        var requestedExamId = Guid.NewGuid();

        var studentResult = CreateStudentResult(
            tenantId,
            actualExamId,
            10m);

        var context = CreateContext(studentResult);

        var result = await context.Handler.Handle(
            new GetBeltExamScoreSheetQuery(
                tenantId,
                requestedExamId,
                studentResult.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_EXAM_SCORE_SHEET_APP_001",
            result.Error.Code);
    }

    [Fact]
    public async Task Handle_NoSubjects_ShouldReturnIncompleteSheet()
    {
        var tenantId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        var studentResult = CreateStudentResult(
            tenantId,
            examId,
            36m);

        var context = CreateContext(
            studentResult,
            Array.Empty<BeltExamSubject>(),
            Array.Empty<BeltExamScore>());

        var result = await context.Handler.Handle(
            CreateQuery(studentResult),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var sheet = result.Value;

        Assert.Equal(36m, sheet.RecordedTotalScore);
        Assert.Equal(0m, sheet.CalculatedTotalScore);
        Assert.Equal(0, sheet.ScoredSubjectCount);
        Assert.Equal(0, sheet.TotalSubjectCount);
        Assert.False(sheet.IsComplete);
        Assert.Null(sheet.HasTotalScoreMismatch);
        Assert.Empty(sheet.Subjects);
    }

    [Fact]
    public async Task Handle_UnscoredSubject_ShouldReturnNullScore()
    {
        var tenantId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        var studentResult = CreateStudentResult(
            tenantId,
            examId,
            7m);

        var subject = CreateSubject(
            tenantId,
            examId,
            "Lý thuyết",
            1,
            10m);

        var context = CreateContext(
            studentResult,
            new[] { subject },
            Array.Empty<BeltExamScore>());

        var result = await context.Handler.Handle(
            CreateQuery(studentResult),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var sheet = result.Value;
        var item = Assert.Single(sheet.Subjects);

        Assert.Null(item.ScoreId);
        Assert.Null(item.Score);
        Assert.False(item.IsScored);

        Assert.Equal(0m, sheet.CalculatedTotalScore);
        Assert.Equal(0, sheet.ScoredSubjectCount);
        Assert.Equal(1, sheet.TotalSubjectCount);
        Assert.False(sheet.IsComplete);
        Assert.Null(sheet.HasTotalScoreMismatch);
    }

    [Fact]
    public async Task Handle_ZeroScore_ShouldCountAsScored()
    {
        var tenantId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        var studentResult = CreateStudentResult(
            tenantId,
            examId,
            0m);

        var subject = CreateSubject(
            tenantId,
            examId,
            "Lý thuyết",
            1,
            10m);

        var score = CreateScore(
            tenantId,
            studentResult.Id,
            subject.Id,
            0m);

        var context = CreateContext(
            studentResult,
            new[] { subject },
            new[] { score });

        var result = await context.Handler.Handle(
            CreateQuery(studentResult),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var sheet = result.Value;
        var item = Assert.Single(sheet.Subjects);

        Assert.Equal(score.Id, item.ScoreId);
        Assert.Equal(0m, item.Score);
        Assert.True(item.IsScored);

        Assert.Equal(0m, sheet.CalculatedTotalScore);
        Assert.Equal(1, sheet.ScoredSubjectCount);
        Assert.Equal(1, sheet.TotalSubjectCount);
        Assert.True(sheet.IsComplete);
        Assert.False(sheet.HasTotalScoreMismatch);
    }

    [Fact]
    public async Task Handle_PartialScores_ShouldCalculatePartialTotalWithoutMismatch()
    {
        var tenantId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        var studentResult = CreateStudentResult(
            tenantId,
            examId,
            13m);

        var theory = CreateSubject(
            tenantId,
            examId,
            "Lý thuyết",
            1,
            10m);

        var form = CreateSubject(
            tenantId,
            examId,
            "Quyền",
            2,
            10m);

        var theoryScore = CreateScore(
            tenantId,
            studentResult.Id,
            theory.Id,
            7m);

        var context = CreateContext(
            studentResult,
            new[] { theory, form },
            new[] { theoryScore });

        var result = await context.Handler.Handle(
            CreateQuery(studentResult),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var sheet = result.Value;

        Assert.Equal(7m, sheet.CalculatedTotalScore);
        Assert.Equal(1, sheet.ScoredSubjectCount);
        Assert.Equal(2, sheet.TotalSubjectCount);
        Assert.False(sheet.IsComplete);
        Assert.Null(sheet.HasTotalScoreMismatch);

        Assert.True(sheet.Subjects[0].IsScored);
        Assert.False(sheet.Subjects[1].IsScored);
    }

    [Fact]
    public async Task Handle_CompleteScores_ShouldDetectMatchingAndMismatchingTotals()
    {
        var tenantId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        var theory = CreateSubject(
            tenantId,
            examId,
            "Lý thuyết",
            1,
            10m);

        var form = CreateSubject(
            tenantId,
            examId,
            "Quyền",
            2,
            10m);

        var matchingStudentResult = CreateStudentResult(
            tenantId,
            examId,
            13m);

        var matchingScores = new[]
        {
            CreateScore(
                tenantId,
                matchingStudentResult.Id,
                theory.Id,
                7m),

            CreateScore(
                tenantId,
                matchingStudentResult.Id,
                form.Id,
                6m)
        };

        var matchingContext = CreateContext(
            matchingStudentResult,
            new[] { theory, form },
            matchingScores);

        var matchingResult =
            await matchingContext.Handler.Handle(
                CreateQuery(matchingStudentResult),
                CancellationToken.None);

        Assert.True(matchingResult.IsSuccess);
        Assert.True(matchingResult.Value.IsComplete);
        Assert.Equal(
            13m,
            matchingResult.Value.CalculatedTotalScore);
        Assert.False(
            matchingResult.Value.HasTotalScoreMismatch);

        var mismatchingStudentResult = CreateStudentResult(
            tenantId,
            examId,
            14m);

        var mismatchingScores = new[]
        {
            CreateScore(
                tenantId,
                mismatchingStudentResult.Id,
                theory.Id,
                7m),

            CreateScore(
                tenantId,
                mismatchingStudentResult.Id,
                form.Id,
                6m)
        };

        var mismatchingContext = CreateContext(
            mismatchingStudentResult,
            new[] { theory, form },
            mismatchingScores);

        var mismatchingResult =
            await mismatchingContext.Handler.Handle(
                CreateQuery(mismatchingStudentResult),
                CancellationToken.None);

        Assert.True(mismatchingResult.IsSuccess);
        Assert.True(mismatchingResult.Value.IsComplete);
        Assert.Equal(
            13m,
            mismatchingResult.Value.CalculatedTotalScore);
        Assert.True(
            mismatchingResult.Value.HasTotalScoreMismatch);
    }

    [Fact]
    public async Task Handle_CompleteScoresWithoutRecordedTotal_ShouldLeaveMismatchUnknown()
    {
        var tenantId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        var studentResult = CreateStudentResult(
            tenantId,
            examId,
            null);

        var subject = CreateSubject(
            tenantId,
            examId,
            "Lý thuyết",
            1,
            10m);

        var score = CreateScore(
            tenantId,
            studentResult.Id,
            subject.Id,
            7m);

        var context = CreateContext(
            studentResult,
            new[] { subject },
            new[] { score });

        var result = await context.Handler.Handle(
            CreateQuery(studentResult),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var sheet = result.Value;

        Assert.True(sheet.IsComplete);
        Assert.Equal(7m, sheet.CalculatedTotalScore);
        Assert.Null(sheet.RecordedTotalScore);
        Assert.Null(sheet.HasTotalScoreMismatch);
    }

    [Fact]
    public async Task Handle_ArchivedSubject_ShouldExcludeSubjectAndItsScore()
    {
        var tenantId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        var studentResult = CreateStudentResult(
            tenantId,
            examId,
            7m);

        var activeSubject = CreateSubject(
            tenantId,
            examId,
            "Lý thuyết",
            1,
            10m);

        var archivedSubject = CreateSubject(
            tenantId,
            examId,
            "Quyền",
            2,
            10m);

        archivedSubject.Archive(null);

        var activeScore = CreateScore(
            tenantId,
            studentResult.Id,
            activeSubject.Id,
            7m);

        var archivedSubjectScore = CreateScore(
            tenantId,
            studentResult.Id,
            archivedSubject.Id,
            6m);

        var context = CreateContext(
            studentResult,
            new[] { activeSubject, archivedSubject },
            new[] { activeScore, archivedSubjectScore });

        var result = await context.Handler.Handle(
            CreateQuery(studentResult),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var sheet = result.Value;
        var item = Assert.Single(sheet.Subjects);

        Assert.Equal(activeSubject.Id, item.SubjectId);
        Assert.Equal(7m, sheet.CalculatedTotalScore);
        Assert.Equal(1, sheet.TotalSubjectCount);
        Assert.Equal(1, sheet.ScoredSubjectCount);
        Assert.True(sheet.IsComplete);
        Assert.False(sheet.HasTotalScoreMismatch);
    }

    [Fact]
    public async Task Handle_ArchivedScore_ShouldTreatSubjectAsUnscored()
    {
        var tenantId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        var studentResult = CreateStudentResult(
            tenantId,
            examId,
            7m);

        var subject = CreateSubject(
            tenantId,
            examId,
            "Lý thuyết",
            1,
            10m);

        var score = CreateScore(
            tenantId,
            studentResult.Id,
            subject.Id,
            7m);

        score.Archive(null);

        var context = CreateContext(
            studentResult,
            new[] { subject },
            new[] { score });

        var result = await context.Handler.Handle(
            CreateQuery(studentResult),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var sheet = result.Value;
        var item = Assert.Single(sheet.Subjects);

        Assert.Null(item.ScoreId);
        Assert.Null(item.Score);
        Assert.False(item.IsScored);
        Assert.Equal(0m, sheet.CalculatedTotalScore);
        Assert.Equal(0, sheet.ScoredSubjectCount);
        Assert.Equal(1, sheet.TotalSubjectCount);
        Assert.False(sheet.IsComplete);
        Assert.Null(sheet.HasTotalScoreMismatch);
    }

    [Fact]
    public async Task Handle_Subjects_ShouldBeOrderedByDisplayOrder()
    {
        var tenantId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        var studentResult = CreateStudentResult(
            tenantId,
            examId,
            null);

        var third = CreateSubject(
            tenantId,
            examId,
            "Võ lực",
            3,
            10m);

        var first = CreateSubject(
            tenantId,
            examId,
            "Lý thuyết",
            1,
            10m);

        var second = CreateSubject(
            tenantId,
            examId,
            "Quyền",
            2,
            10m);

        var context = CreateContext(
            studentResult,
            new[] { third, first, second },
            Array.Empty<BeltExamScore>());

        var result = await context.Handler.Handle(
            CreateQuery(studentResult),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var subjects = result.Value.Subjects;

        Assert.Equal(3, subjects.Count);
        Assert.Equal(first.Id, subjects[0].SubjectId);
        Assert.Equal(second.Id, subjects[1].SubjectId);
        Assert.Equal(third.Id, subjects[2].SubjectId);

        Assert.Equal(
            new[] { 1, 2, 3 },
            subjects.Select(x => x.DisplayOrder).ToArray());
    }
    private static TestContext CreateContext(
        BeltExamStudentResult studentResult,
        IReadOnlyList<BeltExamSubject>? subjects = null,
        IReadOnlyList<BeltExamScore>? scores = null)
    {
        subjects ??= Array.Empty<BeltExamSubject>();
        scores ??= Array.Empty<BeltExamScore>();

        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        var subjectRepository =
            new Mock<IRepository<BeltExamSubject>>();

        var scoreRepository =
            new Mock<IRepository<BeltExamScore>>();

        resultRepository
            .Setup(x => x.GetByIdAsync(
                studentResult.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentResult);

        subjectRepository
            .Setup(x => x.ListAsync(
                It.IsAny<Expression<Func<BeltExamSubject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                Expression<Func<BeltExamSubject, bool>> predicate,
                CancellationToken _) =>
                subjects.Where(predicate.Compile()).ToList());

        scoreRepository
            .Setup(x => x.ListAsync(
                It.IsAny<Expression<Func<BeltExamScore, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                Expression<Func<BeltExamScore, bool>> predicate,
                CancellationToken _) =>
                scores.Where(predicate.Compile()).ToList());

        var handler =
            new GetBeltExamScoreSheetQueryHandler(
                resultRepository.Object,
                subjectRepository.Object,
                scoreRepository.Object);

        return new TestContext(handler);
    }

    private static GetBeltExamScoreSheetQuery CreateQuery(
        BeltExamStudentResult studentResult)
    {
        return new GetBeltExamScoreSheetQuery(
            studentResult.TenantId,
            studentResult.BeltExamId,
            studentResult.Id);
    }

    private static BeltExamStudentResult CreateStudentResult(
        Guid tenantId,
        Guid examId,
        decimal? totalScore)
    {
        var result = BeltExamStudentResult.Create(
            tenantId,
            examId,
            Guid.NewGuid(),
            "Trấn Biên",
            "Đạt",
            StudentBeltResult.Passed,
            totalScore,
            1,
            null,
            null);

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    private static BeltExamSubject CreateSubject(
        Guid tenantId,
        Guid examId,
        string name,
        int displayOrder,
        decimal? maximumScore)
    {
        var result = BeltExamSubject.Create(
            tenantId,
            examId,
            name,
            displayOrder,
            maximumScore,
            null);

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    private static BeltExamScore CreateScore(
        Guid tenantId,
        Guid studentResultId,
        Guid subjectId,
        decimal score)
    {
        var result = BeltExamScore.Create(
            tenantId,
            studentResultId,
            subjectId,
            score,
            null);

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    private sealed record TestContext(
        GetBeltExamScoreSheetQueryHandler Handler);
}
