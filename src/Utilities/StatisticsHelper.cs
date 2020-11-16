#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Statistical calculation results for pipeline analytics.
/// </summary>
public sealed record StatisticsHelper
{
    /// <summary>
    /// Gets the mean (average) of a dataset.
    /// </summary>
    public double Mean { get; init; }

    /// <summary>
    /// Gets the median of a dataset.
    /// </summary>
    public double Median { get; init; }

    /// <summary>
    /// Gets the standard deviation of a dataset.
    /// </summary>
    public double StandardDeviation { get; init; }

    /// <summary>
    /// Gets the coefficient of variation.
    /// </summary>
    public double CoefficientOfVariation { get; init; }

    /// <summary>
    /// Gets the list of outliers found using the IQR method.
    /// </summary>
    public IReadOnlyList<double> Outliers { get; init; } = Array.Empty<double>();

    /// <summary>
    /// Creates a new <see cref="StatisticsHelper"/> instance with calculated statistics.
    /// </summary>
    public static StatisticsHelper FromData(
        List<double> values,
        double? mean = null,
        double? median = null,
        double? standardDeviation = null,
        double? coefficientOfVariation = null,
        List<double>? outliers = null)
    {
        return new StatisticsHelper
        {
            Mean = mean ?? CalculateMean(values),
            Median = median ?? CalculateMedian(values),
            StandardDeviation = standardDeviation ?? CalculateStandardDeviation(values),
            CoefficientOfVariation = coefficientOfVariation ?? CalculateCoefficientOfVariation(values),
            Outliers = outliers ?? FindOutliers(values)
        };
    }

    /// <summary>
    /// Calculates the mean (average) of a dataset.
    /// </summary>
    public static double CalculateMean(List<double> values)
    {
        if (values is null || values.Count == 0) return 0d;
        return values.Sum() / values.Count;
    }

    /// <summary>
    /// Calculates the median of a dataset.
    /// </summary>
    public static double CalculateMedian(List<double> values)
    {
        if (values is null || values.Count == 0) return 0d;

        var sorted = values.OrderBy(x => x).ToList();
        int count = sorted.Count;

        if (count % 2 == 0)
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
        else
            return sorted[count / 2];
    }

    /// <summary>
    /// Calculates the standard deviation of a dataset.
    /// </summary>
    public static double CalculateStandardDeviation(List<double> values)
    {
        if (values is null || values.Count <= 1) return 0d;

        double mean = CalculateMean(values);
        double sumOfSquares = values.Sum(x => Math.Pow(x - mean, 2));
        return Math.Sqrt(sumOfSquares / values.Count);
    }

    /// <summary>
    /// Calculates a percentile from a dataset.
    /// </summary>
    public static double CalculatePercentile(List<double> values, int percentile)
    {
        if (values is null || values.Count == 0) return 0d;
        if (percentile < 0 || percentile > 100)
            throw new ArgumentException("Percentile must be between 0 and 100", nameof(percentile));

        var sorted = values.OrderBy(x => x).ToList();
        int index = (int)Math.Ceiling((percentile / 100.0) * sorted.Count) - 1;
        index = Math.Max(0, Math.Min(index, sorted.Count - 1));

        return sorted[index];
    }

    /// <summary>
    /// Calculates moving average over a window.
    /// </summary>
    public static List<double> CalculateMovingAverage(List<double> values, int windowSize)
    {
        if (values is null || values.Count < windowSize)
            return new();

        var result = new List<double>();

        for (int i = 0; i <= values.Count - windowSize; i++)
        {
            double avg = values.Skip(i).Take(windowSize).Average();
            result.Add(avg);
        }

        return result;
    }

    /// <summary>
    /// Calculates the rate of change between two values.
    /// </summary>
    public static double CalculateRateOfChange(double oldValue, double newValue)
    {
        if (oldValue == 0) return 0d;
        return ((newValue - oldValue) / oldValue) * 100;
    }

    /// <summary>
    /// Calculates the coefficient of variation.
    /// </summary>
    public static double CalculateCoefficientOfVariation(List<double> values)
    {
        if (values is null || values.Count == 0) return 0d;

        double mean = CalculateMean(values);
        if (mean == 0) return 0d;

        double stdDev = CalculateStandardDeviation(values);
        return (stdDev / mean) * 100;
    }

    /// <summary>
    /// Finds outliers using the IQR method.
    /// </summary>
    public static List<double> FindOutliers(List<double> values)
    {
        if (values is null || values.Count < 4) return new();

        double q1 = CalculatePercentile(values, 25);
        double q3 = CalculatePercentile(values, 75);
        double iqr = q3 - q1;
        double lowerBound = q1 - (1.5 * iqr);
        double upperBound = q3 + (1.5 * iqr);

        return values.Where(x => x < lowerBound || x > upperBound).ToList();
    }

    /// <summary>
    /// Calculates the Pearson correlation coefficient between two datasets.
    /// </summary>
    public static double CalculateCorrelation(List<double> x, List<double> y)
    {
        if (x is null || y is null || x.Count != y.Count || x.Count < 2)
            return 0d;

        double meanX = CalculateMean(x);
        double meanY = CalculateMean(y);

        double numerator = 0;
        double denominatorX = 0;
        double denominatorY = 0;

        for (int i = 0; i < x.Count; i++)
        {
            double diffX = x[i] - meanX;
            double diffY = y[i] - meanY;

            numerator += diffX * diffY;
            denominatorX += diffX * diffX;
            denominatorY += diffY * diffY;
        }

        double denominator = Math.Sqrt(denominatorX * denominatorY);
        if (denominator == 0) return 0d;

        return numerator / denominator;
    }
}
