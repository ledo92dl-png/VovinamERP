using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Finance.ConfirmReceipt;
using VovinamERP.Domain.Finance;
using VovinamERP.Infrastructure.Persistence.Repositories;
using VovinamERP.Infrastructure.Repositories;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Tests;

public class ConfirmReceiptConcurrencyIntegrationTests
{
    [Fact]
    public async Task ConcurrentConfirmations_ShouldNotDuplicatePaymentOrStudentCredit()
    {
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var collectorId = Guid.NewGuid();

        Guid invoiceId;
        Guid receiptId;

        // Arrange using a separate setup context.
        await using (var setupDb = TestDatabase.CreateContext())
        {
            var invoiceResult =
                TuitionInvoice.CreateMonthlyInvoice(
                    tenantId,
                    studentId,
                    $"HP-{Guid.NewGuid():N}",
                    2027,
                    4,
                    300_000m,
                    0m,
                    null);

            Assert.True(invoiceResult.IsSuccess);
            Assert.NotNull(invoiceResult.Value);

            var invoice = invoiceResult.Value!;
            invoiceId = invoice.Id;

            setupDb.TuitionInvoices.Add(invoice);

            var receiptResult =
                Receipt.Create(
                    tenantId,
                    collectorId,
                    $"PT-{Guid.NewGuid():N}",
                    PaymentMethod.Cash,
                    new DateOnly(2027, 4, 10),
                    null,
                    null,
                    "Concurrent confirmation test");

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
                    invoice.Id,
                    "Tuition payment",
                    1m,
                    500_000m,
                    0m,
                    null);

            Assert.True(itemResult.IsSuccess);
            Assert.NotNull(itemResult.Value);

            var addItemResult =
                receipt.AddItem(itemResult.Value!);

            Assert.True(addItemResult.IsSuccess);

            setupDb.Receipts.Add(receipt);

            await setupDb.SaveChangesAsync();
        }

        // Two independent DbContexts simulate two concurrent requests.
        await using var dbA = TestDatabase.CreateContext();
        await using var dbB = TestDatabase.CreateContext();

        var handlerA =
            new ConfirmReceiptCommandHandler(
                new ReceiptRepository(dbA),
                new TuitionInvoiceRepository(dbA),
                new StudentCreditRepository(dbA),
                dbA,
                 new ReceiptLockTransaction(dbA));

        var handlerB =
            new ConfirmReceiptCommandHandler(
                new ReceiptRepository(dbB),
                new TuitionInvoiceRepository(dbB),
                new StudentCreditRepository(dbB),
                        dbB,
        new ReceiptLockTransaction(dbB));

        var command =
            new ConfirmReceiptCommand(
                tenantId,
                receiptId);

        // Act.
        var taskA =
            CaptureAsync(() =>
                handlerA.Handle(
                    command,
                    CancellationToken.None));

        var taskB =
            CaptureAsync(() =>
                handlerB.Handle(
                    command,
                    CancellationToken.None));

        var outcomes =
    await Task.WhenAll(taskA, taskB);

    Assert.Equal(
    2,
    outcomes.Count(x => x.Succeeded));
        // Verify persisted state with a fresh context.
        await using var verifyDb = TestDatabase.CreateContext();

        var savedReceipt =
            await verifyDb.Receipts
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Id == receiptId);

        Assert.Equal(
            ReceiptStatus.Confirmed,
            savedReceipt.Status);

        var savedInvoice =
            await verifyDb.TuitionInvoices
                .AsNoTracking()
                .Include(x => x.Payments)
                .SingleAsync(x =>
                    x.Id == invoiceId);

        // The receipt must result in exactly one tuition payment.
        var payment =
            Assert.Single(savedInvoice.Payments);

        Assert.Equal(
            300_000m,
            payment.Amount);

        Assert.Equal(
            0m,
            savedInvoice.BalanceAmount);

        // The 200,000 excess must become exactly one credit.
        var credits =
            await verifyDb.StudentCreditTransactions
                .AsNoTracking()
                .Where(x =>
                    x.TenantId == tenantId &&
                    x.StudentId == studentId &&
                    x.TransactionType ==
                        StudentCreditTransactionType.Credit &&
                    x.ReceiptId == receiptId)
                .ToListAsync();

        var credit =
            Assert.Single(credits);

        Assert.Equal(
            200_000m,
            credit.Amount);

        var totalCredit =
            await verifyDb.StudentCreditTransactions
                .AsNoTracking()
                .Where(x =>
                    x.TenantId == tenantId &&
                    x.StudentId == studentId)
                .SumAsync(x =>
                    x.TransactionType ==
                        StudentCreditTransactionType.Credit
                        ? x.Amount
                        : -x.Amount);

        Assert.Equal(
            200_000m,
            totalCredit);
    }
        [Fact]
    public async Task ConcurrentDifferentReceipts_ShouldNotOverpaySameTuitionInvoice()
    {
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var collectorId = Guid.NewGuid();

        Guid invoiceId;
        Guid receiptAId;
        Guid receiptBId;

        // One tuition invoice has a remaining balance of 300,000.
        // Two different receipts each try to pay 300,000 concurrently.
        await using (var setupDb = TestDatabase.CreateContext())
        {
            var invoiceResult =
                TuitionInvoice.CreateMonthlyInvoice(
                    tenantId,
                    studentId,
                    $"HP-{Guid.NewGuid():N}",
                    2027,
                    5,
                    300_000m,
                    0m,
                    null);

            Assert.True(invoiceResult.IsSuccess);
            Assert.NotNull(invoiceResult.Value);

            var invoice = invoiceResult.Value!;
            invoiceId = invoice.Id;

            setupDb.TuitionInvoices.Add(invoice);

            var receiptAResult =
                Receipt.Create(
                    tenantId,
                    collectorId,
                    $"PT-A-{Guid.NewGuid():N}",
                    PaymentMethod.Cash,
                    new DateOnly(2027, 5, 10),
                    null,
                    null,
                    "Concurrent different receipt A");

            Assert.True(receiptAResult.IsSuccess);
            Assert.NotNull(receiptAResult.Value);

            var receiptA = receiptAResult.Value!;
            receiptAId = receiptA.Id;

            var itemAResult =
                ReceiptItem.Create(
                    tenantId,
                    receiptA.Id,
                    studentId,
                    ReceiptItemType.Tuition,
                    invoice.Id,
                    "Tuition payment A",
                    1m,
                    300_000m,
                    0m,
                    null);

            Assert.True(itemAResult.IsSuccess);
            Assert.NotNull(itemAResult.Value);

            var addItemAResult =
                receiptA.AddItem(itemAResult.Value!);

            Assert.True(addItemAResult.IsSuccess);

            var receiptBResult =
                Receipt.Create(
                    tenantId,
                    collectorId,
                    $"PT-B-{Guid.NewGuid():N}",
                    PaymentMethod.Cash,
                    new DateOnly(2027, 5, 10),
                    null,
                    null,
                    "Concurrent different receipt B");

            Assert.True(receiptBResult.IsSuccess);
            Assert.NotNull(receiptBResult.Value);

            var receiptB = receiptBResult.Value!;
            receiptBId = receiptB.Id;

            var itemBResult =
                ReceiptItem.Create(
                    tenantId,
                    receiptB.Id,
                    studentId,
                    ReceiptItemType.Tuition,
                    invoice.Id,
                    "Tuition payment B",
                    1m,
                    300_000m,
                    0m,
                    null);

            Assert.True(itemBResult.IsSuccess);
            Assert.NotNull(itemBResult.Value);

            var addItemBResult =
                receiptB.AddItem(itemBResult.Value!);

            Assert.True(addItemBResult.IsSuccess);

            setupDb.Receipts.Add(receiptA);
            setupDb.Receipts.Add(receiptB);

            await setupDb.SaveChangesAsync();
        }

        // Two independent DbContexts simulate two requests confirming
        // different receipts at the same time.
        await using var dbA = TestDatabase.CreateContext();
        await using var dbB = TestDatabase.CreateContext();

        var handlerA =
            new ConfirmReceiptCommandHandler(
                new ReceiptRepository(dbA),
                new TuitionInvoiceRepository(dbA),
                new StudentCreditRepository(dbA),
                dbA,
                new ReceiptLockTransaction(dbA));

        var handlerB =
            new ConfirmReceiptCommandHandler(
                new ReceiptRepository(dbB),
                new TuitionInvoiceRepository(dbB),
                new StudentCreditRepository(dbB),
                dbB,
                new ReceiptLockTransaction(dbB));

        var commandA =
            new ConfirmReceiptCommand(
                tenantId,
                receiptAId);

        var commandB =
            new ConfirmReceiptCommand(
                tenantId,
                receiptBId);

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

        // Both receipt confirmations are valid requests.
        Assert.Equal(
            2,
            outcomes.Count(x => x.Succeeded));

        // Verify the final database state.
        await using var verifyDb = TestDatabase.CreateContext();

        var savedInvoice =
            await verifyDb.TuitionInvoices
                .AsNoTracking()
                .Include(x => x.Payments)
                .SingleAsync(x =>
                    x.Id == invoiceId);

        var totalPayments =
            savedInvoice.Payments.Sum(x => x.Amount);

        var credits =
            await verifyDb.StudentCreditTransactions
                .AsNoTracking()
                .Where(x =>
                    x.TenantId == tenantId &&
                    x.StudentId == studentId &&
                    x.TransactionType ==
                        StudentCreditTransactionType.Credit &&
                    (x.ReceiptId == receiptAId ||
                     x.ReceiptId == receiptBId))
                .ToListAsync();

        var totalCredits =
            credits.Sum(x => x.Amount);

        // The invoice must never receive more than its 300,000 balance.
        Assert.Equal(
            300_000m,
            totalPayments);

        Assert.Equal(
            0m,
            savedInvoice.BalanceAmount);

        // The other 300,000 must be preserved as Student Credit.
        Assert.Equal(
            300_000m,
            totalCredits);
    }

    private static async Task<RequestOutcome> CaptureAsync(
        Func<Task<VovinamERP.SharedKernel.Results.Result>> action)
    {
        try
        {
            var result =
                await action();

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

    private sealed record RequestOutcome(
        bool Succeeded,
        Exception? Exception);
}