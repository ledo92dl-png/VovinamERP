namespace VovinamERP.Application.Finance.TuitionCalculation;

public sealed record TuitionCalculationPolicy(
    int HalfFeeMinimumAttendance,
    int FullFeeMinimumAttendance,
    decimal HalfFeeRate)
{
    public static TuitionCalculationPolicy Default =>
        new(
            HalfFeeMinimumAttendance: 2,
            FullFeeMinimumAttendance: 6,
            HalfFeeRate: 0.5m);
}