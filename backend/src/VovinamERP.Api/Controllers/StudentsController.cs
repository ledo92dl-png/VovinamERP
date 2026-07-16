using VovinamERP.Application.Students.RegenerateStudentQr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using VovinamERP.Application.Students.GetStudentQr;
using Microsoft.EntityFrameworkCore;
using VovinamERP.Api.Contracts.Common;
using VovinamERP.Api.Contracts.Students;
using VovinamERP.Domain.Persons;
using VovinamERP.Domain.Students;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/students")]
public sealed class StudentsController : ControllerBase
{
    private readonly VovinamDbContext _dbContext;
    private readonly ISender _sender;

   public StudentsController(
    VovinamDbContext dbContext,
    ISender sender)
{
    _dbContext = dbContext;
    _sender = sender;
}
    [HttpGet("{studentId:guid}/qr")]
    public async Task<IActionResult> GetStudentQr(
    Guid studentId,
    [FromQuery] Guid tenantId,
    CancellationToken cancellationToken)
{
    var query = new GetStudentQrQuery(
        tenantId,
        studentId);

    var result = await _sender.Send(
        query,
        cancellationToken);

    return Ok(result);
}
    [HttpPost]
    public async Task<ActionResult<StudentResponse>> Create(
        [FromBody] CreateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var personCode = $"P-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        var memberNumber = $"MS-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

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

        var studentResult = Student.Register(
            request.TenantId,
            personResult.Value.Id,
            request.OrganizationId,
            request.CurrentBeltRankId,
            memberNumber,
            request.EnrollmentDate,
            request.MartialName,
            request.IntroducedBy,
            request.MartialProfileNote);

        if (studentResult.IsFailure || studentResult.Value is null)
            return BadRequest(studentResult.Error);

        _dbContext.Set<Person>().Add(personResult.Value);
        _dbContext.Set<Student>().Add(studentResult.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = ToResponse(studentResult.Value, personResult.Value);

        return CreatedAtAction(nameof(GetById), new { id = response.StudentId }, response);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<StudentResponse>>> GetAll(
        [AsParameters] StudentListQuery request,
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
            from student in _dbContext.Set<Student>().AsNoTracking()
            join person in _dbContext.Set<Person>().AsNoTracking()
                on student.PersonId equals person.Id
            select new { student, person };

        if (request.TenantId.HasValue)
            query = query.Where(x => x.student.TenantId == request.TenantId.Value);

        if (request.OrganizationId.HasValue)
            query = query.Where(x => x.student.OrganizationId == request.OrganizationId.Value);

        if (request.CurrentBeltRankId.HasValue)
            query = query.Where(x => x.student.CurrentBeltRankId == request.CurrentBeltRankId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.student.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();

            query = query.Where(x =>
                x.person.FullName.ToLower().Contains(keyword) ||
                x.student.MemberNumber.ToLower().Contains(keyword) ||
                (x.student.MartialName != null && x.student.MartialName.ToLower().Contains(keyword)));
        }

        query = (request.SortBy?.Trim().ToLower(), request.Descending) switch
        {
            ("membernumber", true) => query.OrderByDescending(x => x.student.MemberNumber),
            ("membernumber", false) => query.OrderBy(x => x.student.MemberNumber),
            ("enrollmentdate", true) => query.OrderByDescending(x => x.student.EnrollmentDate),
            ("enrollmentdate", false) => query.OrderBy(x => x.student.EnrollmentDate),
            ("status", true) => query.OrderByDescending(x => x.student.Status),
            ("status", false) => query.OrderBy(x => x.student.Status),
            ("fullname", true) => query.OrderByDescending(x => x.person.FullName),
            _ => query.OrderBy(x => x.person.FullName)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var students = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToResponse(x.student, x.person))
            .ToListAsync(cancellationToken);

        return Ok(new PagedResult<StudentResponse>(students, page, pageSize, totalCount, totalPages));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var student = await _dbContext.Set<Student>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (student is null)
            return NotFound();

        var person = await _dbContext.Set<Person>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == student.PersonId, cancellationToken);

        if (person is null)
            return NotFound();

        return Ok(ToResponse(student, person));
    }

    [HttpGet("by-member-number/{memberNumber}")]
    public async Task<ActionResult<StudentResponse>> GetByMemberNumber(
        string memberNumber,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(memberNumber))
            return BadRequest("Member number is required.");

        var student = await _dbContext.Set<Student>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MemberNumber == memberNumber, cancellationToken);

        if (student is null)
            return NotFound();

        var person = await _dbContext.Set<Person>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == student.PersonId, cancellationToken);

        if (person is null)
            return NotFound();

        return Ok(ToResponse(student, person));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StudentResponse>> Update(
        Guid id,
        [FromBody] UpdateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var student = await _dbContext.Set<Student>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (student is null)
            return NotFound();

        if (student.TenantId != request.TenantId)
            return BadRequest("Student does not belong to the specified tenant.");

        var person = await _dbContext.Set<Person>()
            .FirstOrDefaultAsync(x => x.Id == student.PersonId, cancellationToken);

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

        var martialProfileResult = student.UpdateMartialProfile(
            request.MartialName,
            request.IntroducedBy,
            request.MartialProfileNote,
            null);

        if (martialProfileResult.IsFailure)
            return BadRequest(martialProfileResult.Error);

        if (request.CurrentBeltRankId.HasValue && request.CurrentBeltRankId.Value != student.CurrentBeltRankId)
        {
            var beltResult = student.ChangeCurrentBelt(
                request.CurrentBeltRankId.Value,
                DateOnly.FromDateTime(DateTime.UtcNow),
                "Updated from student profile.",
                null);

            if (beltResult.IsFailure)
                return BadRequest(beltResult.Error);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(student, person));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var student = await _dbContext.Set<Student>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (student is null)
            return NotFound();

        student.Archive(null);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static StudentResponse ToResponse(Student student, Person person)
    {
        return new StudentResponse(
            student.Id,
            student.PersonId,
            student.TenantId,
            student.OrganizationId,
            student.MemberNumber,
            person.FullName,
            person.Gender,
            person.DateOfBirth,
            person.PhoneNumber,
            person.Email,
            person.Address,
            student.CurrentBeltRankId,
            student.EnrollmentDate,
            student.Status,
            student.MartialName,
            student.IntroducedBy,
            student.MartialProfileNote);
    }
    [HttpPost("{studentId:guid}/qr/regenerate")]
public async Task<IActionResult> RegenerateStudentQr(
    Guid studentId,
    [FromBody] RegenerateStudentQrRequest request,
    CancellationToken cancellationToken)
{
    var command = new RegenerateStudentQrCommand(
        request.TenantId,
        studentId,
        request.RegeneratedByUserId);

    var result = await _sender.Send(
        command,
        cancellationToken);

    if (result.IsFailure || result.Value is null)
    {
        return BadRequest(new
        {
            Code = result.Error.Code,
            Message = result.Error.Message
        });
    }

    return Ok(result.Value);
}
}
