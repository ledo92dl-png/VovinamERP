using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.Common;

public interface IStudentCreditRepository
{
    Task<decimal> GetBalanceAsync(
        Guid tenantId,
        Guid studentId,
        CancellationToken cancellationToken = default);

    Task<List<StudentCreditTransaction>> GetTransactionsAsync(
        Guid tenantId,
        Guid studentId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        StudentCreditTransaction transaction,
        CancellationToken cancellationToken = default);

        
Task<List<StudentCreditTransaction>> GetByInvoiceIdAsync(
    Guid tenantId,
    Guid studentId,
    Guid tuitionInvoiceId,
    CancellationToken cancellationToken = default);
}