using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Clubs;

public sealed class Club : AggregateRoot
{
    public string ClubCode { get; private set; } = default!;
    public string ClubName { get; private set; } = default!;
    public string? ShortName { get; private set; }
    public string? Address { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string? ManagerName { get; private set; }
    public DateOnly? EstablishedDate { get; private set; }
    public ClubStatus Status { get; private set; }

    private Club()
    {
    }

    private Club(
        string clubCode,
        string clubName,
        string? shortName,
        string? address,
        string? phoneNumber,
        string? email,
        string? managerName,
        DateOnly? establishedDate)
    {
        ClubCode = clubCode.Trim();
        ClubName = clubName.Trim();
        ShortName = shortName?.Trim();
        Address = address?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        Email = email?.Trim();
        ManagerName = managerName?.Trim();
        EstablishedDate = establishedDate;
        Status = ClubStatus.Active;

        RaiseDomainEvent(new ClubCreatedEvent(Id, ClubCode, ClubName));
    }

    public static Result<Club> Create(
        string clubCode,
        string clubName,
        string? shortName,
        string? address,
        string? phoneNumber,
        string? email,
        string? managerName,
        DateOnly? establishedDate)
    {
        if (string.IsNullOrWhiteSpace(clubCode))
            return Result<Club>.Failure(ClubErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(clubName))
            return Result<Club>.Failure(ClubErrors.NameRequired);

        var club = new Club(
            clubCode,
            clubName,
            shortName,
            address,
            phoneNumber,
            email,
            managerName,
            establishedDate);

        return Result<Club>.Success(club);
    }

    public Result UpdateBasicInfo(
        string clubName,
        string? shortName,
        string? address,
        string? phoneNumber,
        string? email,
        string? managerName,
        DateOnly? establishedDate)
    {
        if (IsArchived)
            return Result.Failure(ClubErrors.AlreadyArchived);

        if (string.IsNullOrWhiteSpace(clubName))
            return Result.Failure(ClubErrors.NameRequired);

        ClubName = clubName.Trim();
        ShortName = shortName?.Trim();
        Address = address?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        Email = email?.Trim();
        ManagerName = managerName?.Trim();
        EstablishedDate = establishedDate;

        RaiseDomainEvent(new ClubUpdatedEvent(Id));

        return Result.Success();
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        Status = ClubStatus.Archived;
        base.Archive(userId);

        RaiseDomainEvent(new ClubArchivedEvent(Id));
    }
}
