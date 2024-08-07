using ArtEvaluatorAPI.Services;
using Moq;
using Serilog;
using System;
using System.IO;
using Xunit;

namespace ArtEvaluatorAPI.Tests;

public class ChatPromptServiceTests
{
    private const string TestChatPromptFileName = "ChatPrompt.txt";
    private readonly string _testChatPromptFilePath;

    public ChatPromptServiceTests()
    {
        _testChatPromptFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TestChatPromptFileName);
    }

    [Fact]
    public void Constructor_FileExists_ReadsChatPrompt()
    {
        // Arrange
        var expectedChatPrompt = "This is a test prompt.";
        File.WriteAllText(_testChatPromptFilePath, expectedChatPrompt);

        // Act
        var service = new ChatPromptService();

        // Assert
        Assert.Equal(expectedChatPrompt, service.ChatPrompt);

        // Cleanup
        File.Delete(_testChatPromptFilePath);
    }

    [Fact]
    public void Constructor_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        if (File.Exists(_testChatPromptFilePath))
        {
            File.Delete(_testChatPromptFilePath);
        }

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => new ChatPromptService());
        Assert.Contains("ChatPrompt.txt file not found", exception.Message);
    }
}
