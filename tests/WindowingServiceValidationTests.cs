using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Xunit;
using DotNetRealtimePipeline.Services;

namespace DotNetRealtimePipeline.Services.Tests;

public class WindowingServiceValidationTests
{
    private static WindowingService CreateValidInstance()
    {
        // Create an instance without invoking any constructor.
        var instance = (WindowingService)FormatterServices.GetUninitializedObject(typeof(WindowingService));

        var type = typeof(WindowingService);
        var binding = BindingFlags.NonPublic | BindingFlags.Instance;

        // _config must be non‑null.
        var configField = type.GetField("_config", binding);
        configField?.SetValue(instance, new object());

        // _activeWindows must be a non‑null IDictionary with a non‑negative Count.
        var activeWindowsField = type.GetField("_activeWindows", binding);
        var dict = new Dictionary<string, object>();
        activeWindowsField?.SetValue(instance, dict);

        // _nextWindowId must be non‑negative.
        var nextWindowIdField = type.GetField("_nextWindowId", binding);
        nextWindowIdField?.SetValue(instance, 0L);

        return instance;
    }

    [Fact]
    public void Validate_ReturnsEmpty_WhenInstanceIsValid()
    {
        var ws = CreateValidInstance();

        var problems = ws.Validate();

        Assert.Empty(problems);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenInstanceIsValid()
    {
        var ws = CreateValidInstance();

        Assert.True(ws.IsValid());
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_WhenInstanceIsValid()
    {
        var ws = CreateValidInstance();

        var ex = Record.Exception(() => ws.EnsureValid());

        Assert.Null(ex);
    }

    [Fact]
    public void Validate_ReturnsProblem_WhenConfigIsNull()
    {
        // Create an instance with a null _config but other fields valid.
        var ws = (WindowingService)FormatterServices.GetUninitializedObject(typeof(WindowingService));
        var type = typeof(WindowingService);
        var binding = BindingFlags.NonPublic | BindingFlags.Instance;

        var activeWindowsField = type.GetField("_activeWindows", binding);
        activeWindowsField?.SetValue(ws, new Dictionary<string, object>());

        var nextWindowIdField = type.GetField("_nextWindowId", binding);
        nextWindowIdField?.SetValue(ws, 0L);

        var problems = ws.Validate();

        Assert.Contains("WindowingService configuration cannot be null.", problems);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_WhenInvalid()
    {
        // Create an instance with a negative next window id (and null config).
        var ws = (WindowingService)FormatterServices.GetUninitializedObject(typeof(WindowingService));
        var type = typeof(WindowService);
        var binding = BindingFlags.NonPublic | BindingFlags.Instance;

        var activeWindowsField = type.GetField("_activeWindows", binding);
        activeWindowsField?.SetValue(ws, new Dictionary<string, object>());

        var nextWindowIdField = type.GetField("_nextWindowId", binding);
        nextWindowIdField?.SetValue(ws, -5L);

        var ex = Assert.Throws<ArgumentException>(() => ws.EnsureValid());

        Assert.Contains("Next window ID must be non-negative", ex.Message);
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenValueIsNull()
    {
        WindowingService? ws = null;
        Assert.Throws<ArgumentNullException>(() => WindowingServiceValidation.Validate(ws!));
    }

    [Fact]
    public void IsValid_ThrowsArgumentNullException_WhenValueIsNull()
    {
        WindowingService? ws = null;
        Assert.Throws<ArgumentNullException>(() => ws!.IsValid());
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentNullException_WhenValueIsNull()
    {
        WindowingService? ws = null;
        Assert.Throws<ArgumentNullException>(() => ws!.EnsureValid());
    }
}
