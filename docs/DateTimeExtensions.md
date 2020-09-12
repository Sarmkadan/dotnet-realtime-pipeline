# DateTimeExtensions
The `DateTimeExtensions` class provides a set of static methods for working with dates and times in the context of a real-time pipeline. It offers functionality for converting between Unix milliseconds and `DateTime` objects, calculating window boundaries, and determining the age of events. These methods are designed to be used in a real-time data processing pipeline, where efficient and accurate date and time calculations are crucial.

## API
* `public static long ToUnixMilliseconds(this DateTime dateTime)`: Converts a `DateTime` object to Unix milliseconds. This method takes a `DateTime` object as a parameter and returns the equivalent Unix milliseconds as a `long` value. It does not throw any exceptions.
* `public static DateTime FromUnixMilliseconds(long unixMilliseconds)`: Converts Unix milliseconds to a `DateTime` object. This method takes a `long` value representing Unix milliseconds as a parameter and returns the equivalent `DateTime` object. It does not throw any exceptions.
* `public static long GetCurrentUnixMilliseconds()`: Returns the current Unix milliseconds. This method takes no parameters and returns the current Unix milliseconds as a `long` value. It does not throw any exceptions.
* `public static long GetWindowStart(long unixMilliseconds, long windowSizeMs)`: Calculates the start of a window given the provided Unix milliseconds and window size. This method takes two `long` values as parameters: `unixMilliseconds` and `windowSizeMs`. It returns the start of the window as a `long` value. It does not throw any exceptions.
* `public static long GetWindowEnd(long unixMilliseconds, long windowSizeMs)`: Calculates the end of a window given the provided Unix milliseconds and window size. This method takes two `long` values as parameters: `unixMilliseconds` and `windowSizeMs`. It returns the end of the window as a `long` value. It does not throw any exceptions.
* `public static long GetAgeMs(long eventUnixMilliseconds)`: Calculates the age of an event in milliseconds given the event's Unix milliseconds. This method takes a `long` value as a parameter and returns the age of the event in milliseconds as a `long` value. It does not throw any exceptions.
* `public static long RoundToWindowBoundary(long unixMilliseconds, long windowSizeMs)`: Rounds the provided Unix milliseconds to the nearest window boundary. This method takes two `long` values as parameters: `unixMilliseconds` and `windowSizeMs`. It returns the rounded Unix milliseconds as a `long` value. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `DateTimeExtensions` class:
```csharp
// Example 1: Converting between DateTime and Unix milliseconds
DateTime now = DateTime.Now;
long unixMilliseconds = now.ToUnixMilliseconds();
DateTime dateTime = DateTimeExtensions.FromUnixMilliseconds(unixMilliseconds);
Console.WriteLine($"Unix milliseconds: {unixMilliseconds}, DateTime: {dateTime}");

// Example 2: Calculating window boundaries and event age
long windowSizeMs = 10000; // 10 seconds
long eventUnixMilliseconds = 1643723400000; // some event time
long windowStart = DateTimeExtensions.GetWindowStart(eventUnixMilliseconds, windowSizeMs);
long windowEnd = DateTimeExtensions.GetWindowEnd(eventUnixMilliseconds, windowSizeMs);
long eventAgeMs = DateTimeExtensions.GetAgeMs(eventUnixMilliseconds);
Console.WriteLine($"Window start: {windowStart}, Window end: {windowEnd}, Event age: {eventAgeMs} ms");
```

## Notes
The `DateTimeExtensions` class is designed to be thread-safe, as all methods are static and do not rely on any shared state. However, it is essential to note that the `GetCurrentUnixMilliseconds` method returns the current time, which may vary slightly between threads or even within the same thread due to the nature of system clocks. When working with window boundaries, it is crucial to consider the window size and the event time to ensure accurate calculations. Additionally, the `RoundToWindowBoundary` method uses the provided window size to determine the nearest boundary, which may result in rounding up or down depending on the event time's position within the window.
