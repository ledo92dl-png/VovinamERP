using Moq;
using VovinamERP.Application.BeltExams.AddBeltExamScore;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.BeltExams;

namespace VovinamERP.Tests;

public sealed class AddBeltExamScoreCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldAddScore()
    {
        var tenantId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        var studentResult = CreateStudentResult(
            tenantId,
            examId);

        var subject = CreateSubject(
            tenantId,
            examId,
            10m);

        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        var subjectRepository =
            new Mock<IRepository<BeltExamSubject>>();

        var scoreRepository =
            new Mock<IRepository<BeltExamScore>>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        SetupEntities(
            resultRepository,
            subjectRepository,
            studentResult,
            subject);

        scoreRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<BeltExamScore, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        BeltExamScore? savedScore = null;

        scoreRepository
            .Setup(x => x.AddAsync(
                It.IsAny<BeltExamScore>(),
                It.IsAny<CancellationToken>()))
            .Callback<BeltExamScore, CancellationToken>(
                (score, _) => savedScore = score)
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler(
            resultRepository,
            subjectRepository,
            scoreRepository,
            unitOfWork);

        var command = new AddBeltExamScoreCommand(
            tenantId,
            studentResult.Id,
            subject.Id,
            7.5m,
            Guid.NewGuid());

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedScore);
        Assert.Equal(savedScore.Id, result.Value);
        Assert.Equal(tenantId, savedScore.TenantId);
        Assert.Equal(
            studentResult.Id,
            savedScore.BeltExamStudentResultId);
        Assert.Equal(
            subject.Id,
            savedScore.BeltExamSubjectId);
        Assert.Equal(7.5m, savedScore.Score);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ZeroScore_ShouldSucceed()
    {
        var context = CreateValidContext(10m);

        BeltExamScore? savedScore = null;

        context.ScoreRepository
            .Setup(x => x.AddAsync(
                It.IsAny<BeltExamScore>(),
                It.IsAny<CancellationToken>()))
            .Callback<BeltExamScore, CancellationToken>(
                (score, _) => savedScore = score)
            .Returns(Task.CompletedTask);

        var command = new AddBeltExamScoreCommand(
            context.TenantId,
            context.StudentResult.Id,
            context.Subject.Id,
            0m,
            null);

        var result = await context.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedScore);
        Assert.Equal(0m, savedScore.Score);
    }

    [Fact]
    public async Task Handle_MissingStudentResult_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();

        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        var subjectRepository =
            new Mock<IRepository<BeltExamSubject>>();

        var scoreRepository =
            new Mock<IRepository<BeltExamScore>>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        var studentResultId = Guid.NewGuid();

        resultRepository
            .Setup(x => x.GetByIdAsync(
                studentResultId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BeltExamStudentResult?)null);

        var handler = CreateHandler(
            resultRepository,
            subjectRepository,
            scoreRepository,
            unitOfWork);

        var result = await handler.Handle(
            new AddBeltExamScoreCommand(
                tenantId,
                studentResultId,
                Guid.NewGuid(),
                5m,
                null),
            CancellationToken.None);

        AssertFailureWithoutSaving(
            result,
            "BELT_EXAM_SCORE_APP_001",
            scoreRepository,
            unitOfWork);
    }

    [Fact]
    public async Task Handle_MissingSubject_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        var studentResult = CreateStudentResult(
            tenantId,
            examId);

        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        var subjectRepository =
            new Mock<IRepository<BeltExamSubject>>();

        var scoreRepository =
            new Mock<IRepository<BeltExamScore>>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        resultRepository
            .Setup(x => x.GetByIdAsync(
                studentResult.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentResult);

        var subjectId = Guid.NewGuid();

        subjectRepository
            .Setup(x => x.GetByIdAsync(
                subjectId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BeltExamSubject?)null);

        var handler = CreateHandler(
            resultRepository,
            subjectRepository,
            scoreRepository,
            unitOfWork);

        var result = await handler.Handle(
            new AddBeltExamScoreCommand(
                tenantId,
                studentResult.Id,
                subjectId,
                5m,
                null),
            CancellationToken.None);

        AssertFailureWithoutSaving(
            result,
            "BELT_EXAM_SCORE_APP_002",
            scoreRepository,
            unitOfWork);
    }

    [Fact]
    public async Task Handle_WrongTenantStudentResult_ShouldFail()
    {
        var tenantId = Guid.NewGuid();

        var studentResult = CreateStudentResult(
            Guid.NewGuid(),
            Guid.NewGuid());

        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        var subjectRepository =
            new Mock<IRepository<BeltExamSubject>>();

        var scoreRepository =
            new Mock<IRepository<BeltExamScore>>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        resultRepository
            .Setup(x => x.GetByIdAsync(
                studentResult.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentResult);

        var handler = CreateHandler(
            resultRepository,
            subjectRepository,
            scoreRepository,
            unitOfWork);

        var result = await handler.Handle(
            new AddBeltExamScoreCommand(
                tenantId,
                studentResult.Id,
                Guid.NewGuid(),
                5m,
                null),
            CancellationToken.None);

        AssertFailureWithoutSaving(
            result,
            "BELT_EXAM_SCORE_APP_001",
            scoreRepository,
            unitOfWork);
    }

    [Fact]
    public async Task Handle_DifferentExams_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();

        var studentResult = CreateStudentResult(
            tenantId,
            Guid.NewGuid());

        var subject = CreateSubject(
            tenantId,
            Guid.NewGuid(),
            10m);

        var context = CreateContext(
            tenantId,
            studentResult,
            subject);

        var result = await context.Handler.Handle(
            new AddBeltExamScoreCommand(
                tenantId,
                studentResult.Id,
                subject.Id,
                5m,
                null),
            CancellationToken.None);

        AssertFailureWithoutSaving(
            result,
            "BELT_EXAM_SCORE_APP_003",
            context.ScoreRepository,
            context.UnitOfWork);
    }

    [Fact]
    public async Task Handle_ScoreAboveMaximum_ShouldFailWithoutSaving()
    {
        var context = CreateValidContext(10m);

        var result = await context.Handler.Handle(
            new AddBeltExamScoreCommand(
                context.TenantId,
                context.StudentResult.Id,
                context.Subject.Id,
                10.01m,
                null),
            CancellationToken.None);

        AssertFailureWithoutSaving(
            result,
            "BELT_EXAM_SCORE_APP_004",
            context.ScoreRepository,
            context.UnitOfWork);
    }

    [Fact]
    public async Task Handle_NegativeScore_ShouldReturnDomainError()
    {
        var context = CreateValidContext(10m);

        var result = await context.Handler.Handle(
            new AddBeltExamScoreCommand(
                context.TenantId,
                context.StudentResult.Id,
                context.Subject.Id,
                -0.01m,
                null),
            CancellationToken.None);

        AssertFailureWithoutSaving(
            result,
            "BELT_EXAM_SCORE_004",
            context.ScoreRepository,
            context.UnitOfWork);
    }

    [Fact]
    public async Task Handle_DuplicateScore_ShouldFailWithoutSaving()
    {
        var context = CreateValidContext(10m);

        context.ScoreRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<BeltExamScore, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await context.Handler.Handle(
            new AddBeltExamScoreCommand(
                context.TenantId,
                context.StudentResult.Id,
                context.Subject.Id,
                5m,
                null),
            CancellationToken.None);

        AssertFailureWithoutSaving(
            result,
            "BELT_EXAM_SCORE_APP_005",
            context.ScoreRepository,
            context.UnitOfWork);
    }

    [Fact]
    public async Task Handle_ArchivedDuplicateScore_ShouldFailWithoutSaving()
    {
        var context = CreateValidContext(10m);

        var archivedScoreResult = BeltExamScore.Create(
            context.TenantId,
            context.StudentResult.Id,
            context.Subject.Id,
            5m,
            null);

        Assert.True(archivedScoreResult.IsSuccess);

        var archivedScore = archivedScoreResult.Value;
        archivedScore.Archive(null);

        context.ScoreRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<BeltExamScore, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                System.Linq.Expressions.Expression<Func<BeltExamScore, bool>> predicate,
                CancellationToken _) =>
                    predicate.Compile()(archivedScore));

        var result = await context.Handler.Handle(
            new AddBeltExamScoreCommand(
                context.TenantId,
                context.StudentResult.Id,
                context.Subject.Id,
                6m,
                null),
            CancellationToken.None);

        AssertFailureWithoutSaving(
            result,
            "BELT_EXAM_SCORE_APP_005",
            context.ScoreRepository,
            context.UnitOfWork);
    }

    private static TestContext CreateValidContext(
        decimal? maximumScore)
    {
        var tenantId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        var studentResult = CreateStudentResult(
            tenantId,
            examId);

        var subject = CreateSubject(
            tenantId,
            examId,
            maximumScore);

        return CreateContext(
            tenantId,
            studentResult,
            subject);
    }

    private static TestContext CreateContext(
        Guid tenantId,
        BeltExamStudentResult studentResult,
        BeltExamSubject subject)
    {
        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        var subjectRepository =
            new Mock<IRepository<BeltExamSubject>>();

        var scoreRepository =
            new Mock<IRepository<BeltExamScore>>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        SetupEntities(
            resultRepository,
            subjectRepository,
            studentResult,
            subject);

        scoreRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<BeltExamScore, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        scoreRepository
            .Setup(x => x.AddAsync(
                It.IsAny<BeltExamScore>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        return new TestContext(
            tenantId,
            studentResult,
            subject,
            scoreRepository,
            unitOfWork,
            CreateHandler(
                resultRepository,
                subjectRepository,
                scoreRepository,
                unitOfWork));
    }

    private static void SetupEntities(
        Mock<IRepository<BeltExamStudentResult>> resultRepository,
        Mock<IRepository<BeltExamSubject>> subjectRepository,
        BeltExamStudentResult studentResult,
        BeltExamSubject subject)
    {
        resultRepository
            .Setup(x => x.GetByIdAsync(
                studentResult.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentResult);

        subjectRepository
            .Setup(x => x.GetByIdAsync(
                subject.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subject);
    }

    private static AddBeltExamScoreCommandHandler CreateHandler(
        Mock<IRepository<BeltExamStudentResult>> resultRepository,
        Mock<IRepository<BeltExamSubject>> subjectRepository,
        Mock<IRepository<BeltExamScore>> scoreRepository,
        Mock<IUnitOfWork> unitOfWork)
    {
        return new AddBeltExamScoreCommandHandler(
            resultRepository.Object,
            subjectRepository.Object,
            scoreRepository.Object,
            unitOfWork.Object);
    }

    private static void AssertFailureWithoutSaving(
        VovinamERP.SharedKernel.Results.Result<Guid> result,
        string expectedErrorCode,
        Mock<IRepository<BeltExamScore>> scoreRepository,
        Mock<IUnitOfWork> unitOfWork)
    {
        Assert.True(result.IsFailure);
        Assert.Equal(
            expectedErrorCode,
            result.Error.Code);

        scoreRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltExamScore>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static BeltExamStudentResult CreateStudentResult(
        Guid tenantId,
        Guid examId)
    {
        var result = BeltExamStudentResult.Create(
            tenantId,
            examId,
            Guid.NewGuid(),
            "Tran Bien",
            "Dat",
            VovinamERP.Domain.Students.StudentBeltResult.Passed,
            36m,
            1,
            null,
            null);

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    private static BeltExamSubject CreateSubject(
        Guid tenantId,
        Guid examId,
        decimal? maximumScore)
    {
        var result = BeltExamSubject.Create(
            tenantId,
            examId,
            "Ly thuyet",
            1,
            maximumScore,
            null);

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    private sealed record TestContext(
        Guid TenantId,
        BeltExamStudentResult StudentResult,
        BeltExamSubject Subject,
        Mock<IRepository<BeltExamScore>> ScoreRepository,
        Mock<IUnitOfWork> UnitOfWork,
        AddBeltExamScoreCommandHandler Handler);
}
