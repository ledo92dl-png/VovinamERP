using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Students.QrCodes;

public static class StudentQrPayloadParser
{
    private const string SystemName = "VOVINAMERP";
    private const string EntityType = "STUDENT";

    public static Result<StudentQrPayload> Parse(string? qrContent)
    {
        if (string.IsNullOrWhiteSpace(qrContent))
        {
            return Result<StudentQrPayload>.Failure(
                new Error(
                    "STUDENT_QR_002",
                    "QR content is required."));
        }

        var parts = qrContent
            .Trim()
            .Split(
                '|',
                StringSplitOptions.TrimEntries |
                StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 4)
        {
            return Result<StudentQrPayload>.Failure(
                new Error(
                    "STUDENT_QR_003",
                    "QR content has an invalid format."));
        }

        if (!string.Equals(
                parts[0],
                SystemName,
                StringComparison.OrdinalIgnoreCase))
        {
            return Result<StudentQrPayload>.Failure(
                new Error(
                    "STUDENT_QR_004",
                    "QR code does not belong to VovinamERP."));
        }

        if (!string.Equals(
                parts[1],
                EntityType,
                StringComparison.OrdinalIgnoreCase))
        {
            return Result<StudentQrPayload>.Failure(
                new Error(
                    "STUDENT_QR_005",
                    "QR code is not a student QR code."));
        }

        if (!Guid.TryParse(parts[2], out var tenantId) ||
            tenantId == Guid.Empty)
        {
            return Result<StudentQrPayload>.Failure(
                new Error(
                    "STUDENT_QR_006",
                    "QR code contains an invalid tenant identifier."));
        }

        var qrToken = parts[3].Trim().ToLowerInvariant();

        if (qrToken.Length is < 16 or > 64)
        {
            return Result<StudentQrPayload>.Failure(
                new Error(
                    "STUDENT_QR_007",
                    "QR token has an invalid length."));
        }

        return Result<StudentQrPayload>.Success(
            new StudentQrPayload(
                tenantId,
                qrToken));
    }
}