using Microsoft.EntityFrameworkCore;
using Moq;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Finance.GenerateMonthlyTuitionInvoice;
using VovinamERP.Domain.Finance;
using VovinamERP.Domain.Students;
using VovinamERP.Infrastructure.Persistence.Repositories;
using VovinamERP.Infrastructure.Repositories;

namespace VovinamERP.Tests;

public class GenerateMonthlyTuitionInvoiceIntegrationTests
{
    [Fact]
    public async Task ExistingCredit100000_ShouldApplyToNewInvoice()
    {
        await using var db = TestDatabase.CreateContext();

        var tenantId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var studentResult = Student.Register(
            tenantId,
            personId,
            organizationId,
            null,
            $"MS-{Guid.NewGuid():N}",
            new DateOnly(2026, 1, 1),
            null,
            null,
            null);

        Assert.True(studentResult.IsSuccess);
        Assert.NotNull(studentResult.Value);

        var student = studentResult.Value!;

        db.Students.Add(student);

        var creditResult = StudentCreditTransaction.Create(
            tenantId,
            student.Id,
            StudentCreditTransactionType.Credit,
            100_000m,
            new DateOnly(2026, 8, 1),
            null,
            null,
            null,
            "Existing student credit");

        Assert.True(creditResult.IsSuccess);
        Assert.NotNull(creditResult.Value);

        db.StudentCreditTransactions.Add(creditResult.Value!);

        await db.SaveChangesAsync();

        var attendanceRepository =
            new Mock<IAttendanceRepository>();

        attendanceRepository
            .Setup(x =>
                x.CountStudentAttendancesByMonthAsync(
                    tenantId,
                    student.Id,
                    2026,
                    9,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(6);

        var studentCreditRepository =
            new StudentCreditRepository(db);

        var handler =
            new GenerateMonthlyTuitionInvoiceCommandHandler(
                new TuitionInvoiceRepository(db),
                studentCreditRepository,
                attendanceRepository.Object,
                new VovinamERP.Infrastructure.Repositories.StudentRepository(db),
db,
new VovinamERP.Infrastructure.Persistence.StudentCreditLockTransaction(db));

        var command =
            new GenerateMonthlyTuitionInvoiceCommand(
                tenantId,
                student.Id,
                $"HP-CREDIT-{Guid.NewGuid():N}",
                2026,
                9,
                300_000m,
                null);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.Equal(300_000m, result.PayableAmount);
        Assert.Equal(200_000m, result.BalanceAmount);

        db.ChangeTracker.Clear();

        var persistedInvoice =
            await db.TuitionInvoices
                .SingleAsync(x => x.Id == result.TuitionInvoiceId);

        Assert.Equal(200_000m, persistedInvoice.BalanceAmount);

        var debit =
            await db.StudentCreditTransactions
                .SingleAsync(x =>
                    x.TuitionInvoiceId == persistedInvoice.Id &&
                    x.TransactionType ==
                        StudentCreditTransactionType.Debit);

        Assert.Equal(100_000m, debit.Amount);
        Assert.Equal(student.Id, debit.StudentId);

        var remainingCredit =
            await studentCreditRepository.GetBalanceAsync(
                tenantId,
                student.Id);

        Assert.Equal(0m, remainingCredit);
    }

    [Fact]
    public async Task ExistingCredit500000_ShouldApplyOnlyInvoiceBalance()
    {
        await using var db = TestDatabase.CreateContext();

        var tenantId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var studentResult = Student.Register(
            tenantId,
            personId,
            organizationId,
            null,
            $"MS-{Guid.NewGuid():N}",
            new DateOnly(2026, 1, 1),
            null,
            null,
            null);

        Assert.True(studentResult.IsSuccess);
        Assert.NotNull(studentResult.Value);

        var student = studentResult.Value!;

        db.Students.Add(student);

        var creditResult = StudentCreditTransaction.Create(
            tenantId,
            student.Id,
            StudentCreditTransactionType.Credit,
            500_000m,
            new DateOnly(2026, 8, 1),
            null,
            null,
            null,
            "Existing student credit");

        Assert.True(creditResult.IsSuccess);
        Assert.NotNull(creditResult.Value);

        db.StudentCreditTransactions.Add(creditResult.Value!);

        await db.SaveChangesAsync();

        var attendanceRepository =
            new Mock<IAttendanceRepository>();

        attendanceRepository
            .Setup(x =>
                x.CountStudentAttendancesByMonthAsync(
                    tenantId,
                    student.Id,
                    2026,
                    9,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(6);

        var studentCreditRepository =
            new StudentCreditRepository(db);

        var handler =
            new GenerateMonthlyTuitionInvoiceCommandHandler(
                new TuitionInvoiceRepository(db),
                studentCreditRepository,
                attendanceRepository.Object,
                new VovinamERP.Infrastructure.Repositories.StudentRepository(db),
db,
new VovinamERP.Infrastructure.Persistence.StudentCreditLockTransaction(db));

        var command =
            new GenerateMonthlyTuitionInvoiceCommand(
                tenantId,
                student.Id,
                $"HP-CREDIT-{Guid.NewGuid():N}",
                2026,
                9,
                300_000m,
                null);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.Equal(300_000m, result.PayableAmount);
        Assert.Equal(0m, result.BalanceAmount);

        db.ChangeTracker.Clear();

        var persistedInvoice =
            await db.TuitionInvoices
                .SingleAsync(x => x.Id == result.TuitionInvoiceId);

        Assert.Equal(0m, persistedInvoice.BalanceAmount);

        var debit =
            await db.StudentCreditTransactions
                .SingleAsync(x =>
                    x.TuitionInvoiceId == persistedInvoice.Id &&
                    x.TransactionType ==
                        StudentCreditTransactionType.Debit);

        Assert.Equal(300_000m, debit.Amount);
        Assert.Equal(student.Id, debit.StudentId);

        var remainingCredit =
            await studentCreditRepository.GetBalanceAsync(
                tenantId,
                student.Id);

        Assert.Equal(200_000m, remainingCredit);
    }
    [Fact]
    public async Task SameStudentSameMonth_ShouldRejectDuplicateInvoice()
    {
        await using var db = TestDatabase.CreateContext();

        var tenantId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var studentResult = Student.Register(
            tenantId,
            personId,
            organizationId,
            null,
            $"MS-{Guid.NewGuid():N}",
            new DateOnly(2026, 1, 1),
            null,
            null,
            null);

        Assert.True(studentResult.IsSuccess);
        Assert.NotNull(studentResult.Value);

        var student = studentResult.Value!;

        db.Students.Add(student);
        await db.SaveChangesAsync();

        var attendanceRepository =
            new Mock<IAttendanceRepository>();

        attendanceRepository
            .Setup(x =>
                x.CountStudentAttendancesByMonthAsync(
                    tenantId,
                    student.Id,
                    2026,
                    10,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(6);

        var handler =
            new GenerateMonthlyTuitionInvoiceCommandHandler(
                new TuitionInvoiceRepository(db),
                new StudentCreditRepository(db),
                attendanceRepository.Object,
                new VovinamERP.Infrastructure.Repositories.StudentRepository(db),
db,
new VovinamERP.Infrastructure.Persistence.StudentCreditLockTransaction(db));

        var firstCommand =
            new GenerateMonthlyTuitionInvoiceCommand(
                tenantId,
                student.Id,
                $"HP-FIRST-{Guid.NewGuid():N}",
                2026,
                10,
                300_000m,
                null);

        var firstResult =
            await handler.Handle(
                firstCommand,
                CancellationToken.None);

        Assert.NotEqual(Guid.Empty, firstResult.TuitionInvoiceId);

        var secondCommand =
            new GenerateMonthlyTuitionInvoiceCommand(
                tenantId,
                student.Id,
                $"HP-SECOND-{Guid.NewGuid():N}",
                2026,
                10,
                300_000m,
                null);

        await Assert.ThrowsAsync<
            VovinamERP.Application.Common.Exceptions.ConflictException>(
                () => handler.Handle(
                    secondCommand,
                    CancellationToken.None));

        db.ChangeTracker.Clear();

        var invoiceCount =
            await db.TuitionInvoices.CountAsync(
                x =>
                    x.TenantId == tenantId &&
                    x.StudentId == student.Id &&
                    x.Year == 2026 &&
                    x.Month == 10);

        Assert.Equal(1, invoiceCount);
    }
}