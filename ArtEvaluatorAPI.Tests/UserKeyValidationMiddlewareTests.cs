using ArtEvaluatorAPI.Middleware;
using ArtEvaluatorAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArtEvaluatorAPI.Tests;

public class UserKeyValidationMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<AppDbContext> _dbContextMock;
    private readonly UserKeyValidationMiddleware _middleware;

    public UserKeyValidationMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _dbContextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        _middleware = new UserKeyValidationMiddleware(_nextMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_MissingUserKeyHeader_ReturnsUnauthorized()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
            .AddSingleton(_dbContextMock.Object)
            .BuildServiceProvider()
        };

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_InvalidUserKeyFormat_ReturnsForbidden()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["userkey"] = "invaliduserkey";
        context.RequestServices = new ServiceCollection()
            .AddSingleton(_dbContextMock.Object)
            .BuildServiceProvider();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

}
