using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Training;

public static class TrainingErrors
{
    public static readonly Error TenantRequired =
        new("TRAINING_001", "Tenant is required.");

    public static readonly Error OrganizationRequired =
        new("TRAINING_002", "Organization is required.");

    public static readonly Error CodeRequired =
        new("TRAINING_003", "Code is required.");

    public static readonly Error NameRequired =
        new("TRAINING_004", "Name is required.");

    public static readonly Error StudentRequired =
        new("TRAINING_005", "Student is required.");

    public static readonly Error ClassRequired =
        new("TRAINING_006", "Training class is required.");

    public static readonly Error SessionRequired =
        new("TRAINING_007", "Training session is required.");

    public static readonly Error InstructorRequired =
        new("TRAINING_008", "Instructor is required.");

    public static readonly Error AttendanceRecordRequired =
        new("TRAINING_009", "Attendance record is required.");

    public static readonly Error InvalidDate =
        new("TRAINING_010", "Date is invalid.");

    public static readonly Error InvalidTimeRange =
        new("TRAINING_011", "End time must be after start time.");

    public static readonly Error AlreadyArchived =
        new("TRAINING_012", "Record has already been archived.");

    public static readonly Error SessionNotOpened =
        new("TRAINING_013", "Attendance can only be created when session is opened.");

    public static readonly Error SessionAlreadyClosed =
        new("TRAINING_014", "Session has already been closed.");
}
