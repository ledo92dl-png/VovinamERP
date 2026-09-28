namespace VovinamERP.Api.Controllers.Receipts;

public sealed record AssignStudentToReceiptItemRequest(
    Guid TenantId,
    Guid StudentId);