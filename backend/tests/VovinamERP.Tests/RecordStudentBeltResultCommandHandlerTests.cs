using Moq;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Students.Common;
using VovinamERP.Application.Students.RecordStudentBeltResult;
using VovinamERP.Domain.Belts;
using VovinamERP.Domain.Persons;
using VovinamERP.Domain.Students;
using IPersonRepository =
    VovinamERP.Application.Common.Repositories.IPersonRepository;

namespace VovinamERP.Tests;

public sealed class RecordStudentBeltResultCommandHandlerTests
{
    [Fact]
    public async Task Handle_PassedResult_ShouldSaveHistoryAndChangeCurrentBelt()
    {
        var tenantId = Guid.NewGuid();

        var person = CreatePerson(
            tenantId,
            "P-001",
            null);

        var oldBelt = CreateBeltRank(
            "LAM-2",
            "Lam đai II",
            4);

        var newBelt = CreateBeltRank(
            "LAM-3",
            "Lam đai III",
            5);

        var student = CreateStudent(
            tenantId,
            person.Id,
            oldBelt.Id,
            "MS-001");

        var studentRepository = new Mock<IStudentRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var beltRankRepository = new Mock<IRepository<BeltRank>>();
        var beltHistoryRepository =
            new Mock<IRepository<StudentBeltHistory>>();
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
                newBelt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(newBelt);

        StudentBeltHistory? savedHistory = null;

        beltHistoryRepository
            .Setup(x => x.AddAsync(
                It.IsAny<StudentBeltHistory>(),
                It.IsAny<CancellationToken>()))
            .Callback<StudentBeltHistory, CancellationToken>(
                (history, _) => savedHistory = history)
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RecordStudentBeltResultCommandHandler(
            studentRepository.Object,
            personRepository.Object,
            beltRankRepository.Object,
            beltHistoryRepository.Object,
            unitOfWork.Object);

        var command = new RecordStudentBeltResultCommand(
            tenantId,
            student.Id,
            newBelt.Id,
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
        Assert.Equal(
            StudentBeltResult.Passed,
            savedHistory.Result);
        Assert.Equal(
            newBelt.Id,
            savedHistory.BeltRankId);
        Assert.Equal(
            newBelt.Id,
            student.CurrentBeltRankId);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_FailedResult_ShouldSaveHistoryAndKeepCurrentBelt()
    {
        var tenantId = Guid.NewGuid();

        var person = CreatePerson(
            tenantId,
            "P-002",
            new DateOnly(2014, 9, 30));

        var currentBelt = CreateBeltRank(
            "LAM-3",
            "Lam đai III",
            5);

        var attemptedBelt = CreateBeltRank(
            StudentBeltAgePolicy.YellowBeltCode,
            "Hoàng đai",
            6);

        var student = CreateStudent(
            tenantId,
            person.Id,
            currentBelt.Id,
            "MS-002");

        var studentRepository = new Mock<IStudentRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var beltRankRepository = new Mock<IRepository<BeltRank>>();
        var beltHistoryRepository =
            new Mock<IRepository<StudentBeltHistory>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                student.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        beltRankRepository
            .Setup(x => x.GetByIdAsync(
                attemptedBelt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(attemptedBelt);

        StudentBeltHistory? savedHistory = null;

        beltHistoryRepository
            .Setup(x => x.AddAsync(
                It.IsAny<StudentBeltHistory>(),
                It.IsAny<CancellationToken>()))
            .Callback<StudentBeltHistory, CancellationToken>(
                (history, _) => savedHistory = history)
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RecordStudentBeltResultCommandHandler(
            studentRepository.Object,
            personRepository.Object,
            beltRankRepository.Object,
            beltHistoryRepository.Object,
            unitOfWork.Object);

        var command = new RecordStudentBeltResultCommand(
            tenantId,
            student.Id,
            attemptedBelt.Id,
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
        Assert.Equal(
            StudentBeltResult.Failed,
            savedHistory.Result);
        Assert.Equal(
            currentBelt.Id,
            student.CurrentBeltRankId);

        personRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidBeltRank_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();

        var person = CreatePerson(
            tenantId,
            "P-003",
            null);

        var currentBelt = CreateBeltRank(
            "LAM-3",
            "Lam đai III",
            5);

        var student = CreateStudent(
            tenantId,
            person.Id,
            currentBelt.Id,
            "MS-003");

        var missingBeltId = Guid.NewGuid();

        var studentRepository = new Mock<IStudentRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var beltRankRepository = new Mock<IRepository<BeltRank>>();
        var beltHistoryRepository =
            new Mock<IRepository<StudentBeltHistory>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                student.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        beltRankRepository
            .Setup(x => x.GetByIdAsync(
                missingBeltId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BeltRank?)null);

        var handler = new RecordStudentBeltResultCommandHandler(
            studentRepository.Object,
            personRepository.Object,
            beltRankRepository.Object,
            beltHistoryRepository.Object,
            unitOfWork.Object);

        var command = new RecordStudentBeltResultCommand(
            tenantId,
            student.Id,
            missingBeltId,
            new DateOnly(2026, 9, 20),
            StudentBeltResult.Passed,
            new DateOnly(2026, 9, 20),
            null,
            null);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "STUDENT_BELT_002",
            result.Error.Code);

        beltHistoryRepository.Verify(
            x => x.AddAsync(
                It.IsAny<StudentBeltHistory>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Under12PassedYellowBelt_ShouldFailWithoutSaving()
    {
        var setup = CreateAgeRuleSetup(
            new DateOnly(2014, 9, 30),
            StudentBeltAgePolicy.YellowBeltCode,
            "Hoàng đai");

        var command = new RecordStudentBeltResultCommand(
            setup.TenantId,
            setup.Student.Id,
            setup.TargetBelt.Id,
            new DateOnly(2026, 9, 29),
            StudentBeltResult.Passed,
            new DateOnly(2026, 9, 29),
            null,
            null);

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "STUDENT_BELT_AGE_002",
            result.Error.Code);

        Assert.Equal(
            setup.CurrentBelt.Id,
            setup.Student.CurrentBeltRankId);

        setup.BeltHistoryRepository.Verify(
            x => x.AddAsync(
                It.IsAny<StudentBeltHistory>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Under12PassedJuniorYellowBelt_ShouldSucceed()
    {
        var setup = CreateAgeRuleSetup(
            new DateOnly(2014, 9, 30),
            StudentBeltAgePolicy.JuniorYellowBeltCode,
            "Hoàng đai thiếu nhi");

        var command = new RecordStudentBeltResultCommand(
            setup.TenantId,
            setup.Student.Id,
            setup.TargetBelt.Id,
            new DateOnly(2026, 9, 29),
            StudentBeltResult.Passed,
            new DateOnly(2026, 9, 29),
            null,
            null);

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            setup.TargetBelt.Id,
            setup.Student.CurrentBeltRankId);

        setup.BeltHistoryRepository.Verify(
            x => x.AddAsync(
                It.IsAny<StudentBeltHistory>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_OnTwelfthBirthdayPassedJuniorYellowBelt_ShouldFailWithoutSaving()
    {
        var setup = CreateAgeRuleSetup(
            new DateOnly(2014, 9, 30),
            StudentBeltAgePolicy.JuniorYellowBeltCode,
            "Hoàng đai thiếu nhi");

        var command = new RecordStudentBeltResultCommand(
            setup.TenantId,
            setup.Student.Id,
            setup.TargetBelt.Id,
            new DateOnly(2026, 9, 30),
            StudentBeltResult.Passed,
            new DateOnly(2026, 9, 30),
            null,
            null);

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "STUDENT_BELT_AGE_003",
            result.Error.Code);

        Assert.Equal(
            setup.CurrentBelt.Id,
            setup.Student.CurrentBeltRankId);

        setup.BeltHistoryRepository.Verify(
            x => x.AddAsync(
                It.IsAny<StudentBeltHistory>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_OnTwelfthBirthdayPassedYellowBelt_ShouldSucceed()
    {
        var setup = CreateAgeRuleSetup(
            new DateOnly(2014, 9, 30),
            StudentBeltAgePolicy.YellowBeltCode,
            "Hoàng đai");

        var command = new RecordStudentBeltResultCommand(
            setup.TenantId,
            setup.Student.Id,
            setup.TargetBelt.Id,
            new DateOnly(2026, 9, 30),
            StudentBeltResult.Passed,
            new DateOnly(2026, 9, 30),
            null,
            null);

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            setup.TargetBelt.Id,
            setup.Student.CurrentBeltRankId);

        setup.BeltHistoryRepository.Verify(
            x => x.AddAsync(
                It.IsAny<StudentBeltHistory>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static AgeRuleSetup CreateAgeRuleSetup(
        DateOnly dateOfBirth,
        string targetBeltCode,
        string targetBeltName)
    {
        var tenantId = Guid.NewGuid();

        var person = CreatePerson(
            tenantId,
            "P-AGE",
            dateOfBirth);

        var currentBelt = CreateBeltRank(
            "LAM-3",
            "Lam đai III",
            5);

        var targetBelt = CreateBeltRank(
            targetBeltCode,
            targetBeltName,
            6);

        var student = CreateStudent(
            tenantId,
            person.Id,
            currentBelt.Id,
            "MS-AGE");

        var studentRepository = new Mock<IStudentRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var beltRankRepository = new Mock<IRepository<BeltRank>>();
        var beltHistoryRepository =
            new Mock<IRepository<StudentBeltHistory>>();
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
                targetBelt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetBelt);

        beltHistoryRepository
            .Setup(x => x.AddAsync(
                It.IsAny<StudentBeltHistory>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RecordStudentBeltResultCommandHandler(
            studentRepository.Object,
            personRepository.Object,
            beltRankRepository.Object,
            beltHistoryRepository.Object,
            unitOfWork.Object);

        return new AgeRuleSetup(
            tenantId,
            student,
            currentBelt,
            targetBelt,
            handler,
            beltHistoryRepository,
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

    private sealed record AgeRuleSetup(
        Guid TenantId,
        Student Student,
        BeltRank CurrentBelt,
        BeltRank TargetBelt,
        RecordStudentBeltResultCommandHandler Handler,
        Mock<IRepository<StudentBeltHistory>> BeltHistoryRepository,
        Mock<IUnitOfWork> UnitOfWork);
}