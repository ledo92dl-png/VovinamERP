using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.Finance;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.CreateReceipt;

public sealed class CreateReceiptCommandHandler
    : IRequestHandler<CreateReceiptCommand, CreateReceiptResult>
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReceiptCommandHandler(
        IReceiptRepository receiptRepository,
        IUnitOfWork unitOfWork)
    {
        _receiptRepository = receiptRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateReceiptResult> Handle(
        CreateReceiptCommand request,
        CancellationToken cancellationToken)
    {
        if (await _receiptRepository.ExistsByReceiptNumberAsync(
            request.TenantId,
            request.ReceiptNumber,
            cancellationToken))
        {
            throw new InvalidOperationException(
                $"Receipt number '{request.ReceiptNumber}' already exists.");
        }

        Result<Receipt> result = Receipt.Create(
            request.TenantId,
            request.CollectedByUserId,
            request.ReceiptNumber,
            request.PaymentMethod,
            request.ReceiptDate,
            request.TransactionReference,
            request.EvidenceImageUrl,
            request.Note);

        if (result.IsFailure || result.Value is null)
        {
            throw new InvalidOperationException(result.Error.Message);
        }

        await _receiptRepository.AddAsync(
            result.Value,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateReceiptResult(
            result.Value.Id,
            result.Value.TenantId,
            result.Value.CollectedByUserId,
            result.Value.ReceiptNumber,
            result.Value.PaymentMethod,
            result.Value.ReceiptDate,
            result.Value.Amount,
            result.Value.Status,
            result.Value.TransactionReference,
            result.Value.EvidenceImageUrl,
            result.Value.Note);
    }
}