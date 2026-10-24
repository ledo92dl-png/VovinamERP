using Moq;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Training.OpenTrainingSession;
using VovinamERP.Application.Training.CloseTrainingSession;
using VovinamERP.Domain.Training;
using Xunit;

namespace VovinamERP.Tests;

public sealed class TrainingSessionCommandHandlerTests
{
    private static TrainingSession CreateSession()
    {
        var result = TrainingSession.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 10, 23),
            new TimeOnly(18, 0),
            new TimeOnly(19, 30),
            null,
            null);

        Assert.True(result.IsSuccess);
        return result.Value!;
    }

    [Fact]
    public async Task Open_ValidSession_SavesChanges()
    {
        var session = CreateSession();

        var repository = new Mock<IRepository<TrainingSession>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.GetByIdAsync(
                session.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new OpenTrainingSessionCommandHandler(
            repository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(
            new OpenTrainingSessionCommand(
                session.TenantId,
                session.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TrainingSessionStatus.Opened, session.Status);

        repository.Verify(x => x.Update(session), Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Open_WrongTenant_FailsWithoutSaving()
    {
        var session = CreateSession();

        var repository = new Mock<IRepository<TrainingSession>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.GetByIdAsync(
                session.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new OpenTrainingSessionCommandHandler(
            repository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(
            new OpenTrainingSessionCommand(
                Guid.NewGuid(),
                session.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(TrainingSessionStatus.Planned, session.Status);

        repository.Verify(
            x => x.Update(It.IsAny<TrainingSession>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Open_MissingSession_FailsWithoutSaving()
    {
        var repository = new Mock<IRepository<TrainingSession>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new OpenTrainingSessionCommandHandler(
            repository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(
            new OpenTrainingSessionCommand(
                Guid.NewGuid(),
                Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsFailure);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Close_OpenedSession_SavesChanges()
    {
        var session = CreateSession();
        Assert.True(session.Open().IsSuccess);

        var repository = new Mock<IRepository<TrainingSession>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.GetByIdAsync(
                session.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new CloseTrainingSessionCommandHandler(
            repository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(
            new CloseTrainingSessionCommand(
                session.TenantId,
                session.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TrainingSessionStatus.Closed, session.Status);

        repository.Verify(x => x.Update(session), Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Close_PlannedSession_FailsWithoutSaving()
    {
        var session = CreateSession();

        var repository = new Mock<IRepository<TrainingSession>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.GetByIdAsync(
                session.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new CloseTrainingSessionCommandHandler(
            repository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(
            new CloseTrainingSessionCommand(
                session.TenantId,
                session.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            TrainingErrors.SessionNotOpened.Code,
            result.Error.Code);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Close_AlreadyClosedSession_FailsWithoutSaving()
    {
        var session = CreateSession();
        Assert.True(session.Open().IsSuccess);
        Assert.True(session.Close().IsSuccess);

        var repository = new Mock<IRepository<TrainingSession>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.GetByIdAsync(
                session.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new CloseTrainingSessionCommandHandler(
            repository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(
            new CloseTrainingSessionCommand(
                session.TenantId,
                session.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            TrainingErrors.SessionAlreadyClosed.Code,
            result.Error.Code);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Close_WrongTenant_FailsWithoutSaving()
    {
        var session = CreateSession();
        Assert.True(session.Open().IsSuccess);

        var repository = new Mock<IRepository<TrainingSession>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.GetByIdAsync(
                session.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new CloseTrainingSessionCommandHandler(
            repository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(
            new CloseTrainingSessionCommand(
                Guid.NewGuid(),
                session.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(TrainingSessionStatus.Opened, session.Status);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Close_MissingSession_FailsWithoutSaving()
    {
        var repository = new Mock<IRepository<TrainingSession>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CloseTrainingSessionCommandHandler(
            repository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(
            new CloseTrainingSessionCommand(
                Guid.NewGuid(),
                Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsFailure);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
