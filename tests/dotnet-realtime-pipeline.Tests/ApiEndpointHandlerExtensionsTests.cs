using System;
using System.Collections.Generic;
using System.Linq;
using DotNetRealtimePipeline.API;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

/// <summary>
/// Unit tests for <see cref="ApiEndpointHandlerExtensions"/>.
/// </summary>
public class ApiEndpointHandlerExtensionsTests
{
    private readonly ApiEndpointHandler _handler;

    public ApiEndpointHandlerExtensionsTests()
    {
        _handler = new TestHandler(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
    }

    #region Ok<T> tests

    [Fact]
    public void Ok_CreatesSuccessfulResponseWithData()
    {
        // Arrange
        var testData = "test data";
        var message = "Operation successful";
        var statusCode = 201;

        // Act
        var response = _handler.Ok(testData, message, statusCode);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Data.Should().Be(testData);
        response.Message.Should().Be(message);
        response.StatusCode.Should().Be(statusCode);
        response.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Ok_WithNullMessage_UsesDefaultMessage()
    {
        // Arrange
        var testData = 42;

        // Act
        var response = _handler.Ok(testData);

        // Assert
        response.Message.Should().Be("Request completed successfully");
    }

    [Fact]
    public void Ok_WithDefaultStatusCode_Uses200()
    {
        // Arrange
        var testData = new List<int> { 1, 2, 3 };

        // Act
        var response = _handler.Ok(testData);

        // Assert
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Ok_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        ApiEndpointHandler handler = null!;
        var testData = "test";

        // Act
        Action act = () => handler.Ok(testData);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Error<T> tests

    [Fact]
    public void Error_CreatesErrorResponseWithMessage()
    {
        // Arrange
        var errorMessage = "Something went wrong";
        var statusCode = 400;
        var errorCode = "ERR-001";

        // Act
        var response = _handler.Error<string>(errorMessage, statusCode, errorCode);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().Be(errorMessage);
        response.StatusCode.Should().Be(statusCode);
        response.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Error_WithNullMessage_UsesDefaultMessage()
    {
        // Arrange

        // Act
        var response = _handler.Error<object>(null!);

        // Assert
        response.Message.Should().Be("An error occurred while processing the request");
    }

    [Fact]
    public void Error_WithDefaultStatusCode_Uses500()
    {
        // Arrange

        // Act
        var response = _handler.Error<int>("Error occurred");

        // Assert
        response.StatusCode.Should().Be(500);
    }

    [Fact]
    public void Error_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        ApiEndpointHandler handler = null!;

        // Act
        Action act = () => handler.Error<object>("Error");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region ToPaginatedResponse<T> tests

    [Fact]
    public void ToPaginatedResponse_CreatesPaginatedResponseWithCorrectData()
    {
        // Arrange
        var data = Enumerable.Range(1, 25).ToList();
        var page = 2;
        var pageSize = 10;
        var totalCount = 25;
        var message = "Page 2 of 3";

        // Act
        var response = _handler.ToPaginatedResponse(data, page, pageSize, totalCount, message);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data.Items.Should().HaveCount(pageSize);
        response.Data.Page.Should().Be(page);
        response.Data.PageSize.Should().Be(pageSize);
        response.Data.TotalCount.Should().Be(totalCount);
        response.Data.TotalPages.Should().Be(3);
        response.Message.Should().Be(message);
    }

    [Fact]
    public void ToPaginatedResponse_FirstPage_ReturnsFirstItems()
    {
        // Arrange
        var data = Enumerable.Range(1, 100).ToList();
        var page = 1;
        var pageSize = 10;
        var totalCount = 100;

        // Act
        var response = _handler.ToPaginatedResponse(data, page, pageSize, totalCount);

        // Assert
        response.Data.Items.Should().BeEquivalentTo(Enumerable.Range(1, 10));
    }

    [Fact]
    public void ToPaginatedResponse_LastPage_ReturnsRemainingItems()
    {
        // Arrange
        var data = Enumerable.Range(1, 25).ToList();
        var page = 3;
        var pageSize = 10;
        var totalCount = 25;

        // Act
        var response = _handler.ToPaginatedResponse(data, page, pageSize, totalCount);

        // Assert
        response.Data.Items.Should().BeEquivalentTo(Enumerable.Range(21, 5));
    }

    [Fact]
    public void ToPaginatedResponse_EmptyCollection_ReturnsEmptyItems()
    {
        // Arrange
        var data = new List<int>();
        var page = 1;
        var pageSize = 10;
        var totalCount = 0;

        // Act
        var response = _handler.ToPaginatedResponse(data, page, pageSize, totalCount);

        // Assert
        response.Data.Items.Should().BeEmpty();
        response.Data.TotalCount.Should().Be(0);
        response.Data.TotalPages.Should().Be(0);
    }

    [Fact]
    public void ToPaginatedResponse_PageSizeLargerThanTotal_ReturnsAllItems()
    {
        // Arrange
        var data = Enumerable.Range(1, 5).ToList();
        var page = 1;
        var pageSize = 100;
        var totalCount = 5;

        // Act
        var response = _handler.ToPaginatedResponse(data, page, pageSize, totalCount);

        // Assert
        response.Data.Items.Should().BeEquivalentTo(Enumerable.Range(1, 5));
        response.Data.TotalPages.Should().Be(1);
    }

    [Fact]
    public void ToPaginatedResponse_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        ApiEndpointHandler handler = null!;
        var data = Enumerable.Range(1, 10).ToList();

        // Act
        Action act = () => handler.ToPaginatedResponse(data, 1, 10, 10);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToPaginatedResponse_WithNullData_ThrowsArgumentNullException()
    {
        // Arrange
        var data = (IEnumerable<int>)null!;

        // Act
        Action act = () => _handler.ToPaginatedResponse(data, 1, 10, 10);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0, 10, 100)]
    [InlineData(-1, 10, 100)]
    public void ToPaginatedResponse_WithInvalidPage_ThrowsArgumentOutOfRangeException(int page, int pageSize, int totalCount)
    {
        // Arrange
        var data = Enumerable.Range(1, 100).ToList();

        // Act
        Action act = () => _handler.ToPaginatedResponse(data, page, pageSize, totalCount);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1, 0, 100)]
    [InlineData(1, -1, 100)]
    public void ToPaginatedResponse_WithInvalidPageSize_ThrowsArgumentOutOfRangeException(int page, int pageSize, int totalCount)
    {
        // Arrange
        var data = Enumerable.Range(1, 100).ToList();

        // Act
        Action act = () => _handler.ToPaginatedResponse(data, page, pageSize, totalCount);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ToPaginatedResponse_WithNegativeTotalCount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var data = Enumerable.Range(1, 100).ToList();

        // Act
        Action act = () => _handler.ToPaginatedResponse(data, 1, 10, -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region WithBatchStats tests

    [Fact]
    public void WithBatchStats_CreatesResponseWithCorrectStatistics()
    {
        // Arrange
        var successfulCount = 8;
        var failedCount = 2;
        var totalCount = 10;
        var message = "Batch processed successfully";
        var statusCode = 200;

        // Act
        var response = _handler.WithBatchStats(successfulCount, failedCount, totalCount, message, statusCode);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data.SuccessfulCount.Should().Be(successfulCount);
        response.Data.FailedCount.Should().Be(failedCount);
        response.Data.TotalCount.Should().Be(totalCount);
        response.Message.Should().Be(message);
        response.StatusCode.Should().Be(statusCode);
    }

    [Fact]
    public void WithBatchStats_WithDefaultMessage_UsesDefaultMessage()
    {
        // Arrange
        var successfulCount = 5;
        var failedCount = 3;
        var totalCount = 8;

        // Act
        var response = _handler.WithBatchStats(successfulCount, failedCount, totalCount);

        // Assert
        response.Message.Should().Be("Batch processed: 5 successful, 3 failed");
    }

    [Fact]
    public void WithBatchStats_WithDefaultStatusCode_Uses200()
    {
        // Arrange
        var successfulCount = 1;
        var failedCount = 0;
        var totalCount = 1;

        // Act
        var response = _handler.WithBatchStats(successfulCount, failedCount, totalCount);

        // Assert
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public void WithBatchStats_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        ApiEndpointHandler handler = null!;

        // Act
        Action act = () => handler.WithBatchStats(1, 0, 1);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(0, -1, 0)]
    public void WithBatchStats_WithNegativeSuccessfulCount_ThrowsArgumentOutOfRangeException(int successfulCount, int failedCount, int totalCount)
    {
        // Arrange

        // Act
        Action act = () => _handler.WithBatchStats(successfulCount, failedCount, totalCount);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, -1, 0)]
    [InlineData(5, -2, 7)]
    public void WithBatchStats_WithNegativeFailedCount_ThrowsArgumentOutOfRangeException(int successfulCount, int failedCount, int totalCount)
    {
        // Arrange

        // Act
        Action act = () => _handler.WithBatchStats(successfulCount, failedCount, totalCount);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, 0, -1)]
    [InlineData(5, 3, -1)]
    public void WithBatchStats_WithNegativeTotalCount_ThrowsArgumentOutOfRangeException(int successfulCount, int failedCount, int totalCount)
    {
        // Arrange

        // Act
        Action act = () => _handler.WithBatchStats(successfulCount, failedCount, totalCount);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(5, 3, 10)]
    [InlineData(0, 0, 1)]
    public void WithBatchStats_WithMismatchedTotals_ThrowsArgumentException(int successfulCount, int failedCount, int totalCount)
    {
        // Arrange

        // Act
        Action act = () => _handler.WithBatchStats(successfulCount, failedCount, totalCount);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region PaginatedResponse properties tests

    [Fact]
    public void PaginatedResponse_ItemsProperty_ReturnsReadOnlyList()
    {
        // Arrange
        var data = Enumerable.Range(1, 50).ToList();
        var page = 2;
        var pageSize = 20;
        var totalCount = 50;

        // Act
        var response = _handler.ToPaginatedResponse(data, page, pageSize, totalCount);

        // Assert
        response.Data.Items.Should().BeAssignableTo<IReadOnlyList<int>>();
        response.Data.Items.Should().NotBeNull();
    }

    [Fact]
    public void PaginatedResponse_PageProperty_ReturnsCorrectPage()
    {
        // Arrange
        var data = Enumerable.Range(1, 100).ToList();
        var page = 5;
        var pageSize = 10;
        var totalCount = 100;

        // Act
        var response = _handler.ToPaginatedResponse(data, page, pageSize, totalCount);

        // Assert
        response.Data.Page.Should().Be(page);
    }

    [Fact]
    public void PaginatedResponse_PageSizeProperty_ReturnsCorrectPageSize()
    {
        // Arrange
        var data = Enumerable.Range(1, 100).ToList();
        var pageSize = 25;

        // Act
        var response = _handler.ToPaginatedResponse(data, 1, pageSize, 100);

        // Assert
        response.Data.PageSize.Should().Be(pageSize);
    }

    [Fact]
    public void PaginatedResponse_TotalCountProperty_ReturnsTotalCount()
    {
        // Arrange
        var totalCount = 150;

        // Act
        var response = _handler.ToPaginatedResponse(Enumerable.Range(1, 150), 1, 50, totalCount);

        // Assert
        response.Data.TotalCount.Should().Be(totalCount);
    }

    [Fact]
    public void PaginatedResponse_TotalPagesProperty_CalculatesCorrectTotalPages()
    {
        // Test various page size scenarios
        var data = Enumerable.Range(1, 100).ToList();

        // 100 items, page size 10 = 10 pages
        var response1 = _handler.ToPaginatedResponse(data, 1, 10, 100);
        response1.Data.TotalPages.Should().Be(10);

        // 100 items, page size 25 = 4 pages
        var response2 = _handler.ToPaginatedResponse(data, 1, 25, 100);
        response2.Data.TotalPages.Should().Be(4);

        // 100 items, page size 100 = 1 page
        var response3 = _handler.ToPaginatedResponse(data, 1, 100, 100);
        response3.Data.TotalPages.Should().Be(1);

        // 101 items, page size 10 = 11 pages
        var response4 = _handler.ToPaginatedResponse(Enumerable.Range(1, 101), 1, 10, 101);
        response4.Data.TotalPages.Should().Be(11);
    }

    #endregion

    #region Test handler implementation

    private sealed class TestHandler : ApiEndpointHandler
    {
        public TestHandler(ILogger logger) : base(logger)
        {
        }
    }

    #endregion
}
