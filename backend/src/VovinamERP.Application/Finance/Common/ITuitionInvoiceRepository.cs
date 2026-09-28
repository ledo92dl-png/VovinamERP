using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.Common;

public interface ITuitionInvoiceRepository
{
    Task<TuitionInvoice?> GetByIdAsync(
        Guid tenantId,
        Guid tuitionInvoiceId,
        CancellationToken cancellationToken = default);

    Task<List<TuitionInvoice>> GetByStudentAsync(
        Guid tenantId,
        Guid studentId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForStudentMonthAsync(
        Guid tenantId,
        Guid studentId,
        int year,
        int month,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByInvoiceNumberAsync(
        Guid tenantId,
        string invoiceNumber,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TuitionInvoice tuitionInvoice,
        CancellationToken cancellationToken = default);

    void Update(TuitionInvoice tuitionInvoice);
}