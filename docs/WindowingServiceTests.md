# WindowingServiceTests
The `WindowingServiceTests` class is designed to test the functionality of the `WindowingService` class, which is responsible for managing windows of data points and calculating statistics for these windows. This test class ensures that the `WindowingService` behaves correctly under various scenarios, including creating windows, assigning data points, calculating statistics, and closing windows.

## API
* `public WindowingServiceTests`: The constructor for the `WindowingServiceTests` class, used to initialize the test class.
* `public void CreateWindow_WithValidTime_ShouldSucceed`: Tests that creating a window with a valid time range succeeds. This method does not take any parameters and does not return a value. It throws an exception if the window creation fails.
* `public void AssignDataPointsToWindows_WithValidPoints_ShouldAssign`: Tests that assigning valid data points to windows succeeds. This method does not take any parameters and does not return a value. It throws an exception if the data point assignment fails.
* `public void CalculateWindowStatistics_WithValidWindow_ShouldCalculate`: Tests that calculating statistics for a valid window succeeds. This method does not take any parameters and does not return a value. It throws an exception if the statistic calculation fails.
* `public void GetActiveWindows_ShouldReturnCurrent`: Tests that getting the active windows returns the current windows. This method does not take any parameters and does not return a value. It throws an exception if the active windows are not returned correctly.
* `public void CalculateWindowStatistics_ShouldComputeStandardDeviation`: Tests that calculating statistics for a window computes the standard deviation correctly. This method does not take any parameters and does not return a value. It throws an exception if the standard deviation calculation fails.
* `public void CalculateWindowStatistics_ShouldComputePercentiles`: Tests that calculating statistics for a window computes the percentiles correctly. This method does not take any parameters and does not return a value. It throws an exception if the percentile calculation fails.
* `public void CloseWindow_WithActiveWindow_ShouldArchive`: Tests that closing a window with active data points archives the window correctly. This method does not take any parameters and does not return a value. It throws an exception if the window archiving fails.

## Usage
The following examples demonstrate how to use the `WindowingServiceTests` class:
```csharp
// Example 1: Creating a window and assigning data points
WindowingServiceTests tests = new WindowingServiceTests();
tests.CreateWindow_WithValidTime_ShouldSucceed();
tests.AssignDataPointsToWindows_WithValidPoints_ShouldAssign();

// Example 2: Calculating statistics and closing a window
tests.CalculateWindowStatistics_WithValidWindow_ShouldCalculate();
tests.CloseWindow_WithActiveWindow_ShouldArchive();
```

## Notes
The `WindowingServiceTests` class is designed to be used in a testing environment, and its methods should not be used in production code. The class is not thread-safe, and its methods should not be called concurrently. Additionally, the class assumes that the `WindowingService` class is properly configured and initialized before use. Edge cases, such as creating a window with an invalid time range or assigning invalid data points, are not handled by this class and should be tested separately.
