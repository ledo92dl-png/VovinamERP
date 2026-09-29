using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VovinamERP.Application.Finance.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Infrastructure.Persistence;

public sealed class StudentCreditLockTransaction
    : IStudentCreditTransaction
{
    private readonly VovinamDbContext _dbContext;

    public StudentCreditLockTransaction(
        VovinamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<T>> ExecuteWithStudentLockAsync<T>(
        Guid tenantId,
        Guid studentId,
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            await LockStudentAsync(
                tenantId,
                studentId,
                transaction,
                cancellationToken);

            var result =
                await operation(cancellationToken);

            if (result.IsFailure)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return result;
            }

            await transaction.CommitAsync(
                cancellationToken);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(
                CancellationToken.None);

            throw;
        }
    }

    public async Task<Result<T>> ExecuteWithStudentAndInvoiceLockAsync<T>(
        Guid tenantId,
        Guid studentId,
        Guid tuitionInvoiceId,
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            // Always lock the student first.
            // This serializes Student Credit operations
            // for the same student.
            await LockStudentAsync(
                tenantId,
                studentId,
                transaction,
                cancellationToken);

            // Lock the existing tuition invoice second.
            // Keeping a consistent lock order helps
            // prevent deadlocks.
            await LockInvoiceAsync(
                tenantId,
                tuitionInvoiceId,
                transaction,
                cancellationToken);

            var result =
                await operation(cancellationToken);

            if (result.IsFailure)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return result;
            }

            await transaction.CommitAsync(
                cancellationToken);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(
                CancellationToken.None);

            throw;
        }
    }

    private async Task LockStudentAsync(
        Guid tenantId,
        Guid studentId,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command =
            _dbContext.Database.GetDbConnection().CreateCommand();

        command.Transaction =
            transaction.GetDbTransaction();

        command.CommandText = """
            SELECT "Id"
            FROM "students"
            WHERE "TenantId" = @tenantId
              AND "Id" = @studentId
            FOR UPDATE
            """;

        var tenantParameter =
            command.CreateParameter();
        tenantParameter.ParameterName = "tenantId";
        tenantParameter.Value = tenantId;
        command.Parameters.Add(tenantParameter);

        var studentParameter =
            command.CreateParameter();
        studentParameter.ParameterName = "studentId";
        studentParameter.Value = studentId;
        command.Parameters.Add(studentParameter);

        await command.ExecuteScalarAsync(
            cancellationToken);
    }

    private async Task LockInvoiceAsync(
        Guid tenantId,
        Guid tuitionInvoiceId,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command =
            _dbContext.Database.GetDbConnection().CreateCommand();

        command.Transaction =
            transaction.GetDbTransaction();

        command.CommandText = """
            SELECT "Id"
            FROM "TuitionInvoices"
            WHERE "TenantId" = @tenantId
              AND "Id" = @invoiceId
            FOR UPDATE
            """;

        var tenantParameter =
            command.CreateParameter();
        tenantParameter.ParameterName = "tenantId";
        tenantParameter.Value = tenantId;
        command.Parameters.Add(tenantParameter);

        var invoiceParameter =
            command.CreateParameter();
        invoiceParameter.ParameterName = "invoiceId";
        invoiceParameter.Value = tuitionInvoiceId;
        command.Parameters.Add(invoiceParameter);

        await command.ExecuteScalarAsync(
            cancellationToken);
    }
}