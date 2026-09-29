using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VovinamERP.Application.Finance.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Infrastructure.Persistence;

public sealed class ReceiptLockTransaction
    : IReceiptTransaction
{
    private readonly VovinamDbContext _dbContext;

    public ReceiptLockTransaction(
        VovinamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

   public async Task<Result> ExecuteWithReceiptLockAsync(
        Guid tenantId,
        Guid receiptId,
       Func<CancellationToken, Task<Result>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            await LockReceiptAsync(
                tenantId,
                receiptId,
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

    private async Task LockReceiptAsync(
        Guid tenantId,
        Guid receiptId,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command =
            _dbContext.Database
                .GetDbConnection()
                .CreateCommand();

        command.Transaction =
            transaction.GetDbTransaction();

        command.CommandText = """
            SELECT "Id"
            FROM "receipts"
            WHERE "TenantId" = @tenantId
              AND "Id" = @receiptId
            FOR UPDATE
            """;

        var tenantParameter =
            command.CreateParameter();

        tenantParameter.ParameterName = "tenantId";
        tenantParameter.Value = tenantId;

        command.Parameters.Add(
            tenantParameter);

        var receiptParameter =
            command.CreateParameter();

        receiptParameter.ParameterName = "receiptId";
        receiptParameter.Value = receiptId;

        command.Parameters.Add(
            receiptParameter);

        await command.ExecuteScalarAsync(
            cancellationToken);
    }
}