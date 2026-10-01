using MediatR;
using Microsoft.AspNetCore.Mvc;
using VovinamERP.Api.Contracts.BeltRecognitions;
using VovinamERP.Application.BeltRecognitions.UploadBeltRankDocumentScan;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/belt-rank-documents")]
public sealed class BeltRankDocumentsController : ControllerBase
{
    private readonly ISender _sender;

    public BeltRankDocumentsController(
        ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{documentId:guid}/scan")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11L * 1024L * 1024L)]
    public async Task<IActionResult> UploadScan(
        Guid documentId,
        [FromForm] UploadBeltRankDocumentScanRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null)
        {
            return BadRequest(new
            {
                Code = "BELT_RANK_DOCUMENT_SCAN_API_001",
                Message = "Scan file is required."
            });
        }

        await using var stream =
            request.File.OpenReadStream();

        var command =
            new UploadBeltRankDocumentScanCommand(
                request.TenantId,
                documentId,
                stream,
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                null);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                Code = result.Error.Code,
                Message = result.Error.Message
            });
        }

        return Ok(new
        {
            ScanUrl = result.Value
        });
    }
}