// existing content ...

## StatisticsHelper
The `StatisticsHelper` class provides statistical analysis methods for numerical datasets, including mean, median, standard deviation, percentile calculation, outlier detection, and correlation analysis. It supports both single-value calculations and time-series analysis like moving averages.

### Usage Examples

#### Calculate basic statistics
```csharp
List<double> values = new List<double> { 12.5, 15.3, 14.7, 16.2, 13.8, 100.0 };

double mean = StatisticsHelper.CalculateMean(values);
double median = StatisticsHelper.CalculateMedian(values);
double stdDev = StatisticsHelper.CalculateStandardDeviation(values);
double iqr90 = StatisticsHelper.CalculatePercentile(values, 90);

Console.WriteLine($"Mean: {mean:F2}");
Console.WriteLine($"Median: {median:F2}");
Console.WriteLine($"Std Dev: {stdDev:F2}");
Console.WriteLine($"90th Percentile: {iqr90:F2}");
```

#### Analyze time-series data
```csharp
List<double> timeSeries = new List<double> { 10, 12, 14, 13, 15, 18, 16, 20 };
var movingAverages = StatisticsHelper.CalculateMovingAverage(timeSeries, 3);

Console.WriteLine("3-period Moving Averages:");
Console.WriteLine(string.Join(", ", movingAverages.Select(x => x.ToString("F2"))));
```

#### Detect outliers and correlation
```csharp
List<double> xValues = new List<double> { 1, 2, 3, 4, 5 };
List<double> yValues = new List<double> { 2, 4, 6, 8, 10 };

var outliers = StatisticsHelper.FindOutliers(values);
double correlation = StatisticsHelper.CalculateCorrelation(xValues, yValues);

Console.WriteLine($"Outliers: {string.Join(", ", outliers)}");
Console.WriteLine($"Correlation: {correlation:F2}");
```
