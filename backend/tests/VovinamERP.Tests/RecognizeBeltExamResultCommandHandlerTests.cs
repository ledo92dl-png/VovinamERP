using System.Linq.Expressions;
using Moq;
using VovinamERP.Application.BeltRecognitions.RecognizeBeltExamResult;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Students.Common;
using VovinamERP.Application.Students.RecordStudentBeltResult;
using VovinamERP.Domain.Belts;
using VovinamERP.Domain.BeltExams;
using VovinamERP.Domain.BeltRecognitions;
using VovinamERP.Domain.Persons;
using VovinamERP.Domain.Students;
using IPersonRepository =
    VovinamERP.Application.Common.Repositories.IPersonRepository;

namespace VovinamERP.Tests;

public sealed class RecognizeBeltExamResultCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidPassedResult_ShouldCreateRecognitionAndChangeCurrentBelt()
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

        var userId = Guid.NewGuid();

        var result = await setup.Handler.Handle(
            new RecognizeBeltExamResultCommand(
                setup.TenantId,
                setup.StudentResult.Id,
                new DateOnly(2026, 7, 13),
                "Công nhận kết quả thi",
                userId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedRecognition);

        Assert.Equal(savedRecognition.Id, result.Value);
        Assert.Equal(setup.TenantId, savedRecognition.TenantId);
        Assert.Equal(setup.Student.Id, savedRecognition.StudentId);
        Assert.Equal(setup.TargetBelt.Id, savedRecognition.BeltRankId);
        Assert.Equal(
            BeltRankRecognitionSource.BeltExam,
            savedRecognition.Source);
        Assert.Equal(
            setup.StudentResult.Id,
            savedRecognition.BeltExamStudentResultId);
        Assert.Equal(
            new DateOnly(2026, 7, 13),
            savedRecognition.RecognitionDate);

        Assert.Equal(
            setup.TargetBelt.Id,
            setup.Student.CurrentBeltRankId);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ResultNotFound_ShouldFailWithoutSaving()
    {
        var setup = CreateValidSetup();

        setup.ResultRepository
            .Setup(x => x.GetByIdAsync(
                setup.StudentResult.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BeltExamStudentResult?)null);

        await AssertFailureWithoutSaving(
            setup,
            "BELT_RECOGNITION_APP_001");
    }

    [Fact]
    public async Task Handle_WrongTenantResult_ShouldFailWithoutSaving()
    {
        var setup = CreateValidSetup();

        var command = new RecognizeBeltExamResultCommand(
            Guid.NewGuid(),
            setup.StudentResult.Id,
            new DateOnly(2026, 7, 13),
            null,
            null);

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RECOGNITION_APP_001",
            result.Error.Code);

        VerifyNothingSaved(setup);
    }

    [Fact]
    public async Task Handle_FailedExamResult_ShouldFailWithoutSaving()
    {
        var setup = CreateValidSetup(
            StudentBeltResult.Failed);

        await AssertFailureWithoutSaving(
            setup,
            "BELT_RECOGNITION_APP_002");
    }

    [Fact]
    public async Task Handle_ExamNotFound_ShouldFailWithoutSaving()
    {
        var setup = CreateValidSetup();

        setup.ExamRepository
            .Setup(x => x.GetByIdAsync(
                setup.Exam.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BeltExam?)null);

        await AssertFailureWithoutSaving(
            setup,
            "BELT_RECOGNITION_APP_003");
    }

    [Fact]
    public async Task Handle_RecognitionDateBeforeExamDate_ShouldFailWithoutSaving()
    {
        var setup = CreateValidSetup();

        var result = await setup.Handler.Handle(
            new RecognizeBeltExamResultCommand(
                setup.TenantId,
                setup.StudentResult.Id,
                new DateOnly(2026, 7, 11),
                null,
                null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RECOGNITION_APP_004",
            result.Error.Code);

        VerifyNothingSaved(setup);
    }

    [Fact]
    public async Task Handle_InactiveTargetBelt_ShouldFailWithoutSaving()
    {
        var setup = CreateValidSetup(
            deactivateTargetBelt: true);

        await AssertFailureWithoutSaving(
            setup,
            "BELT_RECOGNITION_APP_005");
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

        await AssertFailureWithoutSaving(
            setup,
            "BELT_RECOGNITION_APP_006");
    }

    [Fact]
    public async Task Handle_DuplicateRecognition_ShouldFailWithoutSaving()
    {
        var setup = CreateValidSetup();

        setup.RecognitionRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<Expression<Func<BeltRankRecognition, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await AssertFailureWithoutSaving(
            setup,
            "BELT_RECOGNITION_APP_007");
    }

    [Fact]
    public async Task Handle_MissingPerson_ShouldFailWithoutSaving()
    {
        var setup = CreateValidSetup();

        setup.PersonRepository
            .Setup(x => x.GetByIdAsync(
                setup.Person.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        await AssertFailureWithoutSaving(
            setup,
            "BELT_RECOGNITION_APP_008");
    }

    [Fact]
    public async Task Handle_JuniorYellowBelt_UnderTwelve_ShouldSucceed()
    {
        var setup = CreateValidSetup(
            dateOfBirth: new DateOnly(2015, 7, 13),
            targetBeltCode:
                StudentBeltAgePolicy.JuniorYellowBeltCode,
            targetBeltName: "Hoàng đai thiếu nhi");

        var result = await setup.Handler.Handle(
            new RecognizeBeltExamResultCommand(
                setup.TenantId,
                setup.StudentResult.Id,
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
            targetBeltName: "Hoàng đai thiếu nhi");

        var originalBeltId =
            setup.Student.CurrentBeltRankId;

        var result = await setup.Handler.Handle(
            new RecognizeBeltExamResultCommand(
                setup.TenantId,
                setup.StudentResult.Id,
                new DateOnly(2026, 7, 13),
                null,
                null),
            CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            originalBeltId,
            setup.Student.CurrentBeltRankId);

        VerifyNothingSaved(setup);
    }

    [Fact]
    public async Task Handle_YellowBelt_OnTwelfthBirthday_ShouldSucceed()
    {
        var setup = CreateValidSetup(
            dateOfBirth: new DateOnly(2014, 7, 13),
            targetBeltCode:
                StudentBeltAgePolicy.YellowBeltCode,
            targetBeltName: "Hoàng đai");

        var result = await setup.Handler.Handle(
            new RecognizeBeltExamResultCommand(
                setup.TenantId,
                setup.StudentResult.Id,
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
    private static RecognitionSetup CreateValidSetup(
        StudentBeltResult examResult =
            StudentBeltResult.Passed,
        bool deactivateTargetBelt = false,
        DateOnly? dateOfBirth = null,
        string targetBeltCode = "HOANG",
        string targetBeltName = "Hoàng đai")
    {
        var tenantId = Guid.NewGuid();

        var person = CreatePerson(
            tenantId,
            dateOfBirth ?? new DateOnly(2010, 1, 1));

        var currentBelt = CreateBeltRank(
            "LAM-3",
            "Lam đai III",
            5);

        var targetBelt = CreateBeltRank(
            targetBeltCode,
            targetBeltName,
            6);

        if (deactivateTargetBelt)
        {
            targetBelt.Deactivate();
        }

        var student = CreateStudent(
            tenantId,
            person.Id,
            currentBelt.Id);

        var exam = CreateExam(
            tenantId,
            targetBelt.Id);

        var studentResult = CreateStudentResult(
            tenantId,
            exam.Id,
            student.Id,
            examResult);

        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        var examRepository =
            new Mock<IRepository<BeltExam>>();

        var beltRankRepository =
            new Mock<IRepository<BeltRank>>();

        var recognitionRepository =
            new Mock<IRepository<BeltRankRecognition>>();

        var studentRepository =
            new Mock<IStudentRepository>();

        var personRepository =
            new Mock<IPersonRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        resultRepository
            .Setup(x => x.GetByIdAsync(
                studentResult.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentResult);

        examRepository
            .Setup(x => x.GetByIdAsync(
                exam.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);

        beltRankRepository
            .Setup(x => x.GetByIdAsync(
                targetBelt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetBelt);

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                student.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        recognitionRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<Expression<Func<BeltRankRecognition, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

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
            new RecognizeBeltExamResultCommandHandler(
                resultRepository.Object,
                examRepository.Object,
                beltRankRepository.Object,
                recognitionRepository.Object,
                studentRepository.Object,
                personRepository.Object,
                unitOfWork.Object);

        return new RecognitionSetup(
            tenantId,
            person,
            student,
            targetBelt,
            exam,
            studentResult,
            resultRepository,
            examRepository,
            recognitionRepository,
            studentRepository,
            personRepository,
            unitOfWork,
            handler);
    }

    private static async Task AssertFailureWithoutSaving(
        RecognitionSetup setup,
        string expectedErrorCode)
    {
        var originalBeltId =
            setup.Student.CurrentBeltRankId;

        var result = await setup.Handler.Handle(
            new RecognizeBeltExamResultCommand(
                setup.TenantId,
                setup.StudentResult.Id,
                new DateOnly(2026, 7, 13),
                null,
                null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            expectedErrorCode,
            result.Error.Code);

        Assert.Equal(
            originalBeltId,
            setup.Student.CurrentBeltRankId);

        VerifyNothingSaved(setup);
    }

    private static void VerifyNothingSaved(
        RecognitionSetup setup)
    {
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

    private static Person CreatePerson(
        Guid tenantId,
        DateOnly? dateOfBirth)
    {
        var result = Person.Create(
            tenantId,
            "P-RECOGNITION",
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
        Guid currentBeltRankId)
    {
        var result = Student.Register(
            tenantId,
            personId,
            Guid.NewGuid(),
            currentBeltRankId,
            "MS-RECOGNITION",
            new DateOnly(2026, 1, 1),
            null,
            null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        return result.Value;
    }

    private static BeltExam CreateExam(
        Guid tenantId,
        Guid targetBeltRankId)
    {
        var result = BeltExam.Create(
            tenantId,
            targetBeltRankId,
            new DateOnly(2026, 7, 12),
            "Khóa 1",
            "Thành phố Đồng Nai",
            "Hoàng đai",
            null,
            null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        return result.Value;
    }

    private static BeltExamStudentResult CreateStudentResult(
        Guid tenantId,
        Guid examId,
        Guid studentId,
        StudentBeltResult examResult)
    {
        var sourceResult =
            examResult == StudentBeltResult.Passed
                ? "Đạt"
                : "Không đạt";

        var result = BeltExamStudentResult.Create(
            tenantId,
            examId,
            studentId,
            "Trấn Biên",
            sourceResult,
            examResult,
            36m,
            1,
            null,
            null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        return result.Value;
    }

    private sealed record RecognitionSetup(
        Guid TenantId,
        Person Person,
        Student Student,
        BeltRank TargetBelt,
        BeltExam Exam,
        BeltExamStudentResult StudentResult,
        Mock<IRepository<BeltExamStudentResult>> ResultRepository,
        Mock<IRepository<BeltExam>> ExamRepository,
        Mock<IRepository<BeltRankRecognition>> RecognitionRepository,
        Mock<IStudentRepository> StudentRepository,
        Mock<IPersonRepository> PersonRepository,
        Mock<IUnitOfWork> UnitOfWork,
        RecognizeBeltExamResultCommandHandler Handler);
}
