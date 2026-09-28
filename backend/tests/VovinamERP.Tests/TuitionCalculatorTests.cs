using VovinamERP.Application.Finance.TuitionCalculation;

namespace VovinamERP.Tests;

public class TuitionCalculatorTests
{
    private const decimal StandardAmount = 300_000m;

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void AttendanceBelowTwo_ShouldBeFullyExempt(
        int attendanceCount)
    {
        var result = TuitionCalculator.Calculate(
            attendanceCount,
            StandardAmount,
            TuitionCalculationPolicy.Default);

        Assert.Equal(0m, result.ChargeRate);
        Assert.Equal(300_000m, result.DiscountAmount);
        Assert.Equal(0m, result.PayableAmount);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void AttendanceFromTwoToFive_ShouldChargeHalfFee(
        int attendanceCount)
    {
        var result = TuitionCalculator.Calculate(
            attendanceCount,
            StandardAmount,
            TuitionCalculationPolicy.Default);

        Assert.Equal(0.5m, result.ChargeRate);
        Assert.Equal(150_000m, result.DiscountAmount);
        Assert.Equal(150_000m, result.PayableAmount);
    }

    [Theory]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(12)]
    [InlineData(20)]
    public void AttendanceFromSixUp_ShouldChargeFullFee(
        int attendanceCount)
    {
        var result = TuitionCalculator.Calculate(
            attendanceCount,
            StandardAmount,
            TuitionCalculationPolicy.Default);

        Assert.Equal(1m, result.ChargeRate);
        Assert.Equal(0m, result.DiscountAmount);
        Assert.Equal(300_000m, result.PayableAmount);
    }

    [Fact]
    public void CustomPolicy_ShouldBeSupported()
    {
        var policy = new TuitionCalculationPolicy(
            HalfFeeMinimumAttendance: 3,
            FullFeeMinimumAttendance: 8,
            HalfFeeRate: 0.4m);

        var result = TuitionCalculator.Calculate(
            attendanceCount: 5,
            standardAmount: 300_000m,
            policy);

        Assert.Equal(0.4m, result.ChargeRate);
        Assert.Equal(180_000m, result.DiscountAmount);
        Assert.Equal(120_000m, result.PayableAmount);
    }

    [Fact]
    public void NegativeAttendance_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TuitionCalculator.Calculate(
                -1,
                StandardAmount,
                TuitionCalculationPolicy.Default));
    }
}