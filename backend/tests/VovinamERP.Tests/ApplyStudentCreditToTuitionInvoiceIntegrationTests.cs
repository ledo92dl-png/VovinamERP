using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Finance.ApplyStudentCreditToTuitionInvoice;
using VovinamERP.Domain.Finance;
using VovinamERP.Domain.Students;
using VovinamERP.Infrastructure.Persistence;
using VovinamERP.Infrastructure.Persistence.Repositories;
using VovinamERP.Infrastructure.Repositories;

namespace VovinamERP.Tests;

public class ApplyStudentCreditToTuitionInvoiceIntegrationTests
{
    [Fact]
    public async Task ConcurrentRequests_ShouldNotDoubleSpendStudentCredit()
    {
        var tenantId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        Guid studentId;
        Guid invoiceAId;
        Guid invoiceBId;

        // Arrange using a separate setup context.
        await using (var setupDb = TestDatabase.CreateContext())
        {
            var studentResult = Student.Register(
                tenantId,
                personId,
                organizationId,
                null,
                $"MS-{Guid.NewGuid():N}",
                new DateOnly(2027, 1, 1),
                null,
                null);

            Assert.True(studentResult.IsSuccess);
            Assert.NotNull(studentResult.Value);

            var student = studentResult.Value!;
            studentId = student.Id;

            setupDb.Students.Add(student);

            var creditResult = StudentCreditTransaction.Create(
                tenantId,
                studentId,
                StudentCreditTransactionType.Credit,
                300_000m,
                new DateOnly(2027, 1, 1),
                null,
                null,
                null,
                "Concurrency test credit");

            Assert.True(creditResult.IsSuccess);
            Assert.NotNull(creditResult.Value);

            setupDb.StudentCreditTransactions.Add(
                creditResult.Value!);

            var invoiceAResult =
                TuitionInvoice.CreateMonthlyInvoice(
                    tenantId,
                    studentId,
                    $"HP-A-{Guid.NewGuid():N}",
                    2027,
                    1,
                    300_000m,
                    0m,
                    null);

            var invoiceBResult =
                TuitionInvoice.CreateMonthlyInvoice(
                    tenantId,
                    studentId,
                    $"HP-B-{Guid.NewGuid():N}",
                    2027,
                    2,
                    300_000m,
                    0m,
                    null);

            Assert.True(invoiceAResult.IsSuccess);
            Assert.NotNull(invoiceAResult.Value);
            Assert.True(invoiceBResult.IsSuccess);
            Assert.NotNull(invoiceBResult.Value);

            var invoiceA = invoiceAResult.Value!;
            var invoiceB = invoiceBResult.Value!;

            invoiceAId = invoiceA.Id;
            invoiceBId = invoiceB.Id;

            setupDb.TuitionInvoices.Add(invoiceA);
            setupDb.TuitionInvoices.Add(invoiceB);

            await setupDb.SaveChangesAsync();
        }

        // Two independent DbContexts simulate two concurrent requests.
        await using var dbA = TestDatabase.CreateContext();
        await using var dbB = TestDatabase.CreateContext();

        var handlerA =
            new ApplyStudentCreditToTuitionInvoiceCommandHandler(
                new TuitionInvoiceRepository(dbA),
                new StudentCreditRepository(dbA),
                new StudentCreditLockTransaction(dbA),
                dbA);

        var handlerB =
            new ApplyStudentCreditToTuitionInvoiceCommandHandler(
                new TuitionInvoiceRepository(dbB),
                new StudentCreditRepository(dbB),
                new StudentCreditLockTransaction(dbB),
                dbB);

        var commandA =
            new ApplyStudentCreditToTuitionInvoiceCommand(
                tenantId,
                studentId,
                invoiceAId,
                300_000m,
                new DateOnly(2027, 1, 2),
                "Concurrent request A");

        var commandB =
            new ApplyStudentCreditToTuitionInvoiceCommand(
                tenantId,
                studentId,
                invoiceBId,
                300_000m,
                new DateOnly(2027, 1, 2),
                "Concurrent request B");

        // Act.
        var taskA =
            CaptureAsync(() =>
                handlerA.Handle(
                    commandA,
                    CancellationToken.None));

        var taskB =
            CaptureAsync(() =>
                handlerB.Handle(
                    commandB,
                    CancellationToken.None));

        var outcomes =
            await Task.WhenAll(taskA, taskB);

        // Exactly one request may consume the available credit.
        Assert.Equal(
            1,
            outcomes.Count(x => x.Succeeded));

        Assert.Equal(
            1,
            outcomes.Count(x => !x.Succeeded));

        // Verify persisted state with a fresh context.
        await using var verifyDb = TestDatabase.CreateContext();

        var transactions =
            await verifyDb.StudentCreditTransactions
                .AsNoTracking()
                .Where(x =>
                    x.TenantId == tenantId &&
                    x.StudentId == studentId)
                .ToListAsync();

        var totalCredit =
            transactions
                .Where(x =>
                    x.TransactionType ==
                    StudentCreditTransactionType.Credit)
                .Sum(x => x.Amount);

        var totalDebit =
            transactions
                .Where(x =>
                    x.TransactionType ==
                    StudentCreditTransactionType.Debit)
                .Sum(x => x.Amount);

        Assert.Equal(300_000m, totalCredit);
        Assert.Equal(300_000m, totalDebit);
        Assert.True(totalDebit <= totalCredit);

        var finalBalance =
            totalCredit - totalDebit;

        Assert.Equal(0m, finalBalance);

        var invoices =
            await verifyDb.TuitionInvoices
                .AsNoTracking()
                .Where(x =>
                    x.Id == invoiceAId ||
                    x.Id == invoiceBId)
                .ToListAsync();

        Assert.Equal(2, invoices.Count);

        // Only one invoice may receive the 300,000 credit.
        Assert.Equal(
            1,
            invoices.Count(x =>
                x.BalanceAmount == 0m));

        Assert.Equal(
            1,
            invoices.Count(x =>
                x.BalanceAmount == 300_000m));
    }

    private static async Task<RequestOutcome> CaptureAsync(
        Func<Task<ApplyStudentCreditToTuitionInvoiceResult>> action)
    {
        try
        {
            await action();

            return new RequestOutcome(
                true,
                null);
        }
        catch (Exception ex)
        {
            return new RequestOutcome(
                false,
                ex);
        }
    }

    private sealed record RequestOutcome(
        bool Succeeded,
        Exception? Exception);
}
