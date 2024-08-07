using ArtEvaluatorAPI.Configurations;
using ArtEvaluatorAPI.Controllers;
using ArtEvaluatorAPI.Models;
using ArtEvaluatorAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace ArtEvaluatorAPI.Tests;

public class ChatControllerTests
{
    private readonly IOptions<AppSettings> _appSettings;
    private readonly IOptions<OpenAIOptions> _openAIOptions;
    private readonly IChatPromptService _chatPromptService;
    private readonly IDummyResponseService _dummyResponseService;
    private readonly AppDbContext _context;

    public ChatControllerTests()
    {
        var appSettings = new AppSettings { TestMode = true };
        var openAIOptions = new OpenAIOptions { ApiKey = "test-api-key" };

        _appSettings = Options.Create(appSettings);
        _openAIOptions = Options.Create(openAIOptions);
        _chatPromptService = Mock.Of<IChatPromptService>(x => x.ChatPrompt == "Test prompt");
        _dummyResponseService = Mock.Of<IDummyResponseService>(x => x.DummyResponse == "Dummy response");
        _context = GetInMemoryDbContext();
    }

    private static AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task ProcessImage_ReturnsBadRequest_WhenImageFileIsNull()
    {
        // Arrange
        var controller = new ChatController(_appSettings, _chatPromptService, _dummyResponseService, _openAIOptions, _context);
#nullable disable
        var request = new ImageProcessingRequest
        {
            ImageFile = null // Ensure ImageFile is set to null
        };
#nullable restore

        // Act
        var result = await controller.ProcessImage(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No image file provided.", badRequestResult.Value);
    }

    [Fact]
    public async Task ProcessImage_ReturnsBadRequest_WhenImageFileIsEmpty()
    {
        // Arrange
        var controller = new ChatController(_appSettings, _chatPromptService, _dummyResponseService, _openAIOptions, _context);

        var mockFormFile = new Mock<IFormFile>();
        mockFormFile.Setup(f => f.Length).Returns(0);

        var request = new ImageProcessingRequest
        {
            ImageFile = mockFormFile.Object
        };

        // Act
        var result = await controller.ProcessImage(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No image file provided.", badRequestResult.Value);
    }
}
