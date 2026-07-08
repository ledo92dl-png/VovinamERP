namespace VovinamERP.SharedKernel.Results;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success()
    {
        return new Result(true, Error.None);
    }

    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }
}

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base(true, Error.None)
    {
        Value = value;
    }

    private Result(Error error) : base(false, error)
    {
        Value = default;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    public static new Result<T> Failure(Error error)
    {
        return new Result<T>(error);
    }
}
