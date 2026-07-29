using System;
using System.Runtime.Serialization;
using Xunit;
using DotNetRealtimePipeline.Data;

namespace DotNetRealtimePipeline.Tests;

public class ExportServiceJsonExtensionsTests
{
    private static ExportService CreateExportService()
    {
        // Create an instance without invoking any constructor.
        // This works even if ExportService only has parameterized constructors.
        return (ExportService)FormatterServices.GetUninitializedObject(typeof(ExportService));
    }

    [Fact]
    public void ToJson_ReturnsJsonString_ForValidInstance()
    {
        var service = CreateExportService();
        var json = service.ToJson();

        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.StartsWith("{", json);
        Assert.EndsWith("}", json);
    }

    [Fact]
    public void ToJson_IndentsJson_WhenRequested()
    {
        var service = CreateExportService();
        var json = service.ToJson(indented: true);

        // Indented JSON should contain at least one newline character.
        Assert.Contains("\n", json);
    }

    [Fact]
    public void ToJson_ThrowsArgumentNullException_WhenNull()
    {
        ExportService? service = null;
        Assert.Throws<ArgumentNullException>(() => service!.ToJson());
    }

    [Fact]
    public void FromJson_ReturnsInstance_WhenValidJson()
    {
        var original = CreateExportService();
        var json = original.ToJson();

        var deserialized = ExportServiceJsonExtensions.FromJson(json);

        Assert.NotNull(deserialized);
        Assert.IsType<ExportService>(deserialized);
    }

    [Fact]
    public void FromJson_ReturnsNull_WhenEmptyString()
    {
        var result = ExportServiceJsonExtensions.FromJson(string.Empty);
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_ReturnsNull_WhenNullString()
    {
        var result = ExportServiceJsonExtensions.FromJson(null!);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_ReturnsTrueAndInstance_WhenValidJson()
    {
        var original = CreateExportService();
        var json = original.ToJson();

        var success = ExportServiceJsonExtensions.TryFromJson(json, out var deserialized);

        Assert.True(success);
        Assert.NotNull(deserialized);
        Assert.IsType<ExportService>(deserialized);
    }

    [Fact]
    public void TryFromJson_ReturnsFalse_WhenInvalidJson()
    {
        var invalidJson = "{ invalid json }";

        var success = ExportServiceJsonExtensions.TryFromJson(invalidJson, out var deserialized);

        Assert.False(success);
        Assert.Null(deserialized);
    }

    [Fact]
    public void TryFromJson_ReturnsFalse_WhenEmptyString()
    {
        var success = ExportServiceJsonExtensions.TryFromJson(string.Empty, out var deserialized);

        Assert.False(success);
        Assert.Null(deserialized);
    }
}
