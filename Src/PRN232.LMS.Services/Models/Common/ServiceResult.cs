namespace PRN232.LMS.Services.Models.Common;

public enum ServiceErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2
}

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IReadOnlyList<string> Errors { get; set; } = [];
    public ServiceErrorType ErrorType { get; set; }

    public static ServiceResult<T> Ok(T data, string message = "Request processed successfully")
        => new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ServiceResult<T> Invalid(string message, params string[] errors)
        => new()
        {
            Success = false,
            Message = message,
            Errors = errors,
            ErrorType = ServiceErrorType.Validation
        };

    public static ServiceResult<T> Invalid(string message, IEnumerable<string> errors)
        => new()
        {
            Success = false,
            Message = message,
            Errors = errors.ToArray(),
            ErrorType = ServiceErrorType.Validation
        };

    public static ServiceResult<T> NotFound(string message, params string[] errors)
        => new()
        {
            Success = false,
            Message = message,
            Errors = errors,
            ErrorType = ServiceErrorType.NotFound
        };
}
