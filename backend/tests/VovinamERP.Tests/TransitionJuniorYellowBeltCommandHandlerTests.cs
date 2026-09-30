using System.Linq.Expressions;
using Moq;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Students.Common;
using VovinamERP.Application.Students.RecordStudentBeltResult;
using VovinamERP.Application.Students.TransitionJuniorYellowBelt;
using VovinamERP.Domain.Belts;
using VovinamERP.Domain.Persons;
using VovinamERP.Domain.Students;
using IPersonRepository =
    VovinamERP.Application.Common.Repositories.IPersonRepository;

namespace VovinamERP.Tests;

public sealed class TransitionJuniorYellowBeltCommandHandlerTests
{
    [Fact]
    public async Task Handle_OnTwelfthBirthday_ShouldTransitionToYellowBelt()
    {
        var setup = CreateSetup(
            new DateOnly(2014, 9, 30),
            StudentBeltAgePolicy.JuniorYellowBeltCode,
            6,
            6);

        var command = new TransitionJuniorYellowBeltCommand(
            setup.TenantId,
            setup.Student.Id,
            new DateOnly(2026, 9, 30),
            "Chuyển Hoàng đai khi đủ 12 tuổi",
            Guid.NewGuid());

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            setup.YellowBelt.Id,
            setup.Student.CurrentBeltRankId);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_OneDayBeforeTwelfthBirthday_ShouldFail()
    {
        var setup = CreateSetup(
            new DateOnly(2014, 9, 30),
            StudentBeltAgePolicy.JuniorYellowBeltCode,
            6,
            6);

        var command = new TransitionJuniorYellowBeltCommand(
            setup.TenantId,
            setup.Student.Id,
            new DateOnly(2026, 9, 29),
            null,
            Guid.NewGuid());

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "STUDENT_BELT_TRANSITION_006",
            result.Error.Code);

        Assert.Equal(
            setup.CurrentBelt.Id,
            setup.Student.CurrentBeltRankId);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CurrentBeltIsNotJuniorYellow_ShouldFail()
    {
        var setup = CreateSetup(
            new DateOnly(2010, 1, 1),
            "LAM-3",
            5,
            6);

        var command = new TransitionJuniorYellowBeltCommand(
            setup.TenantId,
            setup.Student.Id,
            new DateOnly(2026, 9, 30),
            null,
            Guid.NewGuid());

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "STUDENT_BELT_TRANSITION_003",
            result.Error.Code);

        Assert.Equal(
            setup.CurrentBelt.Id,
            setup.Student.CurrentBeltRankId);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_MissingDateOfBirth_ShouldFail()
    {
        var setup = CreateSetup(
            null,
            StudentBeltAgePolicy.JuniorYellowBeltCode,
            6,
            6);

        var command = new TransitionJuniorYellowBeltCommand(
            setup.TenantId,
            setup.Student.Id,
            new DateOnly(2026, 9, 30),
            null,
            Guid.NewGuid());

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "STUDENT_BELT_TRANSITION_005",
            result.Error.Code);

        Assert.Equal(
            setup.CurrentBelt.Id,
            setup.Student.CurrentBeltRankId);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_YellowBeltsHaveDifferentLevels_ShouldFail()
    {
        var setup = CreateSetup(
            new DateOnly(2010, 1, 1),
            StudentBeltAgePolicy.JuniorYellowBeltCode,
            6,
            7);

        var command = new TransitionJuniorYellowBeltCommand(
            setup.TenantId,
            setup.Student.Id,
            new DateOnly(2026, 9, 30),
            null,
            Guid.NewGuid());

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "STUDENT_BELT_TRANSITION_008",
            result.Error.Code);

        Assert.Equal(
            setup.CurrentBelt.Id,
            setup.Student.CurrentBeltRankId);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static TransitionSetup CreateSetup(
        DateOnly? dateOfBirth,
        string currentBeltCode,
        int currentBeltLevel,
        int yellowBeltLevel)
    {
        var tenantId = Guid.NewGuid();

        var person = CreatePerson(
            tenantId,
            "P-TRANSITION",
            dateOfBirth);

        var currentBelt = CreateBeltRank(
            currentBeltCode,
            currentBeltCode == StudentBeltAgePolicy.JuniorYellowBeltCode
                ? "Hoàng đai thiếu nhi"
                : "Lam đai III",
            currentBeltLevel);

        var yellowBelt = CreateBeltRank(
            StudentBeltAgePolicy.YellowBeltCode,
            "Hoàng đai",
            yellowBeltLevel);

        var student = CreateStudent(
            tenantId,
            person.Id,
            currentBelt.Id,
            "MS-TRANSITION");

        var studentRepository = new Mock<IStudentRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var beltRankRepository = new Mock<IRepository<BeltRank>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                student.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        personRepository
            .Setup(x => x.GetByIdAsync(
                person.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        beltRankRepository
            .Setup(x => x.GetByIdAsync(
                currentBelt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentBelt);

        beltRankRepository
            .Setup(x => x.ListAsync(
                It.IsAny<Expression<Func<BeltRank, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BeltRank> { yellowBelt });

        unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new TransitionJuniorYellowBeltCommandHandler(
            studentRepository.Object,
            personRepository.Object,
            beltRankRepository.Object,
            unitOfWork.Object);

        return new TransitionSetup(
            tenantId,
            student,
            currentBelt,
            yellowBelt,
            handler,
            unitOfWork);
    }

    private static Person CreatePerson(
        Guid tenantId,
        string code,
        DateOnly? dateOfBirth)
    {
        var result = Person.Create(
            tenantId,
            code,
            "Môn sinh kiểm thử",
            default,
            dateOfBirth,
            null,
            null,
            null,
            null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        return result.Value;
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

    private static Student CreateStudent(
        Guid tenantId,
        Guid personId,
        Guid currentBeltRankId,
        string memberNumber)
    {
        var result = Student.Register(
            tenantId,
            personId,
            Guid.NewGuid(),
            currentBeltRankId,
            memberNumber,
            new DateOnly(2026, 1, 1),
            null,
            null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        return result.Value;
    }

    private sealed record TransitionSetup(
        Guid TenantId,
        Student Student,
        BeltRank CurrentBelt,
        BeltRank YellowBelt,
        TransitionJuniorYellowBeltCommandHandler Handler,
        Mock<IUnitOfWork> UnitOfWork);
}