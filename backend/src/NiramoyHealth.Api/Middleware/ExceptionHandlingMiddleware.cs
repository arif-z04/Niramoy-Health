using System.Net;
using System.Text.Json;
using NiramoyHealth.Application.Common;
using NiramoyHealth.Domain.Exceptions;

namespace NiramoyHealth.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ApiErrorResponse
        {
            Success = false,
            Timestamp = DateTimeOffset.UtcNow
        };

        switch (exception)
        {
            case ValidationException valEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Validation failed.";
                errorResponse.ErrorCode = "VALIDATION_ERROR";
                errorResponse.Errors = valEx.Errors;
                _logger.LogWarning("Validation error: {Message}", valEx.Message);
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = notFoundEx.Message;
                errorResponse.ErrorCode = "NOT_FOUND";
                _logger.LogWarning("Resource not found: {Message}", notFoundEx.Message);
                break;

            case ConflictException conflictEx:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse.Message = conflictEx.Message;
                errorResponse.ErrorCode = "CONFLICT";
                _logger.LogWarning("Conflict: {Message}", conflictEx.Message);
                break;

            case BusinessRuleViolationException brEx:
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse.Message = brEx.Message;
                errorResponse.ErrorCode = "BUSINESS_RULE_VIOLATION";
                _logger.LogWarning("Business rule violation: {Message}", brEx.Message);
                break;

            case UnauthorizedException unauthEx:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = unauthEx.Message;
                errorResponse.ErrorCode = "UNAUTHORIZED";
                _logger.LogWarning("Unauthorized: {Message}", unauthEx.Message);
                break;

            case ForbiddenException forbiddenEx:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.Message = forbiddenEx.Message;
                errorResponse.ErrorCode = "FORBIDDEN";
                _logger.LogWarning("Forbidden: {Message}", forbiddenEx.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "An unexpected error occurred. Please try again later.";
                errorResponse.ErrorCode = "INTERNAL_SERVER_ERROR";
                _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
                break;
        }

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(json);
    }
}
