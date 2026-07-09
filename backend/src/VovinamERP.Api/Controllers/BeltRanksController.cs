using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VovinamERP.Api.Contracts.BeltRanks;
using VovinamERP.Domain.Belts;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/belt-ranks")]
public sealed class BeltRanksController : ControllerBase
{
    private readonly VovinamDbContext _dbContext;

    public BeltRanksController(VovinamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<BeltRankResponse>> Create(
        CreateBeltRankRequest request,
        CancellationToken cancellationToken)
    {
        var result = BeltRank.Create(
            request.BeltCode,
            request.BeltName,
            request.Level,
            request.Description);

        if (result.IsFailure || result.Value is null)
            return BadRequest(result.Error);

        _dbContext.Set<BeltRank>().Add(result.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = BeltRankResponse.FromDomain(result.Value);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<BeltRankResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var beltRanks = await _dbContext.Set<BeltRank>()
            .AsNoTracking()
            .Where(beltRank => !beltRank.IsArchived)
            .OrderBy(beltRank => beltRank.Level)
            .ThenBy(beltRank => beltRank.BeltName)
            .Select(beltRank => BeltRankResponse.FromDomain(beltRank))
            .ToListAsync(cancellationToken);

        return Ok(beltRanks);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BeltRankResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var beltRank = await _dbContext.Set<BeltRank>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsArchived, cancellationToken);

        if (beltRank is null)
            return NotFound();

        return Ok(BeltRankResponse.FromDomain(beltRank));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateBeltRankRequest request,
        CancellationToken cancellationToken)
    {
        var beltRank = await _dbContext.Set<BeltRank>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsArchived, cancellationToken);

        if (beltRank is null)
            return NotFound();

        var result = beltRank.Update(
            request.BeltName,
            request.Level,
            request.Description);

        if (result.IsFailure)
            return BadRequest(result.Error);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var beltRank = await _dbContext.Set<BeltRank>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsArchived, cancellationToken);

        if (beltRank is null)
            return NotFound();

        beltRank.Activate();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var beltRank = await _dbContext.Set<BeltRank>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsArchived, cancellationToken);

        if (beltRank is null)
            return NotFound();

        beltRank.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
