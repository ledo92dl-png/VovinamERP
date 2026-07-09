using VovinamERP.Domain.Training;

namespace VovinamERP.Api.Contracts.TrainingClasses;

public sealed record TrainingClassResponse(
    Guid Id,
    Guid TenantId,
    Guid OrganizationId,
    string Code,
    string Name,
    string? Description,
    TrainingClassStatus Status)
{
    public static TrainingClassResponse FromDomain(TrainingClass trainingClass)
    {
        return new TrainingClassResponse(
            trainingClass.Id,
            trainingClass.TenantId,
            trainingClass.OrganizationId,
            trainingClass.Code,
            trainingClass.Name,
            trainingClass.Description,
            trainingClass.Status);
    }
}
