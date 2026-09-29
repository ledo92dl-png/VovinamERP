using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Finance.ConfirmReceipt;
using VovinamERP.Domain.Finance;
using VovinamERP.Infrastructure.Persistence;
using VovinamERP.Infrastructure.Persistence.Repositories;
using VovinamERP.Infrastructure.Repositories;

namespace VovinamERP.Tests;

public class ConfirmReceiptIntegrationTests
{
    [Fact]
    public async Task Overpayment_ShouldPayInvoiceAndCreateStudentCredit()
    {
        await using var db = TestDatabase.CreateContext();

        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var collectorId = Guid.NewGuid();

        // Create a tuition invoice of 300,000.
        var invoiceResult =
            TuitionInvoice.CreateMonthlyInvoice(
                tenantId,
                studentId,
                $"HP-{Guid.NewGuid():N}",
                2027,
                1,
                300_000m,
                0m,
                null);

        Assert.True(invoiceResult.IsSuccess);
        Assert.NotNull(invoiceResult.Value);

        var invoice = invoiceResult.Value!;

        db.TuitionInvoices.Add(invoice);

        // Create a receipt.
        var receiptResult =
            Receipt.Create(
                tenantId,
                collectorId,
                $"PT-{Guid.NewGuid():N}",
                PaymentMethod.Cash,
                new DateOnly(2027, 1, 10),
                null,
                null,
                "Overpayment integration test");

        Assert.True(receiptResult.IsSuccess);
        Assert.NotNull(receiptResult.Value);

        var receipt = receiptResult.Value!;

        // Receipt item is 500,000 while the invoice
        // only has a balance of 300,000.
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

        var item = itemResult.Value!;

        var addItemResult =
            receipt.AddItem(item);

        Assert.True(addItemResult.IsSuccess);
        Assert.Equal(500_000m, receipt.Amount);

        db.Receipts.Add(receipt);

        await db.SaveChangesAsync();

        var handler =
            new ConfirmReceiptCommandHandler(
                new ReceiptRepository(db),
                new TuitionInvoiceRepository(db),
                new StudentCreditRepository(db),
                db);

        // Confirm the 500,000 receipt.
        var result =
            await handler.Handle(
                new ConfirmReceiptCommand(
                    tenantId,
                    receipt.Id),
                CancellationToken.None);

        Assert.True(result.IsSuccess);

        db.ChangeTracker.Clear();

        // Reload persisted data.
        var savedReceipt =
            await db.Receipts
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Id == receipt.Id);

        Assert.Equal(
            ReceiptStatus.Confirmed,
            savedReceipt.Status);

        var savedInvoice =
            await db.TuitionInvoices
                .AsNoTracking()
                .Include(x => x.Payments)
                .SingleAsync(x =>
                    x.Id == invoice.Id);

        // Only 300,000 is applied to the invoice.
        Assert.Equal(
            0m,
            savedInvoice.BalanceAmount);

        Assert.Single(
            savedInvoice.Payments);

        Assert.Equal(
            300_000m,
            savedInvoice.Payments.Single().Amount);

        // The remaining 200,000 becomes Student Credit.
        var credit =
            await db.StudentCreditTransactions
                .AsNoTracking()
                .SingleAsync(x =>
                    x.TenantId == tenantId &&
                    x.StudentId == studentId &&
                    x.TransactionType ==
                        StudentCreditTransactionType.Credit &&
                    x.ReceiptId == receipt.Id &&
                    x.ReceiptItemId == item.Id &&
                    x.TuitionInvoiceId == invoice.Id);

        Assert.Equal(
            200_000m,
            credit.Amount);

        var totalCredit =
            await db.StudentCreditTransactions
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
}