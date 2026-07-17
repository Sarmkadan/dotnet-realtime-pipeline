#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.API;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="ApiEndpointHandler"/> and its derived types.
/// Provides utility methods for common API operations and response handling.
/// </summary>
public static class ApiEndpointHandlerExtensions
{
    /// <summary>
    /// Creates a successful API response with the specified data.
    /// </summary>
    /// <typeparam name="T">Type of the data being returned.</typeparam>
    /// <param name="handler">The API endpoint handler instance.</param>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="message">Optional message to include in the response.</param>
    /// <param name="statusCode">HTTP status code to return.</param>
    /// <returns>A successful <see cref="ApiEndpointHandler.ApiResponse{T}"/> with the provided data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if handler is null.</exception>
    public static ApiEndpointHandler.ApiResponse<T> Ok<T>(this ApiEndpointHandler handler, T data, string? message = null, int statusCode = 200)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return new ApiEndpointHandler.ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Request completed successfully",
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an error API response with the specified message and status code.
    /// </summary>
    /// <typeparam name="T">Type of the data being returned.</typeparam>
    /// <param name="handler">The API endpoint handler instance.</param>
    /// <param name="message">Error message to include in the response.</param>
    /// <param name="statusCode">HTTP status code to return.</param>
    /// <param name="errorCode">Optional error code identifier.</param>
    /// <returns>An error <see cref="ApiEndpointHandler.ApiResponse{T}"/> with the provided error details.</returns>
    /// <exception cref="ArgumentNullException">Thrown if handler is null.</exception>
    public static ApiEndpointHandler.ApiResponse<T> Error<T>(this ApiEndpointHandler handler, string message, int statusCode = 500, string? errorCode = null)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return new ApiEndpointHandler.ApiResponse<T>
        {
            Success = false,
            Data = default,
            Message = message ?? "An error occurred while processing the request",
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a paginated response from a collection of data.
    /// </summary>
    /// <typeparam name="T">Type of the data items in the collection.</typeparam>
    /// <param name="handler">The API endpoint handler instance.</param>
    /// <param name="data">The collection of data to paginate.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="totalCount">Total number of items available.</param>
    /// <param name="message">Optional message to include in the response.</param>
    /// <returns>A paginated <see cref="ApiEndpointHandler.ApiResponse{PaginatedResponse{T}}"/> with the paginated data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if handler or data is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if page, pageSize, or totalCount are invalid.</exception>
    public static ApiEndpointHandler.ApiResponse<PaginatedResponse<T>> ToPaginatedResponse<T>(
        this ApiEndpointHandler handler,
        IEnumerable<T> data,
        int page,
        int pageSize,
        int totalCount,
        string? message = null)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(data);

        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be 1 or greater.");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be 1 or greater.");
        }

    if (totalCount < 0)
    {
        throw new ArgumentOutOfRangeException(nameof(totalCount), "Total count cannot be negative.");
    }

        var items = data.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var responseData = new PaginatedResponse<T>
        {
            Items = items.AsReadOnly(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        return handler.Ok(responseData, message ?? $"Page {page} of {totalPages} retrieved");
    }

    /// <summary>
    /// Creates a response with batch statistics from ingestion results.
    /// </summary>
    /// <param name="handler">The API endpoint handler instance.</param>
    /// <param name="successfulCount">Number of successfully processed items.</param>
    /// <param name="failedCount">Number of failed items.</param>
    /// <param name="totalCount">Total number of items processed.</param>
    /// <param name="message">Optional message to include in the response.</param>
    /// <param name="statusCode">HTTP status code to return.</param>
    /// <returns>A <see cref="ApiEndpointHandler.ApiResponse{BatchIngestResult}"/> with batch statistics.</returns>
    /// <exception cref="ArgumentNullException">Thrown if handler is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if counts are negative.</exception>
    public static ApiEndpointHandler.ApiResponse<BatchIngestResult> WithBatchStats(
        this ApiEndpointHandler handler,
        int successfulCount,
        int failedCount,
        int totalCount,
        string? message = null,
        int statusCode = 200)
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (successfulCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(successfulCount), "Successful count cannot be negative.");
        }

        if (failedCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(failedCount), "Failed count cannot be negative.");
        }

        if (totalCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalCount), "Total count cannot be negative.");
        }

        if (totalCount != successfulCount + failedCount)
        {
            throw new ArgumentException("Total count must equal successful count plus failed count.", nameof(totalCount));
        }

        var result = new BatchIngestResult
        {
            SuccessfulCount = successfulCount,
            FailedCount = failedCount,
            TotalCount = totalCount
        };

        return handler.Ok(result, message ?? $"Batch processed: {successfulCount} successful, {failedCount} failed", statusCode);
    }
}

/// <summary>
/// Represents a paginated API response.
/// </summary>
/// <typeparam name="T">Type of items in the collection.</typeparam>
public sealed class PaginatedResponse<T>
{
    /// <summary>
    /// The items on the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// The total number of items available.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// The total number of pages available.
    /// </summary>
    public int TotalPages { get; init; }
}