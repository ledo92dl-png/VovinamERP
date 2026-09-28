using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.AssignStudentToReceiptItem;

public sealed record AssignStudentToReceiptItemCommand(
    Guid TenantId,
    Guid ReceiptId,
    Guid ReceiptItemId,
    Guid StudentId
) : IRequest<Result>;