using Moq;
using VovinamERP.Application.BeltRecognitions.Common;
using VovinamERP.Application.BeltRecognitions.GetStudentBeltRecognitions;
using VovinamERP.Application.Students.Common;
using VovinamERP.Domain.BeltRecognitions;
using VovinamERP.Domain.Students;

namespace VovinamERP.Tests;

public sealed class GetStudentBeltRecognitionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ExistingStudent_ShouldReturnRecognitions()
    {
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var beltRankId = Guid.NewGuid();
        var examResultId = Guid.NewGuid();

        var studentResult = Student.Register(
            tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            beltRankId,
            "MS-REC-001",
            new DateOnly(2026, 1, 1),
            null,
            null);

        Assert.True(studentResult.IsSuccess);
        Assert.NotNull(studentResult.Value);

        var items = new List<BeltRankRecognitionListItem>
        {
            new(
                Guid.NewGuid(),
                beltRankId,
                "HOANG-TN",
                "Hoàng đai thiếu nhi",
                6,
                new DateOnly(2026, 7, 12),
                BeltRankRecognitionSource.BeltExam,
                examResultId,
                "Công nhận kết quả thi.")
        };

        var studentRepository = new Mock<IStudentRepository>();
        var recognitionRepository =
            new Mock<IBeltRankRecognitionRepository>();

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                studentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentResult.Value);

        recognitionRepository
            .Setup(x => x.ListByStudentAsync(
                tenantId,
                studentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var handler = new GetStudentBeltRecognitionsQueryHandler(
            studentRepository.Object,
            recognitionRepository.Object);

        var query = new GetStudentBeltRecognitionsQuery(
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
        Assert.Equal("HOANG-TN", item.BeltCode);
        Assert.Equal("Hoàng đai thiếu nhi", item.BeltName);
        Assert.Equal(
            BeltRankRecognitionSource.BeltExam,
            item.Source);
        Assert.Equal(examResultId, item.BeltExamStudentResultId);

        recognitionRepository.Verify(
            x => x.ListByStudentAsync(
                tenantId,
                studentId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_StudentNotFound_ShouldFailWithoutReadingRecognitions()
    {
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        var studentRepository = new Mock<IStudentRepository>();
        var recognitionRepository =
            new Mock<IBeltRankRecognitionRepository>();

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                studentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        var handler = new GetStudentBeltRecognitionsQueryHandler(
            studentRepository.Object,
            recognitionRepository.Object);

        var query = new GetStudentBeltRecognitionsQuery(
            tenantId,
            studentId);

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RECOGNITION_QUERY_003",
            result.Error.Code);

        recognitionRepository.Verify(
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
        var recognitionRepository =
            new Mock<IBeltRankRecognitionRepository>();

        var handler = new GetStudentBeltRecognitionsQueryHandler(
            studentRepository.Object,
            recognitionRepository.Object);

        var query = new GetStudentBeltRecognitionsQuery(
            Guid.Empty,
            Guid.NewGuid());

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RECOGNITION_QUERY_001",
            result.Error.Code);

        studentRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        recognitionRepository.Verify(
            x => x.ListByStudentAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_EmptyStudentId_ShouldFailWithoutAccessingRepositories()
    {
        var studentRepository = new Mock<IStudentRepository>();
        var recognitionRepository =
            new Mock<IBeltRankRecognitionRepository>();

        var handler = new GetStudentBeltRecognitionsQueryHandler(
            studentRepository.Object,
            recognitionRepository.Object);

        var query = new GetStudentBeltRecognitionsQuery(
            Guid.NewGuid(),
            Guid.Empty);

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RECOGNITION_QUERY_002",
            result.Error.Code);

        studentRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        recognitionRepository.Verify(
            x => x.ListByStudentAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}