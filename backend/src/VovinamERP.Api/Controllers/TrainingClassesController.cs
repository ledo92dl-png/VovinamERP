using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VovinamERP.Api.Contracts.Common;
using VovinamERP.Api.Contracts.TrainingClasses;
using VovinamERP.Domain.Organizations;
using VovinamERP.Domain.Training;
using VovinamERP.Infrastructure.Persistence;
using MediatR;
using VovinamERP.Application.Training.CreateTrainingClass;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/training-classes")]
public sealed class TrainingClassesController : ControllerBase
{
    private readonly VovinamDbContext _dbContext;
private readonly ISender _sender;

public TrainingClassesController(
    VovinamDbContext dbContext,
    ISender sender)
{
    _dbContext = dbContext;
    _sender = sender;
}

    [HttpPost]
public async Task<IActionResult> Create(
    [FromBody] CreateTrainingClassRequest request,
    CancellationToken cancellationToken)
{
    var command = new CreateTrainingClassCommand(
        request.TenantId,
        request.OrganizationId,
        request.Code,
        request.Name,
        request.Description);

    var result = await _sender.Send(
        command,
        cancellationToken);

    if (result.IsFailure)
    {
        if (result.Error.Code == "TRAINING_020")
        {
            return Conflict(new
            {
                Code = result.Error.Code,
                Message = result.Error.Message
            });
        }

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
            id = result.Value
        },
        new
        {
            Id = result.Value
        });
}

    [HttpGet]
    public async Task<ActionResult<PagedResult<TrainingClassResponse>>> GetList(
        [FromQuery] TrainingClassListQuery query,
        CancellationToken cancellationToken)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);

        var trainingClasses = _dbContext.Set<TrainingClass>()
            .AsNoTracking()
            .Where(x => !x.IsArchived);

        if (query.TenantId.HasValue)
            trainingClasses = trainingClasses.Where(x => x.TenantId == query.TenantId.Value);

        if (query.OrganizationId.HasValue)
            trainingClasses = trainingClasses.Where(x => x.OrganizationId == query.OrganizationId.Value);

        if (query.Status.HasValue)
            trainingClasses = trainingClasses.Where(x => x.Status == query.Status.Value);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim().ToLower();
            trainingClasses = trainingClasses.Where(x =>
                x.Code.ToLower().Contains(keyword) ||
                x.Name.ToLower().Contains(keyword) ||
                (x.Description != null && x.Description.ToLower().Contains(keyword)));
        }

        trainingClasses = ApplySorting(trainingClasses, query.SortBy, query.Descending);

        var totalCount = await trainingClasses.CountAsync(cancellationToken);
        var items = await trainingClasses
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => TrainingClassResponse.FromDomain(x))
            .ToListAsync(cancellationToken);

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new PagedResult<TrainingClassResponse>(items, page, pageSize, totalCount, totalPages));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrainingClassResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var trainingClass = await _dbContext.Set<TrainingClass>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsArchived, cancellationToken);

        if (trainingClass is null)
            return NotFound();

        return Ok(TrainingClassResponse.FromDomain(trainingClass));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTrainingClassRequest request,
        CancellationToken cancellationToken)
    {
        var trainingClass = await _dbContext.Set<TrainingClass>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsArchived, cancellationToken);

        if (trainingClass is null)
            return NotFound();

        var result = trainingClass.Rename(request.Name, request.Description);

        if (result.IsFailure)
            return BadRequest(result.Error);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Archive(
        Guid id,
        CancellationToken cancellationToken)
    {
        var trainingClass = await _dbContext.Set<TrainingClass>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsArchived, cancellationToken);

        if (trainingClass is null)
            return NotFound();

        trainingClass.Archive(null);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static IQueryable<TrainingClass> ApplySorting(
        IQueryable<TrainingClass> query,
        string? sortBy,
        bool descending)
    {
        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();

        return normalizedSortBy switch
        {
            "code" => descending ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => descending
                ? query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Code)
                : query.OrderBy(x => x.Name).ThenBy(x => x.Code)
        };
    }
}
