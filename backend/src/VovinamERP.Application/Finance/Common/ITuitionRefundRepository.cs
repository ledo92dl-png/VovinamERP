
using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.Common;

public interface ITuitionRefundRepository
{
    Task AddAsync(
        TuitionRefund refund,
        CancellationToken cancellationToken = default);

    Task<TuitionRefund?> GetByIdAsync(
        Guid tenantId,
        Guid refundId,
        CancellationToken cancellationToken = default);

    Task<TuitionRefund?> GetByAdjustmentIdAsync(
        Guid tenantId,
        Guid tuitionAdjustmentId,
        CancellationToken cancellationToken = default);

    Task<List<TuitionRefund>> GetByInvoiceIdAsync(
        Guid tenantId,
        Guid tuitionInvoiceId,
        CancellationToken cancellationToken = default);
}