using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Belts;

public static class BeltRankErrors
{
    public static readonly Error CodeRequired =
        new("BELT_001", "Belt code is required.");

    public static readonly Error NameRequired =
        new("BELT_002", "Belt name is required.");

    public static readonly Error InvalidLevel =
        new("BELT_003", "Belt level must be greater than zero.");

    public static readonly Error AlreadyArchived =
        new("BELT_004", "Belt rank has already been archived.");
}