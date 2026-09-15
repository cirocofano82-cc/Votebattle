namespace VoteBattle.Core.Common;

/// <summary>
/// Outcome of a service operation. Carries a success flag, a human-readable message,
/// an optional list of detailed errors and an error classification for HTTP mapping.
/// </summary>
public class Result
{
    public bool Succeeded { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<string> Errors { get; init; } = new();
    public ErrorType ErrorType { get; init; } = ErrorType.None;

    public static Result Success(string message = "") =>
        new() { Succeeded = true, Message = message };

    public static Result Failure(ErrorType errorType, string message, List<string>? errors = null) =>
        new() { Succeeded = false, ErrorType = errorType, Message = message, Errors = errors ?? new() };
}

/// <summary>
/// A <see cref="Result"/> that also carries a payload on success.
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; init; }

    public static Result<T> Success(T data, string message = "") =>
        new() { Succeeded = true, Message = message, Data = data };

    public static new Result<T> Failure(ErrorType errorType, string message, List<string>? errors = null) =>
        new() { Succeeded = false, ErrorType = errorType, Message = message, Errors = errors ?? new() };
}
