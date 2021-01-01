#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================

using DotNetRealtimePipeline.Domain.Enums;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Unit tests for the <see cref="WindowingService"/> class that verify windowing functionality including window creation,
/// data point assignment, statistics calculation, and window lifecycle management.
/// </summary>
public sealed class WindowingServiceTests
{
	/// <summary>
	/// Gets the windowing service instance used for testing window creation and management operations.
	/// </summary>
	private readonly WindowingService _service;

	/// <summary>
	/// Gets the pipeline configuration used to initialize the windowing service with specific window settings.
	/// </summary>
	private readonly PipelineConfig _config;

	/// <summary>
	/// Initializes a new instance of the <see cref="WindowingServiceTests"/> class with a configured pipeline configuration.
	/// </summary>
	public WindowingServiceTests()
	{
		_config = new PipelineConfig
		{
			WindowType = WindowType.TUMBLING,
			WindowSizeMs = 5000,
			WindowSlideMs = 5000
		};
		_service = new WindowingService(_config);
	}

	/// <summary>
	/// Tests that <see cref="WindowingService.CreateWindow"/> successfully creates a window with valid start time.
	/// </summary>
	[Fact]
	public void CreateWindow_WithValidTime_ShouldSucceed()
	{
		// Act
		var window = _service.CreateWindow(1000);

		// Assert
		Assert.NotNull(window);
		Assert.Equal(1000, window.StartTimeMs);
	}

	/// <summary>
	/// Tests that <see cref="WindowingService.AssignDataPointsToWindows"/> correctly assigns data points to appropriate windows
	/// based on their timestamps and the configured window settings.
	/// </summary>
	[Fact]
	public void AssignDataPointsToWindows_WithValidPoints_ShouldAssign()
	{
		// Arrange
		var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		var points = new[]
		{
			new DataPoint(1, now * 10000, 10, "S1"),
			new DataPoint(2, now * 10000, 20, "S1"),
			new DataPoint(3, (now + 2500) * 10000, 30, "S1")
		};

		// Act
		var windows = _service.AssignDataPointsToWindows(points);

		// Assert
		Assert.NotEmpty(windows);
	}

	/// <summary>
	/// Tests that <see cref="WindowingService.CalculateWindowStatistics"/> correctly calculates basic statistics
	/// (count, average, minimum, maximum) for a window containing multiple data points.
	/// </summary>
	[Fact]
	public void CalculateWindowStatistics_WithValidWindow_ShouldCalculate()
	{
		// Arrange
		var window = new WindowEvent
		{
			StartTimeMs = 1000,
			EndTimeMs = 6000,
			Data = new List<DataPoint>
			{
				new(1, 2000, 10, "S1"),
				new(2, 3000, 20, "S1"),
				new(3, 4000, 30, "S1")
			}
		};

		// Act
		var stats = _service.CalculateWindowStatistics(window);

		// Assert
		Assert.Equal(3, stats.Count);
		Assert.Equal(20, stats.Average);
		Assert.Equal(10, stats.Minimum);
		Assert.Equal(30, stats.Maximum);
	}

	/// <summary>
	/// Tests that <see cref="WindowingService.GetActiveWindows"/> returns the currently active windows
	/// without throwing exceptions.
	/// </summary>
	[Fact]
	public void GetActiveWindows_ShouldReturnCurrent()
	{
		// Act
		var windows = _service.GetActiveWindows();

		// Assert
		Assert.NotNull(windows);
	}

	/// <summary>
	/// Tests that <see cref="WindowingService.CalculateWindowStatistics"/> correctly computes standard deviation
	/// for a window containing multiple data points with varying values.
	/// </summary>
	[Fact]
	public void CalculateWindowStatistics_ShouldComputeStandardDeviation()
	{
		// Arrange
		var window = new WindowEvent
		{
			StartTimeMs = 1000,
			EndTimeMs = 6000,
			Data = new List<DataPoint>
			{
				new(1, 2000, 10, "S1"),
				new(2, 3000, 20, "S1"),
				new(3, 4000, 30, "S1"),
				new(4, 5000, 40, "S1")
			}
		};

		// Act
		var stats = _service.CalculateWindowStatistics(window);

		// Assert
		Assert.True(stats.StandardDeviation > 0);
	}

	/// <summary>
	/// Tests that <see cref="WindowingService.CalculateWindowStatistics"/> correctly computes percentiles (P50, P95, P99)
	/// for a window containing 100 data points with sequential values.
	/// </summary>
	[Fact]
	public void CalculateWindowStatistics_ShouldComputePercentiles()
	{
		// Arrange
		var window = new WindowEvent
		{
			StartTimeMs = 1000,
			EndTimeMs = 6000,
			Data = Enumerable.Range(1, 100)
				.Select(i => new DataPoint(i, (1000 + i) * 10000, i, "S1"))
				.ToList()
		};

		// Act
		var stats = _service.CalculateWindowStatistics(window);

		// Assert
		Assert.True(stats.P50 >= stats.Minimum);
		Assert.True(stats.P95 >= stats.P50);
		Assert.True(stats.P99 <= stats.Maximum);
	}

	/// <summary>
	/// Tests that <see cref="WindowingService.CloseWindow"/> properly archives a window with data
	/// and makes it available in the archived windows collection.
	/// </summary>
	[Fact]
	public void CloseWindow_WithActiveWindow_ShouldArchive()
	{
		// Arrange
		var window = _service.CreateWindow(1000);
		window.Data.Add(new DataPoint(1, 2000, 42, "S1"));

		// Act
		_service.CloseWindow(window);
		var archived = _service.GetArchivedWindows();

		// Assert
		Assert.NotEmpty(archived);
	}
}