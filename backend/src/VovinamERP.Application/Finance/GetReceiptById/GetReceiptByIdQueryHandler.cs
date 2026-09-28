using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.GetReceiptById;

public sealed class GetReceiptByIdQueryHandler
    : IRequestHandler<
        GetReceiptByIdQuery,
        Result<ReceiptDetailsDto>>
{
    private readonly IReceiptRepository _receiptRepository;

    public GetReceiptByIdQueryHandler(
        IReceiptRepository receiptRepository)
    {
        _receiptRepository = receiptRepository;
    }

    public async Task<Result<ReceiptDetailsDto>> Handle(
        GetReceiptByIdQuery request,
        CancellationToken cancellationToken)
    {
        var receipt = await _receiptRepository.GetByIdAsync(
            request.ReceiptId,
            cancellationToken);

        if (receipt is null)
        {
            return Result<ReceiptDetailsDto>.Failure(
                new Error(
                    "FIN_050",
                    "Receipt was not found."));
        }

        if (receipt.TenantId != request.TenantId)
        {
            return Result<ReceiptDetailsDto>.Failure(
                new Error(
                    "FIN_051",
                    "Receipt does not belong to the specified tenant."));
        }

        var items = receipt.Items
            .Select(item =>
                new ReceiptItemDto(
                    item.Id,
                    item.ItemType.ToString(),
                    item.ReferenceId,
                    item.Description,
                    item.Quantity,
                    item.UnitPrice,
                    item.DiscountAmount,
                    item.TotalAmount,
                    item.Note))
            .ToList();

        var dto = new ReceiptDetailsDto(
            receipt.Id,
            receipt.TenantId,
            receipt.CollectedByUserId,
            receipt.ReceiptNumber,
            receipt.ReceiptDate,
            receipt.PaymentMethod.ToString(),
            receipt.Amount,
            receipt.Status.ToString(),
            receipt.TransactionReference,
            receipt.EvidenceImageUrl,
            receipt.Note,
            items);

        return Result<ReceiptDetailsDto>.Success(dto);
    }
}