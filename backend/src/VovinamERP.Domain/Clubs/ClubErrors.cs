using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Clubs;

public static class ClubErrors
{
    public static readonly Error NameRequired =
        new("CLUB_001", "Club name is required.");

    public static readonly Error CodeRequired =
        new("CLUB_002", "Club code is required.");

    public static readonly Error AlreadyArchived =
        new("CLUB_003", "Club has already been archived.");
}