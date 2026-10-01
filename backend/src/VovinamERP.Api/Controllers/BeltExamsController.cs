using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VovinamERP.Api.Contracts.BeltExams;
using VovinamERP.Application.BeltExams.AddBeltExamScore;
using VovinamERP.Application.BeltExams.AddBeltExamStudentResult;
using VovinamERP.Application.BeltExams.AddBeltExamSubject;
using VovinamERP.Application.BeltExams.GetBeltExamScoreSheet;
using VovinamERP.Application.BeltExams.CreateBeltExam;
using VovinamERP.Domain.BeltExams;
using VovinamERP.Domain.Persons;
using VovinamERP.Domain.Students;
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

    [HttpPost("{beltExamId:guid}/results")]
    public async Task<IActionResult> AddStudentResult(
        Guid beltExamId,
        [FromBody] AddBeltExamStudentResultRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddBeltExamStudentResultCommand(
            request.TenantId,
            beltExamId,
            request.StudentId,
            request.UnitName,
            request.SourceResult,
            request.Result,
            request.TotalScore,
            request.Ranking,
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

        return Created(
            $"/api/belt-exams/{beltExamId}/results?tenantId={request.TenantId}",
            new
            {
                Id = result.Value
            });
    }
    [HttpPost("{beltExamId:guid}/results/{studentResultId:guid}/scores")]
    public async Task<IActionResult> AddScore(
        Guid beltExamId,
        Guid studentResultId,
        [FromBody] AddBeltExamScoreRequest request,
        CancellationToken cancellationToken)
    {
        var studentResultExists = await _dbContext
            .Set<BeltExamStudentResult>()
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == studentResultId &&
                    x.TenantId == request.TenantId &&
                    x.BeltExamId == beltExamId &&
                    !x.IsArchived,
                cancellationToken);

        if (!studentResultExists)
        {
            return NotFound();
        }

        var command = new AddBeltExamScoreCommand(
            request.TenantId,
            studentResultId,
            request.BeltExamSubjectId,
            request.Score,
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
            $"/api/belt-exams/{beltExamId}/results/{studentResultId}/scores?tenantId={request.TenantId}",
            new
            {
                Id = result.Value
            });
    }

    [HttpGet("{beltExamId:guid}/results/{studentResultId:guid}/scores")]
    public async Task<IActionResult> GetScores(
        Guid beltExamId,
        Guid studentResultId,
        [FromQuery] Guid tenantId,
        CancellationToken cancellationToken)
    {
        var query = new GetBeltExamScoreSheetQuery(
            tenantId,
            beltExamId,
            studentResultId);

        var result = await _sender.Send(
            query,
            cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new
            {
                Code = result.Error.Code,
                Message = result.Error.Message
            });
        }

        return Ok(result.Value);
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
    [HttpGet("{beltExamId:guid}/results")]
    public async Task<IActionResult> GetStudentResults(
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

        var results =
            await (
                from examResult in _dbContext
                    .Set<BeltExamStudentResult>()
                    .AsNoTracking()
                join student in _dbContext
                    .Set<Student>()
                    .AsNoTracking()
                    on examResult.StudentId equals student.Id
                join person in _dbContext
                    .Set<Person>()
                    .AsNoTracking()
                    on student.PersonId equals person.Id
                where
                    examResult.TenantId == tenantId &&
                    examResult.BeltExamId == beltExamId &&
                    !examResult.IsArchived
                orderby
                    examResult.Ranking,
                    person.FullName
                select new
                {
                    examResult.Id,
                    examResult.StudentId,
                    person.FullName,
                    examResult.UnitName,
                    examResult.SourceResult,
                    examResult.Result,
                    examResult.TotalScore,
                    examResult.Ranking,
                    examResult.Note
                })
            .ToListAsync(cancellationToken);

        return Ok(results);
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