using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.Finance;
using VovinamERP.SharedKernel.Results;
using VovinamERP.Application.Finance.Common;

namespace VovinamERP.Application.Finance.AddReceiptItem;

public sealed class AddReceiptItemCommandHandler
    : IRequestHandler<
        AddReceiptItemCommand,
        Result<AddReceiptItemResult>>
{
    private readonly IReceiptRepository _receiptRepository;
private readonly ITuitionInvoiceRepository _tuitionInvoiceRepository;
private readonly IUnitOfWork _unitOfWork;

    public AddReceiptItemCommandHandler(
    IReceiptRepository receiptRepository,
    ITuitionInvoiceRepository tuitionInvoiceRepository,
    IUnitOfWork unitOfWork)
{
    _receiptRepository = receiptRepository;
    _tuitionInvoiceRepository = tuitionInvoiceRepository;
    _unitOfWork = unitOfWork;
}

    public async Task<Result<AddReceiptItemResult>> Handle(
        AddReceiptItemCommand request,
        CancellationToken cancellationToken)
    {
        var receipt = await _receiptRepository.GetByIdAsync(
            request.ReceiptId,
            cancellationToken);

        if (receipt is null)
        {
            return Result<AddReceiptItemResult>.Failure(
                new Error(
                    "FIN_030",
                    "Receipt was not found."));
        }

        if (receipt.TenantId != request.TenantId)
        {
            return Result<AddReceiptItemResult>.Failure(
                new Error(
                    "FIN_031",
                    "Receipt does not belong to the specified tenant."));
        }

        if (request.ItemType == ReceiptItemType.Tuition)
{
    if (!request.ReferenceId.HasValue ||
        request.ReferenceId.Value == Guid.Empty)
    {
        return Result<AddReceiptItemResult>.Failure(
            new Error(
                "FIN_043",
                "Tuition receipt item must reference a tuition invoice."));
    }

    var tuitionInvoice =
        await _tuitionInvoiceRepository.GetByIdAsync(
            request.TenantId,
            request.ReferenceId.Value,
            cancellationToken);

    if (tuitionInvoice is null)
    {
        return Result<AddReceiptItemResult>.Failure(
            new Error(
                "FIN_044",
                "Referenced tuition invoice was not found."));
    }

    if (!request.StudentId.HasValue ||
        request.StudentId.Value != tuitionInvoice.StudentId)
    {
        return Result<AddReceiptItemResult>.Failure(
            new Error(
                "FIN_045",
                "Receipt item student does not match the tuition invoice student."));
    }

    if (tuitionInvoice.Status == TuitionInvoiceStatus.Paid ||
        tuitionInvoice.BalanceAmount <= 0)
    {
        return Result<AddReceiptItemResult>.Failure(
            new Error(
                "FIN_047",
                "Tuition invoice has already been fully paid."));
    }

   
}

        var itemResult = ReceiptItem.Create(
            request.TenantId,
            receipt.Id,
            request.StudentId,
            request.ItemType,
            request.ReferenceId,
            request.Description,
            request.Quantity,
            request.UnitPrice,
            request.DiscountAmount,
            request.Note);

        if (itemResult.IsFailure ||
            itemResult.Value is null)
        {
            return Result<AddReceiptItemResult>.Failure(
                itemResult.Error);
        }

        var addResult = receipt.AddItem(
            itemResult.Value);

        if (addResult.IsFailure)
        {
            return Result<AddReceiptItemResult>.Failure(
                addResult.Error);
        }

        await _receiptRepository.AddItemAsync(
    itemResult.Value,
    cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<AddReceiptItemResult>.Success(
            new AddReceiptItemResult(
                receipt.Id,
                itemResult.Value.Id,
                itemResult.Value.TotalAmount,
                receipt.Amount));
    }
}