namespace UserCrudApi.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public object? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Operación exitosa.")
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Message = message,
            Data = data,
            Errors = null
        };
    }

    public static ApiResponse<T> Created(T data, string message = "Recurso creado correctamente.")
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = StatusCodes.Status201Created,
            Message = message,
            Data = data,
            Errors = null
        };
    }

    public static ApiResponse<T> Fail(
        int statusCode,
        string message,
        object? errors = null
    )
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Data = default,
            Errors = errors
        };
    }
}
