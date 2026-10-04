using System.Linq.Expressions;
using Moq;
using VovinamERP.Application.BeltRecognitions.CreateManualBeltRecognition;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Students.Common;
using VovinamERP.Application.Students.RecordStudentBeltResult;
using VovinamERP.Domain.Belts;
using VovinamERP.Domain.BeltRecognitions;
using VovinamERP.Domain.Persons;
using VovinamERP.Domain.Students;
using IPersonRepository =
    VovinamERP.Application.Common.Repositories.IPersonRepository;

namespace VovinamERP.Tests;

public sealed class CreateManualBeltRecognitionCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidManualRecognition_ShouldCreateRecognitionAndChangeCurrentBelt()
    {
        var setup = CreateValidSetup();

        BeltRankRecognition? savedRecognition = null;

        setup.RecognitionRepository
            .Setup(x => x.AddAsync(
                It.IsAny<BeltRankRecognition>(),
                It.IsAny<CancellationToken>()))
            .Callback<BeltRankRecognition, CancellationToken>(
                (recognition, _) => savedRecognition = recognition)
            .Returns(Task.CompletedTask);

        var result = await setup.Handler.Handle(
            new CreateManualBeltRecognitionCommand(
                setup.TenantId,
                setup.Student.Id,
                setup.TargetBelt.Id,
                new DateOnly(2026, 7, 13),
                "Manual recognition",
                Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedRecognition);

        Assert.Equal(
            BeltRankRecognitionSource.Manual,
            savedRecognition!.Source);

        Assert.Null(
            savedRecognition.BeltExamStudentResultId);

        Assert.Equal(
            setup.TargetBelt.Id,
            setup.Student.CurrentBeltRankId);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LowerBeltThanCurrent_ShouldSaveWithoutRegressingCurrentBelt()
    {
        var setup = CreateValidSetup();

        var higherCurrentBelt = CreateBeltRank(
            "HOANG-1",
            "Hoang dai I",
            7);

        var changeResult = setup.Student.ChangeCurrentBelt(
            higherCurrentBelt.Id,
            new DateOnly(2026, 7, 12),
            "Existing higher belt",
            null);

        Assert.True(changeResult.IsSuccess);

        setup.BeltRankRepository
            .Setup(x => x.GetByIdAsync(
                higherCurrentBelt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(higherCurrentBelt);

        var result = await setup.Handler.Handle(
            new CreateManualBeltRecognitionCommand(
                setup.TenantId,
                setup.Student.Id,
                setup.TargetBelt.Id,
                new DateOnly(2026, 7, 13),
                "Historical lower belt",
                null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Equal(
            higherCurrentBelt.Id,
            setup.Student.CurrentBeltRankId);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_OlderThanExistingRecognition_ShouldSaveWithoutChangingCurrentBelt()
    {
        var setup = CreateValidSetup();

        var laterRecognitionResult =
            BeltRankRecognition.Create(
                setup.TenantId,
                setup.Student.Id,
                Guid.NewGuid(),
                new DateOnly(2026, 8, 1),
                BeltRankRecognitionSource.Manual,
                null,
                "Later recognition");

        Assert.True(laterRecognitionResult.IsSuccess);

        setup.RecognitionRepository
            .Setup(x => x.ListAsync(
                It.IsAny<Expression<Func<BeltRankRecognition, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<BeltRankRecognition>
                {
                    laterRecognitionResult.Value
                });

        var originalBeltId =
            setup.Student.CurrentBeltRankId;

        var result = await setup.Handler.Handle(
            new CreateManualBeltRecognitionCommand(
                setup.TenantId,
                setup.Student.Id,
                setup.TargetBelt.Id,
                new DateOnly(2026, 7, 13),
                "Historical recognition",
                null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Equal(
            originalBeltId,
            setup.Student.CurrentBeltRankId);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_StudentNotFound_ShouldFailWithoutSaving()
    {
        var setup = CreateValidSetup();

        setup.StudentRepository
            .Setup(x => x.GetByIdAsync(
                setup.TenantId,
                setup.Student.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        var result = await setup.Handler.Handle(
            new CreateManualBeltRecognitionCommand(
                setup.TenantId,
                setup.Student.Id,
                setup.TargetBelt.Id,
                new DateOnly(2026, 7, 13),
                null,
                null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RECOGNITION_MANUAL_APP_001",
            result.Error.Code);

        setup.RecognitionRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltRankRecognition>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task Handle_BeltRankNotFound_ShouldFailWithoutSaving()
    {
        var setup = CreateValidSetup();

        setup.BeltRankRepository
            .Setup(x => x.GetByIdAsync(
                setup.TargetBelt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BeltRank?)null);

        var result = await setup.Handler.Handle(
            new CreateManualBeltRecognitionCommand(
                setup.TenantId,
                setup.Student.Id,
                setup.TargetBelt.Id,
                new DateOnly(2026, 7, 13),
                null,
                null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RECOGNITION_MANUAL_APP_002",
            result.Error.Code);

        setup.RecognitionRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltRankRecognition>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task Handle_JuniorYellowBelt_UnderTwelve_ShouldSucceed()
    {
        var setup = CreateValidSetup(
            dateOfBirth: new DateOnly(2015, 7, 13),
            targetBeltCode:
                StudentBeltAgePolicy.JuniorYellowBeltCode,
            targetBeltName: "Hoang dai thieu nhi");

        var result = await setup.Handler.Handle(
            new CreateManualBeltRecognitionCommand(
                setup.TenantId,
                setup.Student.Id,
                setup.TargetBelt.Id,
                new DateOnly(2026, 7, 13),
                null,
                null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Equal(
            setup.TargetBelt.Id,
            setup.Student.CurrentBeltRankId);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_JuniorYellowBelt_OnTwelfthBirthday_ShouldFail()
    {
        var setup = CreateValidSetup(
            dateOfBirth: new DateOnly(2014, 7, 13),
            targetBeltCode:
                StudentBeltAgePolicy.JuniorYellowBeltCode,
            targetBeltName: "Hoang dai thieu nhi");

        var originalBeltId =
            setup.Student.CurrentBeltRankId;

        var result = await setup.Handler.Handle(
            new CreateManualBeltRecognitionCommand(
                setup.TenantId,
                setup.Student.Id,
                setup.TargetBelt.Id,
                new DateOnly(2026, 7, 13),
                null,
                null),
            CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            originalBeltId,
            setup.Student.CurrentBeltRankId);

        setup.RecognitionRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltRankRecognition>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_YellowBelt_OnTwelfthBirthday_ShouldSucceed()
    {
        var setup = CreateValidSetup(
            dateOfBirth: new DateOnly(2014, 7, 13),
            targetBeltCode:
                StudentBeltAgePolicy.YellowBeltCode,
            targetBeltName: "Hoang dai");

        var result = await setup.Handler.Handle(
            new CreateManualBeltRecognitionCommand(
                setup.TenantId,
                setup.Student.Id,
                setup.TargetBelt.Id,
                new DateOnly(2026, 7, 13),
                null,
                null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Equal(
            setup.TargetBelt.Id,
            setup.Student.CurrentBeltRankId);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    private static ManualRecognitionSetup CreateValidSetup(
        DateOnly? dateOfBirth = null,
        string targetBeltCode = "HOANG",
        string targetBeltName = "Hoang dai")
    {
        var tenantId = Guid.NewGuid();

        var person = CreatePerson(
            tenantId,
            dateOfBirth ?? new DateOnly(2010, 1, 1));

        var currentBelt = CreateBeltRank(
            "LAM-3",
            "Lam dai III",
            5);

        var targetBelt = CreateBeltRank(
            targetBeltCode,
            targetBeltName,
            6);

        var student = CreateStudent(
            tenantId,
            person.Id,
            currentBelt.Id);

        var beltRankRepository =
            new Mock<IRepository<BeltRank>>();

        var recognitionRepository =
            new Mock<IRepository<BeltRankRecognition>>();

        var historyRepository =
            new Mock<IRepository<StudentBeltHistory>>();

        var studentRepository =
            new Mock<IStudentRepository>();

        var personRepository =
            new Mock<IPersonRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        beltRankRepository
            .Setup(x => x.GetByIdAsync(
                targetBelt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetBelt);

        beltRankRepository
            .Setup(x => x.GetByIdAsync(
                currentBelt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentBelt);

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                student.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        recognitionRepository
            .Setup(x => x.ListAsync(
                It.IsAny<Expression<Func<BeltRankRecognition, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<BeltRankRecognition>());

        historyRepository
            .Setup(x => x.ListAsync(
                It.IsAny<Expression<Func<StudentBeltHistory, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<StudentBeltHistory>());

        recognitionRepository
            .Setup(x => x.AddAsync(
                It.IsAny<BeltRankRecognition>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        personRepository
            .Setup(x => x.GetByIdAsync(
                person.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler =
            new CreateManualBeltRecognitionCommandHandler(
                beltRankRepository.Object,
                recognitionRepository.Object,
                historyRepository.Object,
                studentRepository.Object,
                personRepository.Object,
                unitOfWork.Object);

        return new ManualRecognitionSetup(
            tenantId,
            person,
            student,
            targetBelt,
            beltRankRepository,
            recognitionRepository,
            historyRepository,
            studentRepository,
            personRepository,
            unitOfWork,
            handler);
    }

    private static Person CreatePerson(
        Guid tenantId,
        DateOnly? dateOfBirth)
    {
        var result = Person.Create(
            tenantId,
            "P-MANUAL-RECOGNITION",
            "Manual Recognition Student",
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
        Guid currentBeltRankId)
    {
        var result = Student.Register(
            tenantId,
            personId,
            Guid.NewGuid(),
            currentBeltRankId,
            "MS-MANUAL-RECOGNITION",
            new DateOnly(2026, 1, 1),
            null,
            null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        return result.Value;
    }

    private sealed record ManualRecognitionSetup(
        Guid TenantId,
        Person Person,
        Student Student,
        BeltRank TargetBelt,
        Mock<IRepository<BeltRank>> BeltRankRepository,
        Mock<IRepository<BeltRankRecognition>> RecognitionRepository,
        Mock<IRepository<StudentBeltHistory>> HistoryRepository,
        Mock<IStudentRepository> StudentRepository,
        Mock<IPersonRepository> PersonRepository,
        Mock<IUnitOfWork> UnitOfWork,
        CreateManualBeltRecognitionCommandHandler Handler);
}