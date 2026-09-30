using Moq;
using VovinamERP.Application.Students.Common;
using VovinamERP.Application.Students.GetStudentBeltHistory;
using VovinamERP.Domain.Students;

namespace VovinamERP.Tests;

public sealed class GetStudentBeltHistoryQueryHandlerTests
{
    [Fact]
    public async Task Handle_ExistingStudent_ShouldReturnBeltHistory()
    {
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var beltRankId = Guid.NewGuid();

        var studentResult = Student.Register(
            tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            beltRankId,
            "MS-004",
            new DateOnly(2026, 1, 1),
            null,
            null);

        Assert.True(studentResult.IsSuccess);
        Assert.NotNull(studentResult.Value);

        var items = new List<StudentBeltHistoryListItem>
        {
            new(
                Guid.NewGuid(),
                beltRankId,
                "BLUE-1",
                "Lam đai I",
                1,
                new DateOnly(2026, 9, 20),
                StudentBeltResult.Passed,
                new DateOnly(2026, 9, 20),
                "Thi đạt.")
        };

        var studentRepository = new Mock<IStudentRepository>();
        var beltHistoryRepository =
            new Mock<IStudentBeltHistoryRepository>();

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                studentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentResult.Value);

        beltHistoryRepository
            .Setup(x => x.ListByStudentAsync(
                tenantId,
                studentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var handler = new GetStudentBeltHistoryQueryHandler(
            studentRepository.Object,
            beltHistoryRepository.Object);

        var query = new GetStudentBeltHistoryQuery(
            tenantId,
            studentId);

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(studentId, result.Value.StudentId);
        Assert.Single(result.Value.Items);

        var item = result.Value.Items[0];

        Assert.Equal(beltRankId, item.BeltRankId);
        Assert.Equal("BLUE-1", item.BeltCode);
        Assert.Equal("Lam đai I", item.BeltName);
        Assert.Equal(StudentBeltResult.Passed, item.Result);

        beltHistoryRepository.Verify(
            x => x.ListByStudentAsync(
                tenantId,
                studentId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_StudentNotFound_ShouldFailWithoutReadingHistory()
    {
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        var studentRepository = new Mock<IStudentRepository>();
        var beltHistoryRepository =
            new Mock<IStudentBeltHistoryRepository>();

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                studentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        var handler = new GetStudentBeltHistoryQueryHandler(
            studentRepository.Object,
            beltHistoryRepository.Object);

        var query = new GetStudentBeltHistoryQuery(
            tenantId,
            studentId);

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.True(result.IsFailure);

        beltHistoryRepository.Verify(
            x => x.ListByStudentAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_EmptyTenantId_ShouldFailWithoutAccessingRepositories()
    {
        var studentRepository = new Mock<IStudentRepository>();
        var beltHistoryRepository =
            new Mock<IStudentBeltHistoryRepository>();

        var handler = new GetStudentBeltHistoryQueryHandler(
            studentRepository.Object,
            beltHistoryRepository.Object);

        var query = new GetStudentBeltHistoryQuery(
            Guid.Empty,
            Guid.NewGuid());

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.True(result.IsFailure);

        studentRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        beltHistoryRepository.Verify(
            x => x.ListByStudentAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}