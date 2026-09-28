namespace VovinamERP.Application.Finance.TuitionCalculation;

public static class TuitionCalculator
{
    public static TuitionCalculationResult Calculate(
        int attendanceCount,
        decimal standardAmount,
        TuitionCalculationPolicy policy)
    {
        if (attendanceCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(attendanceCount),
                "Attendance count cannot be negative.");
        }

        if (standardAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(standardAmount),
                "Standard tuition amount must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(policy);

        if (policy.HalfFeeMinimumAttendance < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(policy),
                "Half-fee minimum attendance cannot be negative.");
        }

        if (policy.FullFeeMinimumAttendance <=
            policy.HalfFeeMinimumAttendance)
        {
            throw new ArgumentException(
                "Full-fee minimum attendance must be greater than half-fee minimum attendance.",
                nameof(policy));
        }

        if (policy.HalfFeeRate < 0m ||
            policy.HalfFeeRate > 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(policy),
                "Half-fee rate must be between 0 and 1.");
        }

        decimal chargeRate;

        if (attendanceCount >= policy.FullFeeMinimumAttendance)
        {
            chargeRate = 1m;
        }
        else if (attendanceCount >= policy.HalfFeeMinimumAttendance)
        {
            chargeRate = policy.HalfFeeRate;
        }
        else
        {
            chargeRate = 0m;
        }

        var payableAmount =
            decimal.Round(
                standardAmount * chargeRate,
                0,
                MidpointRounding.AwayFromZero);

        var discountAmount =
            standardAmount - payableAmount;

        return new TuitionCalculationResult(
            attendanceCount,
            standardAmount,
            chargeRate,
            discountAmount,
            payableAmount);
    }
}