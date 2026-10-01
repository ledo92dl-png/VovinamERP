using System.Linq.Expressions;
using Moq;
using VovinamERP.Application.BeltExams.GetBeltExams;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.BeltExams;
using VovinamERP.Domain.Belts;

namespace VovinamERP.Tests;

public sealed class GetBeltExamsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnOnlyActiveExamsForTenantOrderedByNewestDate()
    {
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();

        var targetBelt = CreateBeltRank(
            "HOANG",
            "Hoàng đai",
            6);

        var newestExam = CreateExam(
            tenantId,
            targetBelt.Id,
            new DateOnly(2026, 9, 20),
            "Khóa 3");

        var olderExam = CreateExam(
            tenantId,
            targetBelt.Id,
            new DateOnly(2026, 7, 12),
            "Khóa 1");

        var otherTenantExam = CreateExam(
            otherTenantId,
            targetBelt.Id,
            new DateOnly(2026, 10, 1),
            "Khóa khác");

        var archivedExam = CreateExam(
            tenantId,
            targetBelt.Id,
            new DateOnly(2026, 9, 25),
            "Khóa đã lưu trữ");

        archivedExam.Archive(null);

        var exams = new[]
        {
            olderExam,
            otherTenantExam,
            archivedExam,
            newestExam
        };

        var examRepository =
            new Mock<IRepository<BeltExam>>();

        var beltRankRepository =
            new Mock<IRepository<BeltRank>>();

        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        examRepository
            .Setup(x => x.ListAsync(
                It.IsAny<Expression<Func<BeltExam, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                Expression<Func<BeltExam, bool>> predicate,
                CancellationToken _) =>
                exams.Where(predicate.Compile()).ToList());

        beltRankRepository
            .Setup(x => x.GetByIdAsync(
                targetBelt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetBelt);

        resultRepository
            .Setup(x => x.ListAsync(
                It.IsAny<Expression<Func<BeltExamStudentResult, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<BeltExamStudentResult>());

        var handler = new GetBeltExamsQueryHandler(
            examRepository.Object,
            beltRankRepository.Object,
            resultRepository.Object);

        var result = await handler.Handle(
            new GetBeltExamsQuery(tenantId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);

        Assert.Equal(
            newestExam.Id,
            result.Value[0].Id);

        Assert.Equal(
            olderExam.Id,
            result.Value[1].Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnTargetBeltInformation()
    {
        var tenantId = Guid.NewGuid();

        var targetBelt = CreateBeltRank(
            "HOANG",
            "Hoàng đai",
            6);

        var exam = CreateExam(
            tenantId,
            targetBelt.Id,
            new DateOnly(2026, 7, 12),
            "Khóa 1");

        var context = CreateContext(
            new[] { exam },
            new[] { targetBelt },
            Array.Empty<BeltExamStudentResult>());

        var result = await context.Handler.Handle(
            new GetBeltExamsQuery(tenantId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var item = Assert.Single(result.Value);

        Assert.Equal(targetBelt.Id, item.TargetBeltRankId);
        Assert.Equal("HOANG", item.TargetBeltCode);
        Assert.Equal("Hoàng đai", item.TargetBeltName);
    }

    [Fact]
    public async Task Handle_StudentCount_ShouldCountOnlyValidResultsForExam()
    {
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();

        var targetBelt = CreateBeltRank(
            "HOANG",
            "Hoàng đai",
            6);

        var exam = CreateExam(
            tenantId,
            targetBelt.Id,
            new DateOnly(2026, 7, 12),
            "Khóa 1");

        var otherExam = CreateExam(
            tenantId,
            targetBelt.Id,
            new DateOnly(2026, 8, 12),
            "Khóa 2");

        var validResult1 = CreateStudentResult(
            tenantId,
            exam.Id);

        var validResult2 = CreateStudentResult(
            tenantId,
            exam.Id);

        var archivedResult = CreateStudentResult(
            tenantId,
            exam.Id);

        archivedResult.Archive(null);

        var otherExamResult = CreateStudentResult(
            tenantId,
            otherExam.Id);

        var otherTenantResult = CreateStudentResult(
            otherTenantId,
            exam.Id);

        var context = CreateContext(
            new[] { exam },
            new[] { targetBelt },
            new[]
            {
                validResult1,
                validResult2,
                archivedResult,
                otherExamResult,
                otherTenantResult
            });

        var result = await context.Handler.Handle(
            new GetBeltExamsQuery(tenantId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var item = Assert.Single(result.Value);

        Assert.Equal(2, item.StudentCount);
    }

    [Fact]
    public async Task Handle_NoExams_ShouldReturnSuccessfulEmptyList()
    {
        var tenantId = Guid.NewGuid();

        var context = CreateContext(
            Array.Empty<BeltExam>(),
            Array.Empty<BeltRank>(),
            Array.Empty<BeltExamStudentResult>());

        var result = await context.Handler.Handle(
            new GetBeltExamsQuery(tenantId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_SameExamDate_ShouldOrderBySessionName()
    {
        var tenantId = Guid.NewGuid();

        var targetBelt = CreateBeltRank(
            "HOANG",
            "Hoàng đai",
            6);

        var examB = CreateExam(
            tenantId,
            targetBelt.Id,
            new DateOnly(2026, 7, 12),
            "Khóa B");

        var examA = CreateExam(
            tenantId,
            targetBelt.Id,
            new DateOnly(2026, 7, 12),
            "Khóa A");

        var context = CreateContext(
            new[] { examB, examA },
            new[] { targetBelt },
            Array.Empty<BeltExamStudentResult>());

        var result = await context.Handler.Handle(
            new GetBeltExamsQuery(tenantId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);

        Assert.Equal("Khóa A", result.Value[0].SessionName);
        Assert.Equal("Khóa B", result.Value[1].SessionName);
    }
    [Fact]
    public async Task Handle_ArchivedTargetBelt_ShouldStillReturnHistoricalExam()
    {
        var tenantId = Guid.NewGuid();

        var targetBelt = CreateBeltRank(
            "HOANG",
            "Hoàng đai",
            6);

        var exam = CreateExam(
            tenantId,
            targetBelt.Id,
            new DateOnly(2026, 7, 12),
            "Khóa 1");

        targetBelt.Archive(null);

        var context = CreateContext(
            new[] { exam },
            new[] { targetBelt },
            Array.Empty<BeltExamStudentResult>());

        var result = await context.Handler.Handle(
            new GetBeltExamsQuery(tenantId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var item = Assert.Single(result.Value);

        Assert.Equal(exam.Id, item.Id);
        Assert.Equal("HOANG", item.TargetBeltCode);
        Assert.Equal("Hoàng đai", item.TargetBeltName);
    }
    private static TestContext CreateContext(
        IReadOnlyList<BeltExam> exams,
        IReadOnlyList<BeltRank> beltRanks,
        IReadOnlyList<BeltExamStudentResult> studentResults)
    {
        var examRepository =
            new Mock<IRepository<BeltExam>>();

        var beltRankRepository =
            new Mock<IRepository<BeltRank>>();

        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        examRepository
            .Setup(x => x.ListAsync(
                It.IsAny<Expression<Func<BeltExam, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                Expression<Func<BeltExam, bool>> predicate,
                CancellationToken _) =>
                exams.Where(predicate.Compile()).ToList());

        beltRankRepository
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                Guid id,
                CancellationToken _) =>
                beltRanks.FirstOrDefault(x => x.Id == id));

        resultRepository
            .Setup(x => x.ListAsync(
                It.IsAny<Expression<Func<BeltExamStudentResult, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                Expression<Func<BeltExamStudentResult, bool>> predicate,
                CancellationToken _) =>
                studentResults.Where(predicate.Compile()).ToList());

        var handler = new GetBeltExamsQueryHandler(
            examRepository.Object,
            beltRankRepository.Object,
            resultRepository.Object);

        return new TestContext(handler);
    }

    private static BeltExamStudentResult CreateStudentResult(
        Guid tenantId,
        Guid beltExamId)
    {
        var result = BeltExamStudentResult.Create(
            tenantId,
            beltExamId,
            Guid.NewGuid(),
            "Đơn vị kiểm thử",
            "Đạt",
            VovinamERP.Domain.Students.StudentBeltResult.Passed,
            null,
            null,
            null,
            null);

        Assert.True(result.IsSuccess);

        return result.Value;
    }
    private static BeltExam CreateExam(
        Guid tenantId,
        Guid targetBeltRankId,
        DateOnly examDate,
        string sessionName)
    {
        var result = BeltExam.Create(
            tenantId,
            targetBeltRankId,
            examDate,
            sessionName,
            "Thành phố Đồng Nai",
            "Hoàng đai",
            null,
            null);

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    private static BeltRank CreateBeltRank(
        string beltCode,
        string beltName,
        int level)
    {
        var result = BeltRank.Create(
            beltCode,
            beltName,
            level,
            null);

        Assert.True(result.IsSuccess);

        return result.Value;
    }
    private sealed record TestContext(
        GetBeltExamsQueryHandler Handler);}