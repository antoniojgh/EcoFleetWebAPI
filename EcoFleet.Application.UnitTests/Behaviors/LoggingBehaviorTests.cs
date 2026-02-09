using EcoFleet.Application.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.Behaviors;

public class LoggingBehaviorTests
{
    private readonly ILogger<LoggingBehavior<TestRequest, TestResponse>> _logger;

    public LoggingBehaviorTests()
    {
        _logger = Substitute.For<ILogger<LoggingBehavior<TestRequest, TestResponse>>>();
    }

    public record TestRequest(string Name) : IRequest<TestResponse>;
    public record TestResponse(string Result);

    [Fact]
    public async Task Handle_ShouldCallNextAndReturnResponse()
    {
        var behavior = new LoggingBehavior<TestRequest, TestResponse>(_logger);
        var request = new TestRequest("Test");
        var expectedResponse = new TestResponse("OK");
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();
        next().Returns(expectedResponse);

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Should().Be(expectedResponse);
        await next.Received(1)();
    }

    [Fact]
    public async Task Handle_WhenNextThrows_ShouldPropagateException()
    {
        var behavior = new LoggingBehavior<TestRequest, TestResponse>(_logger);
        var request = new TestRequest("Test");
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();
        next().Returns<TestResponse>(_ => throw new InvalidOperationException("Handler failed"));

        var act = () => behavior.Handle(request, next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Handler failed");
    }

    [Fact]
    public async Task Handle_ShouldLogRequestName()
    {
        var behavior = new LoggingBehavior<TestRequest, TestResponse>(_logger);
        var request = new TestRequest("Test");
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();
        next().Returns(new TestResponse("OK"));

        await behavior.Handle(request, next, CancellationToken.None);

        _logger.ReceivedWithAnyArgs(2).Log(
            default,
            default,
            default!,
            default,
            default!);
    }
}
