using System.Net;
using System.Text.Json;
using Medicine.DTOs;

namespace Medicine.Middleware;

/// <summary>
/// Global exception handling middleware to catch unhandled errors and return consistent error responses
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponseDto();

        // Determine status code and message based on exception type
        switch (exception)
        {
            case ArgumentNullException argNullEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Success = false;
                response.Message = $"Invalid argument: {argNullEx.ParamName}";
                break;

            case ArgumentException argEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Success = false;
                response.Message = $"Invalid input: {argEx.Message}";
                break;

            case InvalidOperationException invalidOpEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Success = false;
                response.Message = invalidOpEx.Message;
                break;

            case UnauthorizedAccessException unauthorizedEx:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                response.Success = false;
                response.Message = "Unauthorized access";
                break;

            case KeyNotFoundException notFoundEx:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.Success = false;
                response.Message = notFoundEx.Message ?? "Resource not found";
                break;

            default:
                // Generic server error
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Success = false;
                response.Message = "An unexpected error occurred. Please try again later.";
#if DEBUG
                response.Message += $" Error: {exception.Message}";
#endif
                break;
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var jsonResponse = JsonSerializer.Serialize(response, options);
        return context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Extension method to add global exception handling middleware
/// </summary>
public static class ExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
