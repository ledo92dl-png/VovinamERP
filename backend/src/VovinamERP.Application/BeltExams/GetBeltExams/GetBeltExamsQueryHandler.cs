using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.BeltExams;
using VovinamERP.Domain.Belts;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltExams.GetBeltExams;

public sealed class GetBeltExamsQueryHandler
    : IRequestHandler<
        GetBeltExamsQuery,
        Result<IReadOnlyList<BeltExamListItem>>>
{
    private readonly IRepository<BeltExam> _examRepository;
    private readonly IRepository<BeltRank> _beltRankRepository;
    private readonly IRepository<BeltExamStudentResult> _resultRepository;

    public GetBeltExamsQueryHandler(
        IRepository<BeltExam> examRepository,
        IRepository<BeltRank> beltRankRepository,
        IRepository<BeltExamStudentResult> resultRepository)
    {
        _examRepository = examRepository;
        _beltRankRepository = beltRankRepository;
        _resultRepository = resultRepository;
    }

    public async Task<Result<IReadOnlyList<BeltExamListItem>>> Handle(
        GetBeltExamsQuery request,
        CancellationToken cancellationToken)
    {
        var exams = await _examRepository.ListAsync(
            x =>
                x.TenantId == request.TenantId &&
                !x.IsArchived,
            cancellationToken);

        var items = new List<BeltExamListItem>();

        foreach (var exam in exams)
        {
            var beltRank = await _beltRankRepository.GetByIdAsync(
                exam.TargetBeltRankId,
                cancellationToken);

            if (beltRank is null)
            {
                continue;
            }

            var studentResults = await _resultRepository.ListAsync(
                x =>
                    x.TenantId == request.TenantId &&
                    x.BeltExamId == exam.Id &&
                    !x.IsArchived,
                cancellationToken);

            items.Add(
                new BeltExamListItem(
                    exam.Id,
                    exam.ExamDate,
                    exam.SessionName,
                    exam.Location,
                    exam.TargetBeltRankId,
                    beltRank.BeltCode,
                    beltRank.BeltName,
                    exam.SourceBeltName,
                    studentResults.Count));
        }

        IReadOnlyList<BeltExamListItem> orderedItems =
            items
                .OrderByDescending(x => x.ExamDate)
                .ThenBy(x => x.SessionName)
                .ToList();

        return Result<IReadOnlyList<BeltExamListItem>>.Success(
            orderedItems);
    }
}