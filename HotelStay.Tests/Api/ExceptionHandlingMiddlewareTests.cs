using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using HotelStay.Api.Middleware;
using HotelStay.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HotelStay.Tests.Api;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
    private readonly ExceptionHandlingMiddleware _middleware;

    public ExceptionHandlingMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNext()
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
    public async Task InvokeAsync_WhenDomainValidationException_ShouldReturn400()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/test";
        
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new DomainValidationException("Invalid data"));

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(400, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problem = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("Validation failed", problem.Title);
        Assert.Equal("Invalid data", problem.Detail);
    }

    [Fact]
    public async Task InvokeAsync_WhenDocumentMismatchException_ShouldReturn422()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/test";
        
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new DocumentMismatchException("Document type mismatch"));

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(422, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problem = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(problem);
        Assert.Equal(422, problem.Status);
        Assert.Equal("Document mismatch", problem.Title);
        Assert.Equal("Document type mismatch", problem.Detail);
    }

    [Fact]
    public async Task InvokeAsync_WhenReservationNotFoundException_ShouldReturn404()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/test";
        
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new ReservationNotFoundException("REF-12345678"));

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(404, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problem = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal("Reservation not found", problem.Title);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledException_ShouldReturn500()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/test";
        
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(500, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problem = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal("Internal server error", problem.Title);
        Assert.Equal("An unexpected error occurred.", problem.Detail);
    }

    [Fact]
    public async Task InvokeAsync_WhenDomainValidationException_ShouldLogWarning()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new DomainValidationException("Invalid data"));

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Domain validation failed")),
                It.IsAny<DomainValidationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledException_ShouldLogError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unhandled exception")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldIncludeRequestPathInProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/hotels/reserve";
        
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new DomainValidationException("Invalid data"));

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problem = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(problem);
        Assert.Equal("/hotels/reserve", problem.Instance);
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseHasStarted_ShouldNotWriteResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var stream = new MemoryStream();
        context.Response.Body = stream;
        
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .Callback(() =>
            {
                // Simulate response started by writing to body
                context.Response.Body.Write(new byte[] { 1 });
                context.Response.Body.Flush();
            })
            .ThrowsAsync(new DomainValidationException("Invalid data"));

        // Act & Assert - Should not throw
        await _middleware.InvokeAsync(context);
    }

    [Fact]
    public async Task InvokeAsync_WhenReservationNotFoundException_ShouldLogInformation()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new ReservationNotFoundException("REF-12345678"));

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Reservation not found")),
                It.IsAny<ReservationNotFoundException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
