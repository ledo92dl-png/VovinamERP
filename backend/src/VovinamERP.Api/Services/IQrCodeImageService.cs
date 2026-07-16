namespace VovinamERP.Api.Services;

public interface IQrCodeImageService
{
    byte[] GeneratePng(
        string content,
        int pixelsPerModule = 20);

    string GenerateSvg(
        string content,
        int pixelsPerModule = 20);
}