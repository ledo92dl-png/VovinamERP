using Moq;
using VovinamERP.Application.BeltExams.AddBeltExamStudentResult;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Students.Common;
using VovinamERP.Domain.BeltExams;
using VovinamERP.Domain.Students;

namespace VovinamERP.Tests;

public sealed class AddBeltExamStudentResultCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldAddResultWithoutChangingCurrentBelt()
    {
        var tenantId = Guid.NewGuid();
        var currentBeltId = Guid.NewGuid();
        var targetBeltId = Guid.NewGuid();

        var exam = CreateExam(tenantId, targetBeltId);

        var student = CreateStudent(
            tenantId,
            currentBeltId,
            "MS-EXAM-001");

        var originalBeltId = student.CurrentBeltRankId;

        var beltExamRepository =
            new Mock<IRepository<BeltExam>>();

        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        var studentRepository =
            new Mock<IStudentRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        beltExamRepository
            .Setup(x => x.GetByIdAsync(
                exam.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                student.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        resultRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<BeltExamStudentResult, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        BeltExamStudentResult? savedResult = null;

        resultRepository
            .Setup(x => x.AddAsync(
                It.IsAny<BeltExamStudentResult>(),
                It.IsAny<CancellationToken>()))
            .Callback<BeltExamStudentResult, CancellationToken>(
                (result, _) => savedResult = result)
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler =
            new AddBeltExamStudentResultCommandHandler(
                beltExamRepository.Object,
                resultRepository.Object,
                studentRepository.Object,
                unitOfWork.Object);

        var command =
            new AddBeltExamStudentResultCommand(
                tenantId,
                exam.Id,
                student.Id,
                "Tran Bien",
                "Dat",
                StudentBeltResult.Passed,
                36m,
                2,
                "Nhap tu phieu tong hop diem.",
                Guid.NewGuid());

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedResult);

        Assert.Equal(savedResult.Id, result.Value);
        Assert.Equal(tenantId, savedResult.TenantId);
        Assert.Equal(exam.Id, savedResult.BeltExamId);
        Assert.Equal(student.Id, savedResult.StudentId);
        Assert.Equal("Tran Bien", savedResult.UnitName);
        Assert.Equal("Dat", savedResult.SourceResult);
        Assert.Equal(
            StudentBeltResult.Passed,
            savedResult.Result);
        Assert.Equal(36m, savedResult.TotalScore);
        Assert.Equal(2, savedResult.Ranking);

        Assert.Equal(
            originalBeltId,
            student.CurrentBeltRankId);

        studentRepository.Verify(
            x => x.Update(It.IsAny<Student>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MissingOrWrongTenantExam_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        var beltExamRepository =
            new Mock<IRepository<BeltExam>>();

        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        var studentRepository =
            new Mock<IStudentRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        beltExamRepository
            .Setup(x => x.GetByIdAsync(
                examId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BeltExam?)null);

        var handler =
            new AddBeltExamStudentResultCommandHandler(
                beltExamRepository.Object,
                resultRepository.Object,
                studentRepository.Object,
                unitOfWork.Object);

        var command = CreateCommand(
            tenantId,
            examId,
            Guid.NewGuid());

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_EXAM_RESULT_APP_001",
            result.Error.Code);

        resultRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltExamStudentResult>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_MissingStudent_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();
        var exam = CreateExam(
            tenantId,
            Guid.NewGuid());

        var studentId = Guid.NewGuid();

        var beltExamRepository =
            new Mock<IRepository<BeltExam>>();

        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        var studentRepository =
            new Mock<IStudentRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        beltExamRepository
            .Setup(x => x.GetByIdAsync(
                exam.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                studentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        var handler =
            new AddBeltExamStudentResultCommandHandler(
                beltExamRepository.Object,
                resultRepository.Object,
                studentRepository.Object,
                unitOfWork.Object);

        var command = CreateCommand(
            tenantId,
            exam.Id,
            studentId);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_EXAM_RESULT_APP_002",
            result.Error.Code);

        resultRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltExamStudentResult>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateStudent_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();

        var exam = CreateExam(
            tenantId,
            Guid.NewGuid());

        var student = CreateStudent(
            tenantId,
            Guid.NewGuid(),
            "MS-EXAM-002");

        var beltExamRepository =
            new Mock<IRepository<BeltExam>>();

        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        var studentRepository =
            new Mock<IStudentRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        beltExamRepository
            .Setup(x => x.GetByIdAsync(
                exam.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                student.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        resultRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<BeltExamStudentResult, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler =
            new AddBeltExamStudentResultCommandHandler(
                beltExamRepository.Object,
                resultRepository.Object,
                studentRepository.Object,
                unitOfWork.Object);

        var command = CreateCommand(
            tenantId,
            exam.Id,
            student.Id);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_EXAM_RESULT_APP_003",
            result.Error.Code);

        resultRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltExamStudentResult>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ArchivedDuplicateStudent_ShouldFailWithoutSaving()
    {
        var tenantId = Guid.NewGuid();

        var exam = CreateExam(
            tenantId,
            Guid.NewGuid());

        var student = CreateStudent(
            tenantId,
            Guid.NewGuid(),
            "MS-EXAM-003");

        var archivedResultCreation = BeltExamStudentResult.Create(
            tenantId,
            exam.Id,
            student.Id,
            "Lý thuyết",
            "Đạt",
            StudentBeltResult.Passed,
            36m,
            1,
            "Kết quả cũ",
            null);

        Assert.True(archivedResultCreation.IsSuccess);
        Assert.NotNull(archivedResultCreation.Value);

        var archivedResult = archivedResultCreation.Value;

        archivedResult.Archive(null);

        var beltExamRepository =
            new Mock<IRepository<BeltExam>>();

        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        var studentRepository =
            new Mock<IStudentRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        beltExamRepository
            .Setup(x => x.GetByIdAsync(
                exam.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                student.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        resultRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<BeltExamStudentResult, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                System.Linq.Expressions.Expression<Func<BeltExamStudentResult, bool>> predicate,
                CancellationToken _) =>
                predicate.Compile()(archivedResult));

        var handler =
            new AddBeltExamStudentResultCommandHandler(
                beltExamRepository.Object,
                resultRepository.Object,
                studentRepository.Object,
                unitOfWork.Object);

        var command = CreateCommand(
            tenantId,
            exam.Id,
            student.Id);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "BELT_EXAM_RESULT_APP_003",
            result.Error.Code);

        resultRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltExamStudentResult>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task Handle_InvalidDomainData_ShouldReturnDomainErrorWithoutSaving()
    {
        var tenantId = Guid.NewGuid();

        var exam = CreateExam(
            tenantId,
            Guid.NewGuid());

        var student = CreateStudent(
            tenantId,
            Guid.NewGuid(),
            "MS-EXAM-003");

        var beltExamRepository =
            new Mock<IRepository<BeltExam>>();

        var resultRepository =
            new Mock<IRepository<BeltExamStudentResult>>();

        var studentRepository =
            new Mock<IStudentRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        beltExamRepository
            .Setup(x => x.GetByIdAsync(
                exam.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                student.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        resultRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<BeltExamStudentResult, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler =
            new AddBeltExamStudentResultCommandHandler(
                beltExamRepository.Object,
                resultRepository.Object,
                studentRepository.Object,
                unitOfWork.Object);

        var command =
            new AddBeltExamStudentResultCommand(
                tenantId,
                exam.Id,
                student.Id,
                "",
                "Dat",
                StudentBeltResult.Passed,
                36m,
                1,
                null,
                null);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_EXAM_RESULT_004",
            result.Error.Code);

        resultRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltExamStudentResult>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static AddBeltExamStudentResultCommand CreateCommand(
        Guid tenantId,
        Guid beltExamId,
        Guid studentId)
    {
        return new AddBeltExamStudentResultCommand(
            tenantId,
            beltExamId,
            studentId,
            "Tran Bien",
            "Dat",
            StudentBeltResult.Passed,
            36m,
            1,
            null,
            null);
    }

    private static BeltExam CreateExam(
        Guid tenantId,
        Guid targetBeltRankId)
    {
        var result = BeltExam.Create(
            tenantId,
            targetBeltRankId,
            new DateOnly(2026, 7, 12),
            "Khoa 1",
            "Thanh pho Dong Nai",
            "Hoang dai thieu nien",
            null,
            null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        return result.Value;
    }

    private static Student CreateStudent(
        Guid tenantId,
        Guid currentBeltRankId,
        string memberNumber)
    {
        var result = Student.Register(
            tenantId,
            Guid.NewGuid(),
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
}