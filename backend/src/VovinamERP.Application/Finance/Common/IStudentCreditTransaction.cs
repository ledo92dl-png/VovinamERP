using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.Common;

public interface IStudentCreditTransaction
{
    Task<Result<T>> ExecuteWithStudentLockAsync<T>(
    Guid tenantId,
    Guid studentId,
    Func<CancellationToken, Task<Result<T>>> operation,
    CancellationToken cancellationToken = default);
    
    Task<Result<T>> ExecuteWithStudentAndInvoiceLockAsync<T>(
        Guid tenantId,
        Guid studentId,
        Guid tuitionInvoiceId,
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken = default);
}