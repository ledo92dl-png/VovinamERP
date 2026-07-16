namespace VovinamERP.Application.Students.RegenerateStudentQr;

public sealed record RegenerateStudentQrResult(
    Guid StudentId,
    string MemberNumber,
    string QrToken,
    string QrContent);