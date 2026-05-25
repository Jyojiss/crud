using System.Text.Json;
using UserCrudApi.Common;
using UserCrudApi.Exceptions;

namespace UserCrudApi.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IWebHostEnvironment env
    )
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            await HandleExceptionAsync(
                context,
                ex.StatusCode,
                ex.Message,
                null
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleExceptionAsync(
                context,
                StatusCodes.Status401Unauthorized,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en la aplicación");

            var message = _env.IsDevelopment()
                ? ex.Message
                : "Ocurrió un error inesperado en el servidor.";

            await HandleExceptionAsync(
                context,
                StatusCodes.Status500InternalServerError,
                message
            );
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        int statusCode,
        string message,
        object? errors = null
    )
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object>.Fail(
            statusCode,
            message,
            errors
        );

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
