using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.Finance;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.GetStudentReceiptHistory;

public sealed class GetStudentReceiptHistoryQueryHandler
    : IRequestHandler<
        GetStudentReceiptHistoryQuery,
        Result<GetStudentReceiptHistoryResult>>
{
    private readonly IReceiptRepository _receiptRepository;

    public GetStudentReceiptHistoryQueryHandler(
        IReceiptRepository receiptRepository)
    {
        _receiptRepository = receiptRepository;
    }

    public async Task<Result<GetStudentReceiptHistoryResult>> Handle(
        GetStudentReceiptHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            return Result<GetStudentReceiptHistoryResult>.Failure(
                new Error(
                    "FIN_070",
                    "Tenant is required."));
        }

        if (request.StudentId == Guid.Empty)
        {
            return Result<GetStudentReceiptHistoryResult>.Failure(
                new Error(
                    "FIN_071",
                    "Student is required."));
        }

        if (request.PageNumber <= 0)
        {
            return Result<GetStudentReceiptHistoryResult>.Failure(
                new Error(
                    "FIN_072",
                    "Page number must be greater than zero."));
        }

        if (request.PageSize <= 0 ||
            request.PageSize > 100)
        {
            return Result<GetStudentReceiptHistoryResult>.Failure(
                new Error(
                    "FIN_073",
                    "Page size must be between 1 and 100."));
        }

        if (request.FromDate.HasValue &&
            request.ToDate.HasValue &&
            request.FromDate.Value > request.ToDate.Value)
        {
            return Result<GetStudentReceiptHistoryResult>.Failure(
                new Error(
                    "FIN_074",
                    "From date cannot be later than to date."));
        }

        ReceiptItemType? itemType = null;

        if (!string.IsNullOrWhiteSpace(request.ItemType))
        {
            if (!Enum.TryParse<ReceiptItemType>(
                    request.ItemType,
                    true,
                    out var parsedItemType))
            {
                return Result<GetStudentReceiptHistoryResult>.Failure(
                    new Error(
                        "FIN_075",
                        "Receipt item type is invalid."));
            }

            itemType = parsedItemType;
        }

        ReceiptStatus? receiptStatus = null;

        if (!string.IsNullOrWhiteSpace(
                request.ReceiptStatus))
        {
            if (!Enum.TryParse<ReceiptStatus>(
                    request.ReceiptStatus,
                    true,
                    out var parsedStatus))
            {
                return Result<GetStudentReceiptHistoryResult>.Failure(
                    new Error(
                        "FIN_076",
                        "Receipt status is invalid."));
            }

            receiptStatus = parsedStatus;
        }

        var (
            rows,
            totalCount,
            totalAmount) =
            await _receiptRepository
                .GetStudentReceiptHistoryAsync(
                    request.TenantId,
                    request.StudentId,
                    request.FromDate,
                    request.ToDate,
                    itemType,
                    receiptStatus,
                    request.PageNumber,
                    request.PageSize,
                    cancellationToken);

        var items = rows
            .Select(x =>
                new StudentReceiptHistoryItemDto(
                    x.Receipt.Id,
                    x.Item.Id,
                    x.Receipt.ReceiptNumber,
                    x.Receipt.ReceiptDate,
                    x.Item.ItemType.ToString(),
                    x.Item.Description,
                    x.Item.Quantity,
                    x.Item.UnitPrice,
                    x.Item.DiscountAmount,
                    x.Item.TotalAmount,
                    x.Receipt.PaymentMethod.ToString(),
                    x.Receipt.Status.ToString(),
                    x.Receipt.CollectedByUserId,
                    x.Item.Note))
            .ToList();

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)request.PageSize);

        var result =
            new GetStudentReceiptHistoryResult(
                request.StudentId,
                items,
                request.PageNumber,
                request.PageSize,
                totalCount,
                totalPages,
                totalAmount);

        return Result<GetStudentReceiptHistoryResult>.Success(
            result);
    }
}