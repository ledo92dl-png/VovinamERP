using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Finance.ApplyStudentCreditToTuitionInvoice;
using VovinamERP.Application.Finance.ConfirmReceipt;
using VovinamERP.Domain.Finance;
using VovinamERP.Infrastructure.Persistence;
using VovinamERP.Infrastructure.Persistence.Repositories;
using VovinamERP.Infrastructure.Repositories;

namespace VovinamERP.Tests;

public class StudentCreditReceiptConcurrencyIntegrationTests
{
    [Fact]
    public async Task ConcurrentCreditCreationAndConsumption_ShouldPreserveStudentCreditBalance()
    {
        var tenantId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var collectorId = Guid.NewGuid();

        Guid studentId;
        Guid sourceInvoiceId;
        Guid targetInvoiceId;
        Guid receiptId;

        await using (var setupDb = TestDatabase.CreateContext())
        {
            var studentResult =
                VovinamERP.Domain.Students.Student.Register(
                    tenantId,
                    personId,
                    organizationId,
                    null,
                    $"MS-{Guid.NewGuid():N}",
                    new DateOnly(2027, 1, 1),
                    null,
                    null,
                    null);

            Assert.True(studentResult.IsSuccess);
            Assert.NotNull(studentResult.Value);

            var student = studentResult.Value!;
            studentId = student.Id;

            setupDb.Students.Add(student);

            var sourceInvoiceResult =
                TuitionInvoice.CreateMonthlyInvoice(
                    tenantId,
                    studentId,
                    $"HP-SOURCE-{Guid.NewGuid():N}",
                    2027,
                    8,
                    300_000m,
                    0m,
                    null);

            var targetInvoiceResult =
                TuitionInvoice.CreateMonthlyInvoice(
                    tenantId,
                    studentId,
                    $"HP-TARGET-{Guid.NewGuid():N}",
                    2027,
                    9,
                    300_000m,
                    0m,
                    null);

            Assert.True(sourceInvoiceResult.IsSuccess);
            Assert.True(targetInvoiceResult.IsSuccess);
            Assert.NotNull(sourceInvoiceResult.Value);
            Assert.NotNull(targetInvoiceResult.Value);

            var sourceInvoice = sourceInvoiceResult.Value!;
            var targetInvoice = targetInvoiceResult.Value!;

            sourceInvoiceId = sourceInvoice.Id;
            targetInvoiceId = targetInvoice.Id;

            setupDb.TuitionInvoices.Add(sourceInvoice);
            setupDb.TuitionInvoices.Add(targetInvoice);

            var existingCreditResult =
                StudentCreditTransaction.Create(
                    tenantId,
                    studentId,
                    StudentCreditTransactionType.Credit,
                    300_000m,
                    new DateOnly(2027, 8, 1),
                    null,
                    null,
                    null,
                    "Existing credit");

            Assert.True(existingCreditResult.IsSuccess);
            Assert.NotNull(existingCreditResult.Value);

            setupDb.StudentCreditTransactions.Add(
                existingCreditResult.Value!);

            var receiptResult =
                Receipt.Create(
                    tenantId,
                    collectorId,
                    $"PT-{Guid.NewGuid():N}",
                    PaymentMethod.Cash,
                    new DateOnly(2027, 8, 10),
                    null,
                    null,
                    "Concurrent credit creation");

            Assert.True(receiptResult.IsSuccess);
            Assert.NotNull(receiptResult.Value);

            var receipt = receiptResult.Value!;
            receiptId = receipt.Id;

            var itemResult =
                ReceiptItem.Create(
                    tenantId,
                    receipt.Id,
                    studentId,
                    ReceiptItemType.Tuition,
                    sourceInvoice.Id,
                    "Source tuition payment",
                    1m,
                    500_000m,
                    0m,
                    null);

            Assert.True(itemResult.IsSuccess);
            Assert.NotNull(itemResult.Value);
            Assert.True(
                receipt.AddItem(itemResult.Value!).IsSuccess);

            setupDb.Receipts.Add(receipt);

            await setupDb.SaveChangesAsync();
        }

        await using var receiptDb =
            TestDatabase.CreateContext();

        await using var creditDb =
            TestDatabase.CreateContext();

        var receiptHandler =
            new ConfirmReceiptCommandHandler(
                new ReceiptRepository(receiptDb),
                new TuitionInvoiceRepository(receiptDb),
                new StudentCreditRepository(receiptDb),
                receiptDb,
                new ReceiptLockTransaction(receiptDb));

        var creditHandler =
            new ApplyStudentCreditToTuitionInvoiceCommandHandler(
                new TuitionInvoiceRepository(creditDb),
                new StudentCreditRepository(creditDb),
                new StudentCreditLockTransaction(creditDb),
                creditDb);

        var confirmTask =
            CaptureAsync(() =>
                receiptHandler.Handle(
                    new ConfirmReceiptCommand(
                        tenantId,
                        receiptId),
                    CancellationToken.None));

        var consumeTask =
            CaptureAsync(() =>
                creditHandler.Handle(
                    new ApplyStudentCreditToTuitionInvoiceCommand(
                        tenantId,
                        studentId,
                        targetInvoiceId,
                        300_000m,
                        new DateOnly(2027, 8, 10),
                        "Concurrent credit consumption"),
                    CancellationToken.None));

        var outcomes =
            await Task.WhenAll(
                confirmTask,
                consumeTask);

        Assert.Equal(
            2,
            outcomes.Count(x => x.Succeeded));

        await using var verifyDb =
            TestDatabase.CreateContext();

        var savedSourceInvoice =
            await verifyDb.TuitionInvoices
                .AsNoTracking()
                .Include(x => x.Payments)
                .SingleAsync(x =>
                    x.Id == sourceInvoiceId);

        var savedTargetInvoice =
            await verifyDb.TuitionInvoices
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Id == targetInvoiceId);

        Assert.Equal(
            0m,
            savedSourceInvoice.BalanceAmount);

        Assert.Equal(
            0m,
            savedTargetInvoice.BalanceAmount);

        var credits =
            await verifyDb.StudentCreditTransactions
                .AsNoTracking()
                .Where(x =>
                    x.TenantId == tenantId &&
                    x.StudentId == studentId)
                .ToListAsync();

        var balance =
            credits.Sum(x =>
                x.TransactionType ==
                    StudentCreditTransactionType.Credit
                    ? x.Amount
                    : -x.Amount);

        // Existing credit 300,000
        // + receipt overpayment 200,000
        // - consumed credit 300,000
        // = 200,000 remaining.
        Assert.Equal(
            200_000m,
            balance);
    }

    private static async Task<RequestOutcome> CaptureAsync(
        Func<Task<VovinamERP.SharedKernel.Results.Result>> action)
    {
        try
        {
            var result = await action();

            return new RequestOutcome(
                result.IsSuccess,
                null);
        }
        catch (Exception ex)
        {
            return new RequestOutcome(
                false,
                ex);
        }
    }

    private static async Task<RequestOutcome> CaptureAsync<T>(
        Func<Task<T>> action)
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
