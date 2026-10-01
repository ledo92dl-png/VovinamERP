using VovinamERP.Application.BeltRecognitions.Common;

namespace VovinamERP.Application.BeltRecognitions.GetStudentBeltRecognitions;

public sealed record GetStudentBeltRecognitionsResult(
    Guid StudentId,
    IReadOnlyList<BeltRankRecognitionListItem> Items);