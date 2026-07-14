using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HotelStay.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HotelStay.Tests.Api;

public class CorrelationIdMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<CorrelationIdMiddleware>> _loggerMock;
    private readonly CorrelationIdMiddleware _middleware;

    public CorrelationIdMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
        _middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WhenNoCorrelationIdProvided_ShouldGenerateNew()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("X-Correlation-Id"));
        var correlationId = context.Response.Headers["X-Correlation-Id"].ToString();
        Assert.NotEmpty(correlationId);
        Assert.Equal(32, correlationId.Length); // Guid without hyphens
    }

    [Fact]
    public async Task InvokeAsync_WhenCorrelationIdProvided_ShouldUseExisting()
    {
        // Arrange
        var existingCorrelationId = "abc123def456";
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-Id"] = existingCorrelationId;
        context.Response.Body = new MemoryStream();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("X-Correlation-Id"));
        var correlationId = context.Response.Headers["X-Correlation-Id"].ToString();
        Assert.Equal(existingCorrelationId, correlationId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var nextCalled = false;
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .Callback(() => nextCalled = true)
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenEmptyCorrelationIdProvided_ShouldGenerateNew()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-Id"] = "";
        context.Response.Body = new MemoryStream();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Response.Headers["X-Correlation-Id"].ToString();
        Assert.NotEmpty(correlationId);
        Assert.Equal(32, correlationId.Length);
    }

    [Fact]
    public async Task InvokeAsync_WhenWhitespaceCorrelationIdProvided_ShouldGenerateNew()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-Id"] = "   ";
        context.Response.Body = new MemoryStream();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Response.Headers["X-Correlation-Id"].ToString();
        Assert.NotEmpty(correlationId);
        Assert.Equal(32, correlationId.Length);
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddCorrelationIdToResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("X-Correlation-Id"));
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleRequests_ShouldGenerateUniqueIds()
    {
        // Arrange
        var context1 = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        var context2 = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        var context3 = new DefaultHttpContext { Response = { Body = new MemoryStream() } };

        // Act
        await _middleware.InvokeAsync(context1);
        await _middleware.InvokeAsync(context2);
        await _middleware.InvokeAsync(context3);

        // Assert
        var id1 = context1.Response.Headers["X-Correlation-Id"].ToString();
        var id2 = context2.Response.Headers["X-Correlation-Id"].ToString();
        var id3 = context3.Response.Headers["X-Correlation-Id"].ToString();

        Assert.NotEqual(id1, id2);
        Assert.NotEqual(id2, id3);
        Assert.NotEqual(id1, id3);
    }

    [Fact]
    public async Task InvokeAsync_GeneratedCorrelationId_ShouldBeValidGuidFormat()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Response.Headers["X-Correlation-Id"].ToString();
        Assert.Matches("^[a-fA-F0-9]{32}$", correlationId);
    }

    [Fact]
    public void HeaderName_ShouldBeCorrect()
    {
        // Assert
        Assert.Equal("X-Correlation-Id", CorrelationIdMiddleware.HeaderName);
    }
}
