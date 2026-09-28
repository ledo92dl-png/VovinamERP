
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VovinamERP.Application.Finance.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Infrastructure.Persistence;

public sealed class TuitionAdjustmentTransaction
    : ITuitionAdjustmentTransaction
{
    private readonly VovinamDbContext _dbContext;

    public TuitionAdjustmentTransaction(
        VovinamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<T>> ExecuteWithInvoiceLockAsync<T>(
        Guid tenantId,
        Guid tuitionInvoiceId,
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        await using (var command =
            _dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.Transaction = transaction.GetDbTransaction();

            command.CommandText = """
                SELECT "Id"
                FROM "TuitionInvoices"
                WHERE "TenantId" = @tenantId
                  AND "Id" = @invoiceId
                FOR UPDATE
                """;

            var tenantParameter = command.CreateParameter();
            tenantParameter.ParameterName = "tenantId";
            tenantParameter.Value = tenantId;
            command.Parameters.Add(tenantParameter);

            var invoiceParameter = command.CreateParameter();
            invoiceParameter.ParameterName = "invoiceId";
            invoiceParameter.Value = tuitionInvoiceId;
            command.Parameters.Add(invoiceParameter);

            await command.ExecuteScalarAsync(cancellationToken);
        }

        try
        {
            var result = await operation(cancellationToken);

            if (result.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return result;
            }

            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}