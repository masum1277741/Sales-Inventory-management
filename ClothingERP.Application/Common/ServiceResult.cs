namespace ClothingERP.Application.Common;

public class ServiceResult
{
    public bool Success { get; protected set; }
    public string? Message { get; protected set; }
    public List<string> Errors { get; protected set; } = new();

    public static ServiceResult Ok(string? message = null) =>
        new() { Success = true, Message = message };

    public static ServiceResult Fail(string message) =>
        new() { Success = false, Message = message };

    public static ServiceResult Fail(List<string> errors) =>
        new() { Success = false, Errors = errors, Message = string.Join(", ", errors) };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; private set; }

    public static ServiceResult<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public new static ServiceResult<T> Fail(string message) =>
        new() { Success = false, Message = message };

    public new static ServiceResult<T> Fail(List<string> errors) =>
        new() { Success = false, Errors = errors, Message = string.Join(", ", errors) };
}