using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.Finance;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.GetReceipts;

public sealed class GetReceiptsQueryHandler
    : IRequestHandler<
        GetReceiptsQuery,
        Result<GetReceiptsResult>>
{
    private readonly IReceiptRepository _receiptRepository;

    public GetReceiptsQueryHandler(
        IReceiptRepository receiptRepository)
    {
        _receiptRepository = receiptRepository;
    }

    public async Task<Result<GetReceiptsResult>> Handle(
        GetReceiptsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            return Result<GetReceiptsResult>.Failure(
                new Error(
                    "FIN_060",
                    "Tenant is required."));
        }

        if (request.PageNumber <= 0)
        {
            return Result<GetReceiptsResult>.Failure(
                new Error(
                    "FIN_061",
                    "Page number must be greater than zero."));
        }

        if (request.PageSize <= 0 ||
            request.PageSize > 100)
        {
            return Result<GetReceiptsResult>.Failure(
                new Error(
                    "FIN_062",
                    "Page size must be between 1 and 100."));
        }

        if (request.FromDate.HasValue &&
            request.ToDate.HasValue &&
            request.FromDate.Value > request.ToDate.Value)
        {
            return Result<GetReceiptsResult>.Failure(
                new Error(
                    "FIN_063",
                    "From date cannot be later than to date."));
        }

        ReceiptStatus? status = null;

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<ReceiptStatus>(
                    request.Status,
                    true,
                    out var parsedStatus))
            {
                return Result<GetReceiptsResult>.Failure(
                    new Error(
                        "FIN_064",
                        "Receipt status is invalid."));
            }

            status = parsedStatus;
        }

        PaymentMethod? paymentMethod = null;

        if (!string.IsNullOrWhiteSpace(
                request.PaymentMethod))
        {
            if (!Enum.TryParse<PaymentMethod>(
                    request.PaymentMethod,
                    true,
                    out var parsedPaymentMethod))
            {
                return Result<GetReceiptsResult>.Failure(
                    new Error(
                        "FIN_065",
                        "Payment method is invalid."));
            }

            paymentMethod = parsedPaymentMethod;
        }

        var (receipts, totalCount) =
            await _receiptRepository.GetPagedAsync(
                request.TenantId,
                request.FromDate,
                request.ToDate,
                status,
                paymentMethod,
                request.CollectedByUserId,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

        var items = receipts
            .Select(receipt =>
                new ReceiptListItemDto(
                    receipt.Id,
                    receipt.ReceiptNumber,
                    receipt.ReceiptDate,
                    receipt.CollectedByUserId,
                    receipt.PaymentMethod.ToString(),
                    receipt.Amount,
                    receipt.Status.ToString(),
                    receipt.Items.Count,
                    receipt.Note))
            .ToList();

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)request.PageSize);

        var result = new GetReceiptsResult(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount,
            totalPages);

        return Result<GetReceiptsResult>.Success(
            result);
    }
}