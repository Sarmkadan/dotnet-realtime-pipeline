# StatisticsHelper

Provides a collection of static methods for computing common descriptive statistics, time-series transformations, and outlier detection on numeric data sets. All operations are performed on in-memory collections of `double` values and return results synchronously.

## API

### CalculateMean

```csharp
public static double CalculateMean(IEnumerable<double> values)
```

Computes the arithmetic mean of a sequence.

**Parameters**
- `values` — the numeric sequence to evaluate.

**Returns**
The average of all elements in the sequence.

**Exceptions**
- `ArgumentNullException` — `values` is `null`.
- `InvalidOperationException` — `values` contains no elements.

---

### CalculateMedian

```csharp
public static double CalculateMedian(IEnumerable<double> values)
```

Computes the median value. For an even number of elements, returns the average of the two middle values after sorting.

**Parameters**
- `values` — the numeric sequence to evaluate.

**Returns**
The median.

**Exceptions**
- `ArgumentNullException` — `values` is `null`.
- `InvalidOperationException` — `values` contains no elements.

---

### CalculateStandardDeviation

```csharp
public static double CalculateStandardDeviation(IEnumerable<double> values)
```

Computes the sample standard deviation (using Bessel's correction, denominator *n − 1*).

**Parameters**
- `values` — the numeric sequence to evaluate.

**Returns**
The sample standard deviation.

**Exceptions**
- `ArgumentNullException` — `values` is `null`.
- `InvalidOperationException` — `values` contains fewer than two elements.

---

### CalculatePercentile

```csharp
public static double CalculatePercentile(IEnumerable<double> values, double percentile)
```

Computes the value at a given percentile using linear interpolation between closest ranks.

**Parameters**
- `values` — the numeric sequence to evaluate.
- `percentile` — a value between 0 and 100 inclusive.

**Returns**
The interpolated value at the specified percentile.

**Exceptions**
- `ArgumentNullException` — `values` is `null`.
- `InvalidOperationException` — `values` contains no elements.
- `ArgumentOutOfRangeException` — `percentile` is less than 0 or greater than 100.

---

### CalculateMovingAverage

```csharp
public static List<double> CalculateMovingAverage(IEnumerable<double> values, int windowSize)
```

Computes the simple moving average over a sliding window. The result has `values.Count - windowSize + 1` elements when the sequence length is at least the window size.

**Parameters**
- `values` — the numeric sequence to evaluate.
- `windowSize` — the number of consecutive elements in each window.

**Returns**
A list of moving-average values in the order of the sliding windows.

**Exceptions**
- `ArgumentNullException` — `values` is `null`.
- `ArgumentOutOfRangeException` — `windowSize` is less than 1 or greater than the number of elements in `values`.

---

### CalculateRateOfChange

```csharp
public static double CalculateRateOfChange(IEnumerable<double> values)
```

Computes the overall rate of change from the first element to the last element, expressed as a proportion: `(last - first) / first`.

**Parameters**
- `values` — the numeric sequence to evaluate.

**Returns**
The rate of change as a `double`. Returns `0` when the first element is `0`.

**Exceptions**
- `ArgumentNullException` — `values` is `null`.
- `InvalidOperationException` — `values` contains fewer than two elements.

---

### CalculateCoefficientOfVariation

```csharp
public static double CalculateCoefficientOfVariation(IEnumerable<double> values)
```

Computes the coefficient of variation as the ratio of the sample standard deviation to the absolute value of the mean.

**Parameters**
- `values` — the numeric sequence to evaluate.

**Returns**
The coefficient of variation. Returns `0` when the mean is `0`.

**Exceptions**
- `ArgumentNullException` — `values` is `null`.
- `InvalidOperationException` — `values` contains fewer than two elements.

---

### FindOutliers

```csharp
public static List<double> FindOutliers(IEnumerable<double> values, double threshold = 1.5)
```

Identifies outliers using the interquartile range (IQR) method. Values below `Q1 - threshold * IQR` or above `Q3 + threshold * IQR` are returned.

**Parameters**
- `values` — the numeric sequence to evaluate.
- `threshold` — the multiplier for the IQR (default `1.5`).

**Returns**
A list of outlier values in the order they appear in the original sequence.

**Exceptions**
- `ArgumentNullException` — `values` is `null`.
- `InvalidOperationException` — `values` contains fewer than four elements.
- `ArgumentOutOfRangeException` — `threshold` is less than or equal to `0`.

---

### CalculateCorrelation

```csharp
public static double CalculateCorrelation(IEnumerable<double> x, IEnumerable<double> y)
```

Computes the Pearson product-moment correlation coefficient between two sequences of equal length.

**Parameters**
- `x` — the first numeric sequence.
- `y` — the second numeric sequence.

**Returns**
The correlation coefficient in the range `[-1, 1]`.

**Exceptions**
- `ArgumentNullException` — either `x` or `y` is `null`.
- `InvalidOperationException` — either sequence contains fewer than two elements.
- `ArgumentException` — the sequences have different lengths.

## Usage

### Basic descriptive statistics on a sensor reading batch

```csharp
double[] temperatures = { 22.1, 22.5, 21.9, 23.0, 22.3, 45.7, 22.0 };

double mean = StatisticsHelper.CalculateMean(temperatures);
double median = StatisticsHelper.CalculateMedian(temperatures);
double stdDev = StatisticsHelper.CalculateStandardDeviation(temperatures);
double cv = StatisticsHelper.CalculateCoefficientOfVariation(temperatures);
List<double> outliers = StatisticsHelper.FindOutliers(temperatures);

Console.WriteLine($"Mean: {mean:F2}, Median: {median:F2}, StdDev: {stdDev:F2}, CV: {cv:F2}");
Console.WriteLine($"Outliers: {string.Join(", ", outliers)}");
```

### Time-series smoothing and correlation

```csharp
List<double> prices = new() { 100, 102, 101, 104, 107, 110, 108 };
List<double> volumes = new() { 1500, 1520, 1480, 1600, 1650, 1700, 1620 };

List<double> smoothed = StatisticsHelper.CalculateMovingAverage(prices, windowSize: 3);
double roc = StatisticsHelper.CalculateRateOfChange(prices);
double correlation = StatisticsHelper.CalculateCorrelation(prices, volumes);

Console.WriteLine($"Smoothed prices: [{string.Join(", ", smoothed.Select(v => v.ToString("F2")))}]");
Console.WriteLine($"Rate of change: {roc:P2}");
Console.WriteLine($"Price/volume correlation: {correlation:F3}");
```

## Notes

- All methods iterate the input `IEnumerable<double>` at least once; callers should pass materialized collections when multiple operations are performed on the same data to avoid re-enumeration costs.
- `CalculateStandardDeviation` and `CalculateCoefficientOfVariation` use sample statistics (*n − 1* denominator). Single-element sequences will throw because sample variance is undefined.
- `CalculatePercentile` uses linear interpolation and expects the percentile parameter in the 0–100 scale (not 0–1).
- `CalculateRateOfChange` and `CalculateCoefficientOfVariation` return `0` when the denominator is zero, rather than throwing or returning `NaN`. Callers should check for this case if zero-meaningful data is expected.
- `FindOutliers` requires at least four elements to compute meaningful quartiles. The default threshold of `1.5` corresponds to the standard Tukey fence definition.
- None of the methods are thread-safe by design; they operate on their own transient state. Concurrent reads from the same input collection are safe provided the collection is not modified during enumeration.
