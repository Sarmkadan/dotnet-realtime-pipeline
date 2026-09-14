#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Formatters;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="IOutputFormatter"/>.
/// </summary>
public static class OutputFormatterExtensions
{
    /// <summary>
    /// Asynchronously formats the specified data and writes it to a file.
    /// </summary>
    /// <typeparam name="T">The reference type of the data to format.</typeparam>
    /// <param name="formatter">The output formatter.</param>
    /// <param name="data">The data to format.</param>
    /// <param name="path">The file path to write the formatted data to.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
    public static async Task WriteToFileAsync<T>(this IOutputFormatter formatter, T data, string path) where T : class
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(path);

        var formatted = await formatter.FormatAsync(data);
        await File.WriteAllTextAsync(path, formatted);
    }

    /// <summary>
    /// Asynchronously formats the specified data and writes it to a text writer.
    /// </summary>
    /// <typeparam name="T">The reference type of the data to format.</typeparam>
    /// <param name="formatter">The output formatter.</param>
    /// <param name="data">The data to format.</param>
    /// <param name="writer">The text writer to write the formatted data to.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is <see langword="null"/>.</exception>
    public static async Task WriteToAsync<T>(this IOutputFormatter formatter, T data, TextWriter writer) where T : class
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(writer);

        var formatted = await formatter.FormatAsync(data);
        await writer.WriteAsync(formatted);
    }

    /// <summary>
    /// Formats all items in the enumeration and concatenates them with the specified separator.
    /// </summary>
    /// <typeparam name="T">The reference type of the items to format.</typeparam>
    /// <param name="formatter">The output formatter.</param>
    /// <param name="items">The items to format.</param>
    /// <param name="separator">The separator to use between formatted items. Defaults to a newline.</param>
    /// <returns>A string containing all formatted items separated by <paramref name="separator"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is <see langword="null"/>.</exception>
    public static string FormatAll<T>(this IOutputFormatter formatter, IEnumerable<T> items, string separator = "\n") where T : class
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(items);

        if (separator == null)
        {
            separator = "\n";
        }

        var formattedItems = new List<string>();
        foreach (var item in items)
        {
            // Note: We allow items to be null? The formatter's Format method expects T: class, but null is allowed.
            // However, the formatter might throw. We'll let the formatter handle null items.
            formattedItems.Add(formatter.Format(item));
        }

        return string.Join(separator, formattedItems);
    }
}