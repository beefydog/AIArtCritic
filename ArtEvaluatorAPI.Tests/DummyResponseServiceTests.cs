using ArtEvaluatorAPI.Services;
using Moq;
using Serilog;
using System;
using System.IO;
using Xunit;

namespace ArtEvaluatorAPI.Tests;

public class DummyResponseServiceTests
{
    private const string TestDummyResponseFileName = "DummyResponse.txt";
    private readonly string _testDummyResponseFilePath;

    public DummyResponseServiceTests()
    {
        _testDummyResponseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TestDummyResponseFileName);
    }

    [Fact]
    public void Constructor_FileExists_ReadsDummyResponse()
    {
        // Arrange
        var expectedDummyResponse = "This is a dummy response.";
        File.WriteAllText(_testDummyResponseFilePath, expectedDummyResponse);

        // Act
        var service = new DummyResponseService();

        // Assert
        Assert.Equal(expectedDummyResponse, service.DummyResponse);

        // Cleanup
        File.Delete(_testDummyResponseFilePath);
    }

    [Fact]
    public void Constructor_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        if (File.Exists(_testDummyResponseFilePath))
        {
            File.Delete(_testDummyResponseFilePath);
        }

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => new DummyResponseService());
        Assert.Contains("DummyResponse.txt file not found", exception.Message);
    }
}
