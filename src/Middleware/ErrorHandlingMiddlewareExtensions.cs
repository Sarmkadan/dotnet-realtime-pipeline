using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DotNetRealtimePipeline.Middleware;

/// <summary>
/// Provides extension methods for the <see cref="ErrorHandlingMiddleware"/> class.
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    /// <summary>
    /// Wraps an async operation with centralized error handling, using the caller name as the operation name.
    /// </summary>
    /// <typeparam name="T">The type of the operation result.</typeparam>
    /// <param name="middleware">The <see cref="ErrorHandlingMiddleware"/> instance.</param>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="operationName">The name of the operation (automatically provided via <see cref="CallerMemberNameAttribute"/>).</param>
    /// <returns>An <see cref="ErrorResponse{T}"/> containing the result or error details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> or <paramref name="operation"/> is <see langword="null"/>.</exception>
    public static async Task<ErrorResponse<T>> ExecuteWithErrorHandlingAsync<T>(
        this ErrorHandlingMiddleware middleware,
        Func<Task<T>> operation,
        [CallerMemberName] string operationName = "")
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentException.ThrowIfNullOrEmpty(operationName);

        return await middleware.ExecuteWithErrorHandlingAsync(operationName, operation);
    }

    /// <summary>
    /// Wraps a synchronous operation with centralized error handling, using the caller name as the operation name.
    /// </summary>
    /// <typeparam name="T">The type of the operation result.</typeparam>
    /// <param name="middleware">The <see cref="ErrorHandlingMiddleware"/> instance.</param>
    /// <param name="operation">The synchronous operation to execute.</param>
    /// <param name="operationName">The name of the operation (automatically provided via <see cref="CallerMemberNameAttribute"/>).</param>
    /// <returns>An <see cref="ErrorResponse{T}"/> containing the result or error details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> or <paramref name="operation"/> is <see langword="null"/>.</exception>
    public static ErrorResponse<T> ExecuteWithErrorHandling<T>(
        this ErrorHandlingMiddleware middleware,
        Func<T> operation,
        [CallerMemberName] string operationName = "")
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentException.ThrowIfNullOrEmpty(operationName);

        return middleware.ExecuteWithErrorHandling(operationName, operation);
    }
}