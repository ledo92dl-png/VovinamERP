
using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.Common;

public interface ITuitionAdjustmentRepository
{
    Task AddAsync(
        TuitionAdjustment adjustment,
        CancellationToken cancellationToken = default);

    Task<List<TuitionAdjustment>> GetByInvoiceIdAsync(
        Guid tenantId,
        Guid tuitionInvoiceId,
        CancellationToken cancellationToken = default);
}