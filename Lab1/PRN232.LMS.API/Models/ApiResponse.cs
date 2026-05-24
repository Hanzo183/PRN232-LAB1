namespace PRN232.LMS.API.Models;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyList<ApiError>? Errors { get; init; }

    public static ApiResponse<T> Ok(T data, string message = "Request processed successfully")
        => new() { Success = true, Message = message, Data = data, Errors = null };

    public static ApiResponse<T> Fail(string code, string message)
        => new() { Success = false, Message = "Request failed", Data = default, Errors = new[] { new ApiError(code, message) } };
}
