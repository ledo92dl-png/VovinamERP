namespace VovinamERP.Application.Finance.TuitionCalculation;

public sealed record TuitionCalculationResult(
    int AttendanceCount,
    decimal StandardAmount,
    decimal ChargeRate,
    decimal DiscountAmount,
    decimal PayableAmount);