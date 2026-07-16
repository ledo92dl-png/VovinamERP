using QRCoder;

namespace VovinamERP.Api.Services;

public sealed class QrCodeImageService : IQrCodeImageService
{
    public byte[] GeneratePng(
        string content,
        int pixelsPerModule = 20)
    {
        ValidateInput(content, pixelsPerModule);

        using var qrCodeData = QRCodeGenerator.GenerateQrCode(
            content,
            QRCodeGenerator.ECCLevel.Q);

        using var qrCode = new PngByteQRCode(qrCodeData);

        return qrCode.GetGraphic(
            pixelsPerModule,
            drawQuietZones: true);
    }

    public string GenerateSvg(
        string content,
        int pixelsPerModule = 20)
    {
        ValidateInput(content, pixelsPerModule);

        using var qrCodeData = QRCodeGenerator.GenerateQrCode(
            content,
            QRCodeGenerator.ECCLevel.Q);

        using var qrCode = new SvgQRCode(qrCodeData);

return qrCode.GetGraphic(pixelsPerModule);
    }

    private static void ValidateInput(
        string content,
        int pixelsPerModule)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "QR content is required.",
                nameof(content));
        }

        if (pixelsPerModule is < 2 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pixelsPerModule),
                "Pixels per module must be between 2 and 100.");
        }
    }
}