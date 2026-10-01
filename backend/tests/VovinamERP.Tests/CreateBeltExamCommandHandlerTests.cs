using Moq;
using VovinamERP.Application.BeltExams.CreateBeltExam;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.BeltExams;
using VovinamERP.Domain.Belts;

namespace VovinamERP.Tests;

public sealed class CreateBeltExamCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateBeltExam()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var targetBelt = CreateBeltRank(
            "HOANG-TN",
            "Hoang dai thieu nhi",
            6);

        var beltExamRepository = new Mock<IRepository<BeltExam>>();
        var beltRankRepository = new Mock<IRepository<BeltRank>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        beltRankRepository
            .Setup(x => x.GetByIdAsync(
                targetBelt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetBelt);

        BeltExam? savedExam = null;

        beltExamRepository
            .Setup(x => x.AddAsync(
                It.IsAny<BeltExam>(),
                It.IsAny<CancellationToken>()))
            .Callback<BeltExam, CancellationToken>(
                (exam, _) => savedExam = exam)
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateBeltExamCommandHandler(
            beltExamRepository.Object,
            beltRankRepository.Object,
            unitOfWork.Object);

        var command = new CreateBeltExamCommand(
            tenantId,
            targetBelt.Id,
            new DateOnly(2026, 7, 12),
            "Khoa 1",
            "Thanh pho Dong Nai",
            "Hoang dai thieu nien",
            "Nhap tu phieu tong hop diem.",
            userId);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedExam);
        Assert.Equal(savedExam.Id, result.Value);
        Assert.Equal(tenantId, savedExam.TenantId);
        Assert.Equal(targetBelt.Id, savedExam.TargetBeltRankId);
        Assert.Equal(
            new DateOnly(2026, 7, 12),
            savedExam.ExamDate);
        Assert.Equal("Khoa 1", savedExam.SessionName);
        Assert.Equal(
            "Thanh pho Dong Nai",
            savedExam.Location);
        Assert.Equal(
            "Hoang dai thieu nien",
            savedExam.SourceBeltName);
        Assert.Equal(userId, savedExam.CreatedBy);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MissingTargetBelt_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();
        var missingBeltId = Guid.NewGuid();

        var beltExamRepository = new Mock<IRepository<BeltExam>>();
        var beltRankRepository = new Mock<IRepository<BeltRank>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        beltRankRepository
            .Setup(x => x.GetByIdAsync(
                missingBeltId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BeltRank?)null);

        var handler = new CreateBeltExamCommandHandler(
            beltExamRepository.Object,
            beltRankRepository.Object,
            unitOfWork.Object);

        var command = CreateCommand(
            tenantId,
            missingBeltId);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_EXAM_APP_001",
            result.Error.Code);

        beltExamRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltExam>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveTargetBelt_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();

        var targetBelt = CreateBeltRank(
            "HOANG-TN",
            "Hoang dai thieu nhi",
            6);

        targetBelt.Deactivate();

        var beltExamRepository = new Mock<IRepository<BeltExam>>();
        var beltRankRepository = new Mock<IRepository<BeltRank>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        beltRankRepository
            .Setup(x => x.GetByIdAsync(
                targetBelt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetBelt);

        var handler = new CreateBeltExamCommandHandler(
            beltExamRepository.Object,
            beltRankRepository.Object,
            unitOfWork.Object);

        var command = CreateCommand(
            tenantId,
            targetBelt.Id);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_EXAM_APP_001",
            result.Error.Code);

        beltExamRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltExam>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ArchivedTargetBelt_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();

        var targetBelt = CreateBeltRank(
            "HOANG-TN",
            "Hoang dai thieu nhi",
            6);

        targetBelt.Archive(null);

        var beltExamRepository = new Mock<IRepository<BeltExam>>();
        var beltRankRepository = new Mock<IRepository<BeltRank>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        beltRankRepository
            .Setup(x => x.GetByIdAsync(
                targetBelt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetBelt);

        var handler = new CreateBeltExamCommandHandler(
            beltExamRepository.Object,
            beltRankRepository.Object,
            unitOfWork.Object);

        var command = CreateCommand(
            tenantId,
            targetBelt.Id);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_EXAM_APP_001",
            result.Error.Code);

        beltExamRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltExam>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    private static CreateBeltExamCommand CreateCommand(
        Guid tenantId,
        Guid targetBeltRankId)
    {
        return new CreateBeltExamCommand(
            tenantId,
            targetBeltRankId,
            new DateOnly(2026, 7, 12),
            "Khoa 1",
            "Thanh pho Dong Nai",
            "Hoang dai thieu nien",
            null,
            null);
    }

    private static BeltRank CreateBeltRank(
        string code,
        string name,
        int level)
    {
        var result = BeltRank.Create(
            code,
            name,
            level,
            null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        return result.Value;
    }
}