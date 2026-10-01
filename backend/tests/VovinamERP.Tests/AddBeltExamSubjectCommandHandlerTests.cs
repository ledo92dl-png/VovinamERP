using Moq;
using VovinamERP.Application.BeltExams.AddBeltExamSubject;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.BeltExams;

namespace VovinamERP.Tests;

public sealed class AddBeltExamSubjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateSubject()
    {
        var tenantId = Guid.NewGuid();
        var beltExam = CreateBeltExam(tenantId);

        var beltExamRepository = new Mock<IRepository<BeltExam>>();
        var subjectRepository = new Mock<IRepository<BeltExamSubject>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        beltExamRepository
            .Setup(x => x.GetByIdAsync(
                beltExam.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(beltExam);

        subjectRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<BeltExamSubject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        BeltExamSubject? savedSubject = null;

        subjectRepository
            .Setup(x => x.AddAsync(
                It.IsAny<BeltExamSubject>(),
                It.IsAny<CancellationToken>()))
            .Callback<BeltExamSubject, CancellationToken>(
                (subject, _) => savedSubject = subject)
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new AddBeltExamSubjectCommandHandler(
            beltExamRepository.Object,
            subjectRepository.Object,
            unitOfWork.Object);

        var command = new AddBeltExamSubjectCommand(
            tenantId,
            beltExam.Id,
            "Quyền",
            2,
            10m,
            null);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedSubject);
        Assert.Equal(savedSubject.Id, result.Value);
        Assert.Equal(tenantId, savedSubject.TenantId);
        Assert.Equal(beltExam.Id, savedSubject.BeltExamId);
        Assert.Equal("Quyền", savedSubject.Name);
        Assert.Equal(2, savedSubject.DisplayOrder);
        Assert.Equal(10m, savedSubject.MaximumScore);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MissingBeltExam_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();
        var beltExamId = Guid.NewGuid();

        var beltExamRepository = new Mock<IRepository<BeltExam>>();
        var subjectRepository = new Mock<IRepository<BeltExamSubject>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        beltExamRepository
            .Setup(x => x.GetByIdAsync(
                beltExamId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BeltExam?)null);

        var handler = new AddBeltExamSubjectCommandHandler(
            beltExamRepository.Object,
            subjectRepository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(
            CreateCommand(tenantId, beltExamId),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_EXAM_SUBJECT_APP_001",
            result.Error.Code);

        subjectRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltExamSubject>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_BeltExamFromDifferentTenant_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var beltExam = CreateBeltExam(otherTenantId);

        var beltExamRepository = new Mock<IRepository<BeltExam>>();
        var subjectRepository = new Mock<IRepository<BeltExamSubject>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        beltExamRepository
            .Setup(x => x.GetByIdAsync(
                beltExam.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(beltExam);

        var handler = new AddBeltExamSubjectCommandHandler(
            beltExamRepository.Object,
            subjectRepository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(
            CreateCommand(tenantId, beltExam.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_EXAM_SUBJECT_APP_001",
            result.Error.Code);

        subjectRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltExamSubject>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateDisplayOrder_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();
        var beltExam = CreateBeltExam(tenantId);

        var beltExamRepository = new Mock<IRepository<BeltExam>>();
        var subjectRepository = new Mock<IRepository<BeltExamSubject>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        beltExamRepository
            .Setup(x => x.GetByIdAsync(
                beltExam.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(beltExam);

        subjectRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<BeltExamSubject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new AddBeltExamSubjectCommandHandler(
            beltExamRepository.Object,
            subjectRepository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(
            CreateCommand(tenantId, beltExam.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_EXAM_SUBJECT_APP_002",
            result.Error.Code);

        subjectRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltExamSubject>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateName_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();
        var beltExam = CreateBeltExam(tenantId);

        var beltExamRepository = new Mock<IRepository<BeltExam>>();
        var subjectRepository = new Mock<IRepository<BeltExamSubject>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        beltExamRepository
            .Setup(x => x.GetByIdAsync(
                beltExam.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(beltExam);

        subjectRepository
            .SetupSequence(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<BeltExamSubject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .ReturnsAsync(true);

        var handler = new AddBeltExamSubjectCommandHandler(
            beltExamRepository.Object,
            subjectRepository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(
            CreateCommand(tenantId, beltExam.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_EXAM_SUBJECT_APP_003",
            result.Error.Code);

        subjectRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltExamSubject>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static AddBeltExamSubjectCommand CreateCommand(
        Guid tenantId,
        Guid beltExamId)
    {
        return new AddBeltExamSubjectCommand(
            tenantId,
            beltExamId,
            "Lý thuyết",
            1,
            10m,
            null);
    }

    private static BeltExam CreateBeltExam(Guid tenantId)
    {
        var result = BeltExam.Create(
            tenantId,
            Guid.NewGuid(),
            new DateOnly(2026, 7, 12),
            "Khóa 1",
            "Thành phố Đồng Nai",
            "Hoàng đai thiếu niên",
            null,
            null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        return result.Value;
    }
}