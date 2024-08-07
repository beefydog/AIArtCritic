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

public class UserKeyValidationMiddlewareIntegrationTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly UserKeyValidationMiddleware _middleware;
    private readonly string _connectionString;

    public UserKeyValidationMiddlewareIntegrationTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _middleware = new UserKeyValidationMiddleware(_nextMock.Object);
        _connectionString = "Server=chi,61433;Database=UserAPIAccess;Trusted_Connection=True;TrustServerCertificate=True;";
    }

    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_connectionString)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task InvokeAsync_MissingUserKeyHeader_ReturnsUnauthorized()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
            .AddSingleton(GetDbContext())
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
            .AddSingleton(GetDbContext())
            .BuildServiceProvider();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ValidUserKey_CallsNextMiddleware()
    {
        // Arrange
        var validGuid = Guid.NewGuid();
        var validUserKey = Convert.ToBase64String(validGuid.ToByteArray()).TrimEnd('=');
        var context = new DefaultHttpContext();
        context.Request.Headers["userkey"] = validUserKey;

        var dbContext = GetDbContext();
        dbContext.UserKeys.Add(new UserKey { Key = validGuid.ToString() });
        dbContext.SaveChanges();

        context.RequestServices = new ServiceCollection()
            .AddSingleton(dbContext)
            .BuildServiceProvider();

        _nextMock.Setup(_ => _(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(_ => _(It.IsAny<HttpContext>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_InvalidUserKey_ReturnsForbidden()
    {
        // Arrange
        var validGuid = Guid.NewGuid();
        var validUserKey = Convert.ToBase64String(validGuid.ToByteArray()).TrimEnd('=');
        var context = new DefaultHttpContext();
        context.Request.Headers["userkey"] = validUserKey;

        var dbContext = GetDbContext();
        var userKey = new UserKey { Key = validGuid.ToString() };
        dbContext.UserKeys.Add(userKey);
        dbContext.SaveChanges();

        dbContext.UserKeys.Remove(userKey);
        dbContext.SaveChanges();

        context.RequestServices = new ServiceCollection()
            .AddSingleton(dbContext)
            .BuildServiceProvider();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }
}
