
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.Common;

public interface ITuitionAdjustmentTransaction
{
    Task<Result<T>> ExecuteWithInvoiceLockAsync<T>(
        Guid tenantId,
        Guid tuitionInvoiceId,
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken = default);
}