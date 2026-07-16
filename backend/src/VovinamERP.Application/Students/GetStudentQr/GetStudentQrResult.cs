namespace VovinamERP.Application.Students.GetStudentQr;

public sealed record GetStudentQrResult(
    Guid StudentId,
    string MemberNumber,
    string QrToken,
    string QrContent);