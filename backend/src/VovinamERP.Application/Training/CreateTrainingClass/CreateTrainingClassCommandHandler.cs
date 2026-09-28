using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.Organizations;
using VovinamERP.Domain.Training;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Training.CreateTrainingClass;

public sealed class CreateTrainingClassCommandHandler
    : IRequestHandler<CreateTrainingClassCommand, Result<Guid>>
{
    private readonly IRepository<TrainingClass> _trainingClassRepository;
    private readonly IRepository<Organization> _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTrainingClassCommandHandler(
        IRepository<TrainingClass> trainingClassRepository,
        IRepository<Organization> organizationRepository,
        IUnitOfWork unitOfWork)
    {
        _trainingClassRepository = trainingClassRepository;
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateTrainingClassCommand request,
        CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(
            request.OrganizationId,
            cancellationToken);

        if (organization is null ||
            organization.TenantId != request.TenantId ||
            organization.IsArchived)
        {
            return Result<Guid>.Failure(
                new Error(
                    "TRAINING_021",
                    "Organization was not found for the specified tenant."));
        }

        var normalizedCode = request.Code?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            return Result<Guid>.Failure(
                new Error(
                    "TRAINING_022",
                    "Training class code is required."));
        }

        var codeExists = await _trainingClassRepository.ExistsAsync(
            x =>
                x.TenantId == request.TenantId &&
                x.Code == normalizedCode &&
                !x.IsArchived,
            cancellationToken);

        if (codeExists)
        {
            return Result<Guid>.Failure(
                new Error(
                    "TRAINING_020",
                    "Training class code already exists."));
        }

        var createResult = TrainingClass.Create(
            request.TenantId,
            request.OrganizationId,
            normalizedCode,
            request.Name,
            request.Description);

        if (createResult.IsFailure || createResult.Value is null)
        {
            return Result<Guid>.Failure(
                createResult.Error);
        }

        var trainingClass = createResult.Value;

        await _trainingClassRepository.AddAsync(
            trainingClass,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(trainingClass.Id);
    }
}