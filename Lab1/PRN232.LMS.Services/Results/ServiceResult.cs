namespace PRN232.LMS.Services.Results;

public sealed class ServiceResult<T>
{
    private ServiceResult(bool success, T? data, ServiceError? error)
    {
        Success = success;
        Data = data;
        Error = error;
    }

    public bool Success { get; }
    public T? Data { get; }
    public ServiceError? Error { get; }

    public static ServiceResult<T> Ok(T data) => new(success: true, data: data, error: null);

    public static ServiceResult<T> Fail(string code, string message)
        => new(success: false, data: default, error: new ServiceError(code, message));
}
