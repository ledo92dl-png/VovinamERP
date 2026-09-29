using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.Common;

public interface IReceiptTransaction
{
    Task<Result> ExecuteWithReceiptLockAsync(
        Guid tenantId,
        Guid receiptId,
        Func<CancellationToken, Task<Result>> operation,
        CancellationToken cancellationToken = default);
}