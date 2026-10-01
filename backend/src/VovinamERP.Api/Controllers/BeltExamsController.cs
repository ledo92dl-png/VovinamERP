using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VovinamERP.Api.Contracts.BeltExams;
using VovinamERP.Application.BeltExams.AddBeltExamSubject;
using VovinamERP.Application.BeltExams.CreateBeltExam;
using VovinamERP.Domain.BeltExams;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/belt-exams")]
public sealed class BeltExamsController : ControllerBase
{
    private readonly VovinamDbContext _dbContext;
    private readonly ISender _sender;

    public BeltExamsController(
        VovinamDbContext dbContext,
        ISender sender)
    {
        _dbContext = dbContext;
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateBeltExamRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateBeltExamCommand(
            request.TenantId,
            request.TargetBeltRankId,
            request.ExamDate,
            request.SessionName,
            request.Location,
            request.SourceBeltName,
            request.Note,
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

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = result.Value,
                tenantId = request.TenantId
            },
            new
            {
                Id = result.Value
            });
    }

    [HttpPost("{beltExamId:guid}/subjects")]
    public async Task<IActionResult> AddSubject(
        Guid beltExamId,
        [FromBody] AddBeltExamSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddBeltExamSubjectCommand(
            request.TenantId,
            beltExamId,
            request.Name,
            request.DisplayOrder,
            request.MaximumScore,
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

        return Created(
            $"/api/belt-exams/{beltExamId}/subjects/{result.Value}",
            new
            {
                Id = result.Value
            });
    }
    [HttpGet("{beltExamId:guid}/subjects")]
    public async Task<IActionResult> GetSubjects(
        Guid beltExamId,
        [FromQuery] Guid tenantId,
        CancellationToken cancellationToken)
    {
        var beltExamExists = await _dbContext
            .Set<BeltExam>()
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == beltExamId &&
                    x.TenantId == tenantId &&
                    !x.IsArchived,
                cancellationToken);

        if (!beltExamExists)
        {
            return NotFound();
        }

        var subjects = await _dbContext
            .Set<BeltExamSubject>()
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId &&
                x.BeltExamId == beltExamId &&
                !x.IsArchived)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.DisplayOrder,
                x.MaximumScore
            })
            .ToListAsync(cancellationToken);

        return Ok(subjects);
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromQuery] Guid tenantId,
        CancellationToken cancellationToken)
    {
        var beltExam = await _dbContext
            .Set<BeltExam>()
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.TenantId == tenantId &&
                !x.IsArchived)
            .Select(x => new
            {
                x.Id,
                x.TenantId,
                x.TargetBeltRankId,
                x.ExamDate,
                x.SessionName,
                x.Location,
                x.SourceBeltName,
                x.Note
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (beltExam is null)
        {
            return NotFound();
        }

        return Ok(beltExam);
    }
}