using VovinamERP.Domain.BeltRecognitions;

namespace VovinamERP.Application.BeltRecognitions.Common;

public sealed record BeltRankRecognitionListItem(
    Guid Id,
    Guid BeltRankId,
    string BeltCode,
    string BeltName,
    int Level,
    DateOnly RecognitionDate,
    BeltRankRecognitionSource Source,
    Guid? BeltExamStudentResultId,
    string? Note);