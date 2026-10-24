using VovinamERP.Domain.Training;
using Xunit;

namespace VovinamERP.Tests;

public sealed class TrainingSessionTests
{
    private static TrainingSession CreateValidSession()
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
        Assert.NotNull(result.Value);

        return result.Value!;
    }

    [Fact]
    public void Create_WithValidData_StartsAsPlanned()
    {
        var session = CreateValidSession();

        Assert.Equal(TrainingSessionStatus.Planned, session.Status);
    }

    [Fact]
    public void Open_PlannedSession_Succeeds()
    {
        var session = CreateValidSession();

        var result = session.Open();

        Assert.True(result.IsSuccess);
        Assert.Equal(TrainingSessionStatus.Opened, session.Status);
    }

    [Fact]
    public void Close_OpenedSession_Succeeds()
    {
        var session = CreateValidSession();
        Assert.True(session.Open().IsSuccess);

        var result = session.Close();

        Assert.True(result.IsSuccess);
        Assert.Equal(TrainingSessionStatus.Closed, session.Status);
    }

    [Fact]
    public void Close_AlreadyClosedSession_Fails()
    {
        var session = CreateValidSession();
        Assert.True(session.Open().IsSuccess);
        Assert.True(session.Close().IsSuccess);

        var result = session.Close();

        Assert.True(result.IsFailure);
        Assert.Equal(
            TrainingErrors.SessionAlreadyClosed.Code,
            result.Error.Code);
    }

    [Fact]
    public void Close_PlannedSession_Fails()
    {
        var session = CreateValidSession();

        var result = session.Close();

        Assert.True(result.IsFailure);
        Assert.Equal(TrainingSessionStatus.Planned, session.Status);
    }

    [Fact]
    public void Open_ClosedSession_Fails()
    {
        var session = CreateValidSession();
        Assert.True(session.Open().IsSuccess);
        Assert.True(session.Close().IsSuccess);

        var result = session.Open();

        Assert.True(result.IsFailure);
        Assert.Equal(
            TrainingErrors.SessionAlreadyClosed.Code,
            result.Error.Code);
    }

    [Fact]
    public void Open_ArchivedSession_Fails()
    {
        var session = CreateValidSession();
        session.Archive(null);

        var result = session.Open();

        Assert.True(result.IsFailure);
        Assert.Equal(
            TrainingErrors.AlreadyArchived.Code,
            result.Error.Code);
    }

    [Fact]
    public void Close_ArchivedSession_Fails()
    {
        var session = CreateValidSession();
        session.Archive(null);

        var result = session.Close();

        Assert.True(result.IsFailure);
        Assert.Equal(
            TrainingErrors.AlreadyArchived.Code,
            result.Error.Code);
    }
}
