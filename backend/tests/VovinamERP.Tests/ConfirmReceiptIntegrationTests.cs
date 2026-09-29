using Microsoft.EntityFrameworkCore;
using Moq;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Finance.ConfirmReceipt;
using VovinamERP.Application.Finance.GenerateMonthlyTuitionInvoice;
using VovinamERP.Domain.Finance;
using VovinamERP.Infrastructure.Persistence;
using VovinamERP.Infrastructure.Persistence.Repositories;
using VovinamERP.Infrastructure.Repositories;
using VovinamERP.Application.Attendance.Common;

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
                        db,
        new ReceiptLockTransaction(db));

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
[Fact]
public async Task Overpayment_ShouldAutomaticallyApplyToNextMonthTuition()
{
    await using var db = TestDatabase.CreateContext();

    var tenantId = Guid.NewGuid();
    var personId = Guid.NewGuid();
    var organizationId = Guid.NewGuid();
    var collectorId = Guid.NewGuid();

    // Create the student because monthly invoice generation
    // verifies that the student exists.
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

    db.Students.Add(student);

    // January tuition: 300,000.
    var januaryInvoiceResult =
        TuitionInvoice.CreateMonthlyInvoice(
            tenantId,
            student.Id,
            $"HP-JAN-{Guid.NewGuid():N}",
            2027,
            1,
            300_000m,
            0m,
            null);

    Assert.True(januaryInvoiceResult.IsSuccess);
    Assert.NotNull(januaryInvoiceResult.Value);

    var januaryInvoice =
        januaryInvoiceResult.Value!;

    db.TuitionInvoices.Add(januaryInvoice);

    // Parent/student pays 500,000 for January.
    var receiptResult =
        Receipt.Create(
            tenantId,
            collectorId,
            $"PT-{Guid.NewGuid():N}",
            PaymentMethod.Cash,
            new DateOnly(2027, 1, 10),
            null,
            null,
            "January tuition overpayment");

    Assert.True(receiptResult.IsSuccess);
    Assert.NotNull(receiptResult.Value);

    var receipt = receiptResult.Value!;

    var receiptItemResult =
        ReceiptItem.Create(
            tenantId,
            receipt.Id,
            student.Id,
            ReceiptItemType.Tuition,
            januaryInvoice.Id,
            "January tuition payment",
            1m,
            500_000m,
            0m,
            null);

    Assert.True(receiptItemResult.IsSuccess);
    Assert.NotNull(receiptItemResult.Value);

    var receiptItem =
        receiptItemResult.Value!;

    var addItemResult =
        receipt.AddItem(receiptItem);

    Assert.True(addItemResult.IsSuccess);

    db.Receipts.Add(receipt);

    await db.SaveChangesAsync();

    // Confirm January receipt:
    // 300,000 pays January and 200,000 becomes credit.
    var confirmHandler =
        new ConfirmReceiptCommandHandler(
            new ReceiptRepository(db),
            new TuitionInvoiceRepository(db),
            new StudentCreditRepository(db),
                    db,
        new ReceiptLockTransaction(db));

    var confirmResult =
        await confirmHandler.Handle(
            new ConfirmReceiptCommand(
                tenantId,
                receipt.Id),
            CancellationToken.None);

    Assert.True(confirmResult.IsSuccess);

    var creditRepository =
        new StudentCreditRepository(db);

    var creditAfterJanuary =
        await creditRepository.GetBalanceAsync(
            tenantId,
            student.Id,
            CancellationToken.None);

    Assert.Equal(
        200_000m,
        creditAfterJanuary);

    // February has 6 attendances, so full tuition is 300,000.
    var attendanceRepository =
        new Mock<IAttendanceRepository>();

    attendanceRepository
        .Setup(x =>
            x.CountStudentAttendancesByMonthAsync(
                tenantId,
                student.Id,
                2027,
                2,
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(6);

    var generateHandler =
        new GenerateMonthlyTuitionInvoiceCommandHandler(
            new TuitionInvoiceRepository(db),
            creditRepository,
            attendanceRepository.Object,
            new VovinamERP.Infrastructure.Repositories.StudentRepository(db),
            db,
            new StudentCreditLockTransaction(db));

    var februaryResult =
        await generateHandler.Handle(
            new GenerateMonthlyTuitionInvoiceCommand(
                tenantId,
                student.Id,
                $"HP-FEB-{Guid.NewGuid():N}",
                2027,
                2,
                300_000m,
                null),
            CancellationToken.None);

    // February invoice is 300,000.
    // Existing credit of 200,000 is automatically applied.
    Assert.Equal(
        300_000m,
        februaryResult.PayableAmount);

        Assert.Equal(
        100_000m,
        februaryResult.BalanceAmount);

        db.ChangeTracker.Clear();

    var februaryInvoice =
        await db.TuitionInvoices
            .AsNoTracking()
            .SingleAsync(x =>
                x.Id == februaryResult.TuitionInvoiceId);

    Assert.Equal(
        100_000m,
        februaryInvoice.BalanceAmount);

	Assert.Equal(
    200_000m,
    februaryResult.PaidAmount);

    // Verify the complete credit ledger:
    // +200,000 from January overpayment
    // -200,000 automatically applied to February.
    var creditTransactions =
        await db.StudentCreditTransactions
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId &&
                x.StudentId == student.Id)
            .ToListAsync();

    Assert.Equal(
        2,
        creditTransactions.Count);

    Assert.Equal(
        200_000m,
        creditTransactions
            .Where(x =>
                x.TransactionType ==
                    StudentCreditTransactionType.Credit)
            .Sum(x => x.Amount));

    Assert.Equal(
        200_000m,
        creditTransactions
            .Where(x =>
                x.TransactionType ==
                    StudentCreditTransactionType.Debit)
            .Sum(x => x.Amount));

    var februaryCreditDebit =
    Assert.Single(
        creditTransactions,
        x =>
            x.TransactionType ==
                StudentCreditTransactionType.Debit);

    Assert.Equal(
        februaryInvoice.Id,
        februaryCreditDebit.TuitionInvoiceId);
}
[Fact]
public async Task ConfirmReceiptTwice_ShouldNotDuplicatePaymentOrStudentCredit()
{
    await using var db = TestDatabase.CreateContext();

    var tenantId = Guid.NewGuid();
    var studentId = Guid.NewGuid();
    var collectorId = Guid.NewGuid();

    // Invoice balance: 300,000.
    var invoiceResult =
        TuitionInvoice.CreateMonthlyInvoice(
            tenantId,
            studentId,
            $"HP-{Guid.NewGuid():N}",
            2027,
            3,
            300_000m,
            0m,
            null);

    Assert.True(invoiceResult.IsSuccess);
    Assert.NotNull(invoiceResult.Value);

    var invoice = invoiceResult.Value!;

    db.TuitionInvoices.Add(invoice);

    // Receipt amount: 500,000.
    var receiptResult =
        Receipt.Create(
            tenantId,
            collectorId,
            $"PT-{Guid.NewGuid():N}",
            PaymentMethod.Cash,
            new DateOnly(2027, 3, 10),
            null,
            null,
            "Receipt idempotence integration test");

    Assert.True(receiptResult.IsSuccess);
    Assert.NotNull(receiptResult.Value);

    var receipt = receiptResult.Value!;

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

    db.Receipts.Add(receipt);

    await db.SaveChangesAsync();

    var handler =
        new ConfirmReceiptCommandHandler(
            new ReceiptRepository(db),
            new TuitionInvoiceRepository(db),
            new StudentCreditRepository(db),
                    db,
        new ReceiptLockTransaction(db));

    // First confirmation.
    var firstResult =
        await handler.Handle(
            new ConfirmReceiptCommand(
                tenantId,
                receipt.Id),
            CancellationToken.None);

    Assert.True(firstResult.IsSuccess);

    // Second confirmation of the same receipt.
    var secondResult =
        await handler.Handle(
            new ConfirmReceiptCommand(
                tenantId,
                receipt.Id),
            CancellationToken.None);

    Assert.True(secondResult.IsSuccess);

    db.ChangeTracker.Clear();

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

    // The invoice must contain only one payment.
    var payment =
        Assert.Single(savedInvoice.Payments);

    Assert.Equal(
        300_000m,
        payment.Amount);

    Assert.Equal(
        0m,
        savedInvoice.BalanceAmount);

    // The overpayment must create only one credit transaction.
    var credits =
        await db.StudentCreditTransactions
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId &&
                x.StudentId == studentId &&
                x.TransactionType ==
                    StudentCreditTransactionType.Credit &&
                x.ReceiptId == receipt.Id)
            .ToListAsync();

    var credit =
        Assert.Single(credits);

    Assert.Equal(
        200_000m,
        credit.Amount);

    // Final credit balance must still be exactly 200,000,
    // not 400,000.
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
    [Fact]
    public async Task ReceiptWithMultipleTuitionItems_ShouldPayMultipleInvoices()
    {
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var collectorId = Guid.NewGuid();

        await using var db =
            TestDatabase.CreateContext();

        var invoiceAResult =
            TuitionInvoice.CreateMonthlyInvoice(
                tenantId,
                studentId,
                $"HP-A-{Guid.NewGuid():N}",
                2027,
                6,
                300_000m,
                0m,
                null);

        var invoiceBResult =
            TuitionInvoice.CreateMonthlyInvoice(
                tenantId,
                studentId,
                $"HP-B-{Guid.NewGuid():N}",
                2027,
                7,
                200_000m,
                0m,
                null);

        Assert.True(invoiceAResult.IsSuccess);
        Assert.True(invoiceBResult.IsSuccess);
        Assert.NotNull(invoiceAResult.Value);
        Assert.NotNull(invoiceBResult.Value);

        var invoiceA = invoiceAResult.Value!;
        var invoiceB = invoiceBResult.Value!;

        db.TuitionInvoices.Add(invoiceA);
        db.TuitionInvoices.Add(invoiceB);

        var receiptResult =
            Receipt.Create(
                tenantId,
                collectorId,
                $"PT-{Guid.NewGuid():N}",
                PaymentMethod.Cash,
                new DateOnly(2027, 6, 10),
                null,
                null,
                "Multiple tuition invoices");

        Assert.True(receiptResult.IsSuccess);
        Assert.NotNull(receiptResult.Value);

        var receipt = receiptResult.Value!;

        var itemAResult =
            ReceiptItem.Create(
                tenantId,
                receipt.Id,
                studentId,
                ReceiptItemType.Tuition,
                invoiceA.Id,
                "June tuition",
                1m,
                300_000m,
                0m,
                null);

        var itemBResult =
            ReceiptItem.Create(
                tenantId,
                receipt.Id,
                studentId,
                ReceiptItemType.Tuition,
                invoiceB.Id,
                "July tuition",
                1m,
                200_000m,
                0m,
                null);

        Assert.True(itemAResult.IsSuccess);
        Assert.True(itemBResult.IsSuccess);
        Assert.NotNull(itemAResult.Value);
        Assert.NotNull(itemBResult.Value);

        Assert.True(
            receipt.AddItem(itemAResult.Value!).IsSuccess);

        Assert.True(
            receipt.AddItem(itemBResult.Value!).IsSuccess);

        db.Receipts.Add(receipt);

        await db.SaveChangesAsync();

        var handler =
            new ConfirmReceiptCommandHandler(
                new ReceiptRepository(db),
                new TuitionInvoiceRepository(db),
                new StudentCreditRepository(db),
                db,
                new ReceiptLockTransaction(db));

        var result =
            await handler.Handle(
                new ConfirmReceiptCommand(
                    tenantId,
                    receipt.Id),
                CancellationToken.None);

        Assert.True(result.IsSuccess);

        db.ChangeTracker.Clear();

        var savedReceipt =
            await db.Receipts
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Id == receipt.Id);

        Assert.Equal(
            ReceiptStatus.Confirmed,
            savedReceipt.Status);

        var savedInvoiceA =
            await db.TuitionInvoices
                .AsNoTracking()
                .Include(x => x.Payments)
                .SingleAsync(x =>
                    x.Id == invoiceA.Id);

        var savedInvoiceB =
            await db.TuitionInvoices
                .AsNoTracking()
                .Include(x => x.Payments)
                .SingleAsync(x =>
                    x.Id == invoiceB.Id);

        Assert.Equal(
            300_000m,
            savedInvoiceA.PaidAmount);

        Assert.Equal(
            0m,
            savedInvoiceA.BalanceAmount);

        var paymentA =
            Assert.Single(savedInvoiceA.Payments);

        Assert.Equal(
            300_000m,
            paymentA.Amount);

        Assert.Equal(
            200_000m,
            savedInvoiceB.PaidAmount);

        Assert.Equal(
            0m,
            savedInvoiceB.BalanceAmount);

        var paymentB =
            Assert.Single(savedInvoiceB.Payments);

        Assert.Equal(
            200_000m,
            paymentB.Amount);

        var credits =
            await db.StudentCreditTransactions
                .AsNoTracking()
                .Where(x =>
                    x.TenantId == tenantId &&
                    x.StudentId == studentId &&
                    x.TransactionType ==
                        StudentCreditTransactionType.Credit &&
                    x.ReceiptId == receipt.Id)
                .ToListAsync();

        Assert.Empty(credits);
    }
}