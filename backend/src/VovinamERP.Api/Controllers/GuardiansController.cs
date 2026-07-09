using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VovinamERP.Api.Contracts.Common;
using VovinamERP.Api.Contracts.Guardians;
using VovinamERP.Domain.Guardians;
using VovinamERP.Domain.Persons;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/guardians")]
public sealed class GuardiansController : ControllerBase
{
    private readonly VovinamDbContext _dbContext;

    public GuardiansController(VovinamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<GuardianResponse>> Create(
        [FromBody] CreateGuardianRequest request,
        CancellationToken cancellationToken)
    {
        var personCode = $"G-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        var personResult = Person.Create(
            request.TenantId,
            personCode,
            request.FullName,
            request.Gender,
            request.DateOfBirth,
            request.PhoneNumber,
            request.Email,
            request.Address,
            request.AvatarUrl);

        if (personResult.IsFailure || personResult.Value is null)
            return BadRequest(personResult.Error);

        var guardianResult = Guardian.Create(
            request.TenantId,
            personResult.Value.Id,
            request.RelationshipNote);

        if (guardianResult.IsFailure || guardianResult.Value is null)
            return BadRequest(guardianResult.Error);

        _dbContext.Set<Person>().Add(personResult.Value);
        _dbContext.Set<Guardian>().Add(guardianResult.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = GuardianResponse.FromDomain(guardianResult.Value, personResult.Value);

        return CreatedAtAction(nameof(GetById), new { id = response.GuardianId }, response);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<GuardianResponse>>> GetAll(
        [AsParameters] GuardianListQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize switch
        {
            <= 0 => 20,
            > 100 => 100,
            _ => request.PageSize
        };

        var query =
            from guardian in _dbContext.Set<Guardian>().AsNoTracking()
            join person in _dbContext.Set<Person>().AsNoTracking()
                on guardian.PersonId equals person.Id
            where !guardian.IsArchived
            select new { guardian, person };

        if (request.TenantId.HasValue)
            query = query.Where(x => x.guardian.TenantId == request.TenantId.Value);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();

            query = query.Where(x =>
                x.person.FullName.ToLower().Contains(keyword) ||
                x.person.Code.ToLower().Contains(keyword) ||
                (x.person.PhoneNumber != null && x.person.PhoneNumber.ToLower().Contains(keyword)) ||
                (x.guardian.RelationshipNote != null && x.guardian.RelationshipNote.ToLower().Contains(keyword)));
        }

        query = (request.SortBy?.Trim().ToLower(), request.Descending) switch
        {
            ("code", true) => query.OrderByDescending(x => x.person.Code),
            ("code", false) => query.OrderBy(x => x.person.Code),
            ("fullname", true) => query.OrderByDescending(x => x.person.FullName),
            _ => query.OrderBy(x => x.person.FullName)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var guardians = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => GuardianResponse.FromDomain(x.guardian, x.person))
            .ToListAsync(cancellationToken);

        return Ok(new PagedResult<GuardianResponse>(guardians, page, pageSize, totalCount, totalPages));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GuardianResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var guardian = await _dbContext.Set<Guardian>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsArchived, cancellationToken);

        if (guardian is null)
            return NotFound();

        var person = await _dbContext.Set<Person>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == guardian.PersonId, cancellationToken);

        if (person is null)
            return NotFound();

        return Ok(GuardianResponse.FromDomain(guardian, person));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GuardianResponse>> Update(
        Guid id,
        [FromBody] UpdateGuardianRequest request,
        CancellationToken cancellationToken)
    {
        var guardian = await _dbContext.Set<Guardian>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsArchived, cancellationToken);

        if (guardian is null)
            return NotFound();

        var person = await _dbContext.Set<Person>()
            .FirstOrDefaultAsync(x => x.Id == guardian.PersonId, cancellationToken);

        if (person is null)
            return NotFound();

        var personResult = person.UpdateBasicInfo(
            request.FullName,
            request.Gender,
            request.DateOfBirth,
            request.PhoneNumber,
            request.Email,
            request.Address,
            request.AvatarUrl);

        if (personResult.IsFailure)
            return BadRequest(personResult.Error);

        var guardianResult = guardian.UpdateRelationshipNote(request.RelationshipNote, null);

        if (guardianResult.IsFailure)
            return BadRequest(guardianResult.Error);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(GuardianResponse.FromDomain(guardian, person));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var guardian = await _dbContext.Set<Guardian>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsArchived, cancellationToken);

        if (guardian is null)
            return NotFound();

        guardian.Archive(null);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
