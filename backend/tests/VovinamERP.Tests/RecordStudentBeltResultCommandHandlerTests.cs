using Moq;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Students.Common;
using VovinamERP.Application.Students.RecordStudentBeltResult;
using VovinamERP.Domain.Belts;
using VovinamERP.Domain.Students;

namespace VovinamERP.Tests;

public sealed class RecordStudentBeltResultCommandHandlerTests
{
    [Fact]
    public async Task Handle_PassedResult_ShouldSaveHistoryAndChangeCurrentBelt()
    {
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var oldBeltRankId = Guid.NewGuid();
        var newBeltRankId = Guid.NewGuid();

        var studentResult = Student.Register(
            tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            oldBeltRankId,
            "MS-001",
            new DateOnly(2026, 1, 1),
            null,
            null);

        Assert.True(studentResult.IsSuccess);
        Assert.NotNull(studentResult.Value);

        var student = studentResult.Value;

        var studentRepository = new Mock<IStudentRepository>();
        var beltRankRepository = new Mock<IRepository<BeltRank>>();
        var beltHistoryRepository = new Mock<IRepository<StudentBeltHistory>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                studentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        beltRankRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<BeltRank, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        StudentBeltHistory? savedHistory = null;

        beltHistoryRepository
            .Setup(x => x.AddAsync(
                It.IsAny<StudentBeltHistory>(),
                It.IsAny<CancellationToken>()))
            .Callback<StudentBeltHistory, CancellationToken>(
                (history, _) => savedHistory = history)
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RecordStudentBeltResultCommandHandler(
            studentRepository.Object,
            beltRankRepository.Object,
            beltHistoryRepository.Object,
            unitOfWork.Object);

        var command = new RecordStudentBeltResultCommand(
            tenantId,
            studentId,
            newBeltRankId,
            new DateOnly(2026, 9, 20),
            StudentBeltResult.Passed,
            new DateOnly(2026, 9, 20),
            "Thi đạt.",
            Guid.NewGuid());

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedHistory);
        Assert.Equal(StudentBeltResult.Passed, savedHistory.Result);
        Assert.Equal(newBeltRankId, savedHistory.BeltRankId);
        Assert.Equal(newBeltRankId, student.CurrentBeltRankId);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_FailedResult_ShouldSaveHistoryAndKeepCurrentBelt()
    {
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var currentBeltRankId = Guid.NewGuid();
        var attemptedBeltRankId = Guid.NewGuid();

        var studentResult = Student.Register(
            tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            currentBeltRankId,
            "MS-002",
            new DateOnly(2026, 1, 1),
            null,
            null);

        Assert.True(studentResult.IsSuccess);
        Assert.NotNull(studentResult.Value);

        var student = studentResult.Value;

        var studentRepository = new Mock<IStudentRepository>();
        var beltRankRepository = new Mock<IRepository<BeltRank>>();
        var beltHistoryRepository = new Mock<IRepository<StudentBeltHistory>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                studentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        beltRankRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<BeltRank, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        StudentBeltHistory? savedHistory = null;

        beltHistoryRepository
            .Setup(x => x.AddAsync(
                It.IsAny<StudentBeltHistory>(),
                It.IsAny<CancellationToken>()))
            .Callback<StudentBeltHistory, CancellationToken>(
                (history, _) => savedHistory = history)
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RecordStudentBeltResultCommandHandler(
            studentRepository.Object,
            beltRankRepository.Object,
            beltHistoryRepository.Object,
            unitOfWork.Object);

        var command = new RecordStudentBeltResultCommand(
            tenantId,
            studentId,
            attemptedBeltRankId,
            new DateOnly(2026, 9, 20),
            StudentBeltResult.Failed,
            null,
            "Thi chưa đạt.",
            Guid.NewGuid());

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedHistory);
        Assert.Equal(StudentBeltResult.Failed, savedHistory.Result);
        Assert.Equal(attemptedBeltRankId, savedHistory.BeltRankId);

        Assert.Equal(
            currentBeltRankId,
            student.CurrentBeltRankId);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidBeltRank_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        var studentResult = Student.Register(
            tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "MS-003",
            new DateOnly(2026, 1, 1),
            null,
            null);

        Assert.True(studentResult.IsSuccess);
        Assert.NotNull(studentResult.Value);

        var studentRepository = new Mock<IStudentRepository>();
        var beltRankRepository = new Mock<IRepository<BeltRank>>();
        var beltHistoryRepository = new Mock<IRepository<StudentBeltHistory>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                studentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentResult.Value);

        beltRankRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<BeltRank, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new RecordStudentBeltResultCommandHandler(
            studentRepository.Object,
            beltRankRepository.Object,
            beltHistoryRepository.Object,
            unitOfWork.Object);

        var command = new RecordStudentBeltResultCommand(
            tenantId,
            studentId,
            Guid.NewGuid(),
            new DateOnly(2026, 9, 20),
            StudentBeltResult.Passed,
            new DateOnly(2026, 9, 20),
            null,
            null);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);

        beltHistoryRepository.Verify(
            x => x.AddAsync(
                It.IsAny<StudentBeltHistory>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
