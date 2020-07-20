// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Middleware;

using DotNetRealtimePipeline.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Middleware for centralized error handling and exception transformation.
/// Converts exceptions to application-level error responses with proper logging and recovery.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly Dictionary<Type, Func<Exception, ErrorResponse>> _errorMappers = new();

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        RegisterDefaultErrorMappers();
    }

    /// <summary>
    /// Wraps an operation with centralized error handling.
    /// </summary>
    public async Task<ErrorResponse<T>> ExecuteWithErrorHandlingAsync<T>(
        string operationName, Func<Task<T>> operation)
    {
        try
        {
            var result = await operation();
            return new ErrorResponse<T> { Success = true, Data = result };
        }
        catch (Exception ex)
        {
            return HandleException<T>(operationName, ex);
        }
    }

    /// <summary>
    /// Wraps a synchronous operation with centralized error handling.
    /// </summary>
    public ErrorResponse<T> ExecuteWithErrorHandling<T>(
        string operationName, Func<T> operation)
    {
        try
        {
            var result = operation();
            return new ErrorResponse<T> { Success = true, Data = result };
        }
        catch (Exception ex)
        {
            return HandleException<T>(operationName, ex);
        }
    }

    /// <summary>
    /// Handles an exception by mapping it to an appropriate error response.
    /// </summary>
    private ErrorResponse<T> HandleException<T>(string operationName, Exception ex)
    {
        var errorResponse = MapException(ex);

        if (errorResponse.IsRecoverable)
        {
            _logger.LogWarning(ex,
                "Recoverable error in {Operation}: {Message} (Code: {Code})",
                operationName, errorResponse.Message, errorResponse.ErrorCode);
        }
        else
        {
            _logger.LogError(ex,
                "Fatal error in {Operation}: {Message} (Code: {Code})",
                operationName, errorResponse.Message, errorResponse.ErrorCode);
        }

        return new ErrorResponse<T>
        {
            Success = false,
            ErrorCode = errorResponse.ErrorCode,
            Message = errorResponse.Message,
            IsRecoverable = errorResponse.IsRecoverable
        };
    }

    /// <summary>
    /// Maps an exception to a standardized error response using registered mappers.
    /// </summary>
    private ErrorResponse MapException(Exception ex)
    {
        var exceptionType = ex.GetType();

        if (_errorMappers.TryGetValue(exceptionType, out var mapper))
        {
            return mapper(ex) as ErrorResponse ?? CreateGenericErrorResponse(ex);
        }

        // Check base types
        foreach (var type in exceptionType.BaseType?.GetInterfaces() ?? Type.EmptyTypes)
        {
            if (_errorMappers.TryGetValue(type, out var baseMapper))
            {
                return baseMapper(ex) as ErrorResponse ?? CreateGenericErrorResponse(ex);
            }
        }

        return CreateGenericErrorResponse(ex);
    }

    /// <summary>
    /// Creates a generic error response for unmapped exceptions.
    /// </summary>
    private static ErrorResponse CreateGenericErrorResponse(Exception ex)
    {
        return new ErrorResponse
        {
            ErrorCode = "INTERNAL_ERROR",
            Message = "An internal error occurred",
            IsRecoverable = false,
            Details = ex.Message
        };
    }

    /// <summary>
    /// Registers default exception mappers.
    /// </summary>
    private void RegisterDefaultErrorMappers()
    {
        _errorMappers[typeof(PipelineException)] = ex =>
        {
            var pEx = ex as PipelineException;
            return new ErrorResponse
            {
                ErrorCode = "PIPELINE_ERROR",
                Message = ex.Message,
                IsRecoverable = true,
                Details = pEx?.InnerException?.Message
            };
        };

        _errorMappers[typeof(TimeoutException)] = ex =>
            new ErrorResponse
            {
                ErrorCode = "TIMEOUT",
                Message = "Operation timed out",
                IsRecoverable = true,
                Details = ex.Message
            };

        _errorMappers[typeof(InvalidOperationException)] = ex =>
            new ErrorResponse
            {
                ErrorCode = "INVALID_OPERATION",
                Message = "Invalid operation",
                IsRecoverable = false,
                Details = ex.Message
            };

        _errorMappers[typeof(ArgumentException)] = ex =>
            new ErrorResponse
            {
                ErrorCode = "INVALID_ARGUMENT",
                Message = "Invalid argument provided",
                IsRecoverable = false,
                Details = ex.Message
            };
    }
}

/// <summary>
/// Represents a standardized error response with recovery information.
/// </summary>
public class ErrorResponse
{
    public string ErrorCode { get; set; } = "ERROR";
    public string Message { get; set; } = string.Empty;
    public bool IsRecoverable { get; set; }
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Generic error response with data payload.
/// </summary>
public class ErrorResponse<T> : ErrorResponse
{
    public bool Success { get; set; }
    public T Data { get; set; }
}

/// <summary>
/// Middleware for automatic retry logic with exponential backoff.
/// </summary>
public class RetryMiddleware
{
    private readonly ILogger<RetryMiddleware> _logger;
    private readonly int _maxRetries;
    private readonly int _initialDelayMs;

    public RetryMiddleware(ILogger<RetryMiddleware> logger, int maxRetries = 3, int initialDelayMs = 100)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxRetries = maxRetries;
        _initialDelayMs = initialDelayMs;
    }

    /// <summary>
    /// Executes an async operation with automatic retry on failure.
    /// </summary>
    public async Task<T> ExecuteWithRetryAsync<T>(
        string operationName, Func<Task<T>> operation, Func<Exception, bool> shouldRetry)
    {
        int attempt = 0;
        int delayMs = _initialDelayMs;

        while (true)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (shouldRetry(ex) && attempt < _maxRetries)
            {
                attempt++;
                _logger.LogWarning(
                    "Operation {Operation} failed (attempt {Attempt}/{Max}), retrying in {DelayMs}ms: {Message}",
                    operationName, attempt, _maxRetries, delayMs, ex.Message);

                await Task.Delay(delayMs);
                delayMs = (int)(delayMs * 2); // Exponential backoff
            }
        }
    }
}

/// <summary>
/// Middleware for circuit breaker pattern to prevent cascade failures.
/// </summary>
public class CircuitBreakerMiddleware
{
    private readonly ILogger<CircuitBreakerMiddleware> _logger;
    private int _failureCount;
    private int _failureThreshold;
    private CircuitState _state = CircuitState.Closed;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private readonly TimeSpan _resetTimeout = TimeSpan.FromSeconds(30);

    public CircuitBreakerMiddleware(ILogger<CircuitBreakerMiddleware> logger, int failureThreshold = 5)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _failureThreshold = failureThreshold;
    }

    /// <summary>
    /// Executes an operation with circuit breaker protection.
    /// </summary>
    public async Task<T> ExecuteAsync<T>(string operationName, Func<Task<T>> operation)
    {
        if (_state == CircuitState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _resetTimeout)
            {
                _logger.LogInformation("Circuit breaker entering half-open state for {Operation}", operationName);
                _state = CircuitState.HalfOpen;
                _failureCount = 0;
            }
            else
            {
                throw new InvalidOperationException($"Circuit breaker is open for {operationName}");
            }
        }

        try
        {
            var result = await operation();

            if (_state == CircuitState.HalfOpen)
            {
                _logger.LogInformation("Circuit breaker closing for {Operation}", operationName);
                _state = CircuitState.Closed;
                _failureCount = 0;
            }

            return result;
        }
        catch (Exception ex)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;

            if (_failureCount >= _failureThreshold)
            {
                _logger.LogError("Circuit breaker opening for {Operation} after {Count} failures",
                    operationName, _failureCount);
                _state = CircuitState.Open;
            }

            throw;
        }
    }
}

public enum CircuitState
{
    Closed,
    HalfOpen,
    Open
}
