using EcoFleet.Application.Behaviors;
using EcoFleet.Application.Exceptions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    private readonly ILogger<ValidationBehavior<TestRequest, TestResponse>> _logger;

    public ValidationBehaviorTests()
    {
        _logger = Substitute.For<ILogger<ValidationBehavior<TestRequest, TestResponse>>>();
    }

    public record TestRequest(string Name) : IRequest<TestResponse>;
    public record TestResponse(string Result);

    [Fact]
    public async Task Handle_WhenNoValidators_ShouldCallNext()
    {
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _logger);
        var request = new TestRequest("Test");
        var expectedResponse = new TestResponse("OK");
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();
        next().Returns(expectedResponse);

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Should().Be(expectedResponse);
        await next.Received(1)();
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_ShouldCallNext()
    {
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var validators = new[] { validator };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _logger);
        var request = new TestRequest("Valid");
        var expectedResponse = new TestResponse("OK");
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();
        next().Returns(expectedResponse);

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Should().Be(expectedResponse);
        await next.Received(1)();
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldThrowValidationErrorException()
    {
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required.")
        };
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));
        var validators = new[] { validator };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _logger);
        var request = new TestRequest("");
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();

        var act = () => behavior.Handle(request, next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationErrorException>();
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldNotCallNext()
    {
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required.")
        };
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));
        var validators = new[] { validator };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _logger);
        var request = new TestRequest("");
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();

        try { await behavior.Handle(request, next, CancellationToken.None); } catch { }

        await next.DidNotReceive()();
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ExceptionShouldContainErrors()
    {
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required."),
            new("Name", "Name must be at least 3 characters."),
            new("Age", "Age must be positive.")
        };
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));
        var validators = new[] { validator };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _logger);
        var request = new TestRequest("");
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();

        var act = () => behavior.Handle(request, next, CancellationToken.None);

        var exception = (await act.Should().ThrowAsync<ValidationErrorException>()).Which;
        exception.Errors.Should().ContainKey("Name");
        exception.Errors["Name"].Should().HaveCount(2);
        exception.Errors.Should().ContainKey("Age");
        exception.Errors["Age"].Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldAggregateFailures()
    {
        var validator1 = Substitute.For<IValidator<TestRequest>>();
        validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Field1", "Error1") }));
        var validator2 = Substitute.For<IValidator<TestRequest>>();
        validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Field2", "Error2") }));
        var validators = new[] { validator1, validator2 };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _logger);
        var request = new TestRequest("");
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();

        var act = () => behavior.Handle(request, next, CancellationToken.None);

        var exception = (await act.Should().ThrowAsync<ValidationErrorException>()).Which;
        exception.Errors.Should().HaveCount(2);
        exception.Errors.Should().ContainKey("Field1");
        exception.Errors.Should().ContainKey("Field2");
    }
}
