using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.AssignStudentToReceiptItem;

public sealed class AssignStudentToReceiptItemCommandHandler
    : IRequestHandler<
        AssignStudentToReceiptItemCommand,
        Result>
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignStudentToReceiptItemCommandHandler(
        IReceiptRepository receiptRepository,
        IUnitOfWork unitOfWork)
    {
        _receiptRepository = receiptRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        AssignStudentToReceiptItemCommand request,
        CancellationToken cancellationToken)
    {
        var receipt = await _receiptRepository.GetByIdAsync(
            request.ReceiptId,
            cancellationToken);

        if (receipt is null)
        {
            return Result.Failure(
                new Error(
                    "FIN_030",
                    "Receipt was not found."));
        }

        if (receipt.TenantId != request.TenantId)
        {
            return Result.Failure(
                new Error(
                    "FIN_031",
                    "Receipt does not belong to the specified tenant."));
        }

        var item = receipt.Items.FirstOrDefault(
            x => x.Id == request.ReceiptItemId);

        if (item is null)
        {
            return Result.Failure(
                new Error(
                    "FIN_021",
                    "Receipt item not found."));
        }

        var assignResult = item.AssignStudent(
            request.StudentId);

        if (assignResult.IsFailure)
        {
            return assignResult;
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}