using CleanArchitectureShowcase.Domain.Common;
using Shouldly;
using Xunit;

namespace CleanArchitectureShowcase.UnitTests.Domain;

public sealed class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        // Act
        Result result = Result.Success();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBe(Error.None);
    }

    [Fact]
    public void Failure_CreatesFailedResultWithError()
    {
        // Arrange
        Error error = Error.NotFound("Item.NotFound", "The requested item does not exist.");

        // Act
        Result result = Result.Failure(error);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
    }

    [Fact]
    public void SuccessWithValue_ReturnsValue()
    {
        // Act
        Result<string> result = Result.Success("hello world");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("hello world");
    }

    [Fact]
    public void FailureWithValue_AccessingValueThrowsInvalidOperationException()
    {
        // Arrange
        Error error = Error.Failure("System.Error", "A failure occurred.");
        Result<int> result = Result.Failure<int>(error);

        // Assert
        result.IsFailure.ShouldBeTrue();
        Should.Throw<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromValue_ReturnsSuccessResult()
    {
        // Act
        Result<string> result = "test value";

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("test value");
    }

    [Fact]
    public void ImplicitConversion_FromNullValue_ReturnsFailureResult()
    {
        // Act
        string? nullString = null;
        Result<string> result = nullString;

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Error.NullValue);
    }

    [Fact]
    public void ImplicitConversion_FromError_ReturnsFailureResult()
    {
        // Arrange
        Error error = Error.Validation("Input.Invalid", "Invalid input provided.");

        // Act
        Result result = error;
        Result<int> typedResult = error;

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
        typedResult.IsFailure.ShouldBeTrue();
        typedResult.Error.ShouldBe(error);
    }

    [Fact]
    public void ValidationError_EncapsulatesMultipleErrors()
    {
        // Arrange
        Error error1 = Error.Validation("Field1", "Error 1");
        Error error2 = Error.Validation("Field2", "Error 2");

        // Act
        var validationError = new ValidationError([error1, error2]);

        // Assert
        validationError.Type.ShouldBe(ErrorType.Validation);
        validationError.Errors.Count.ShouldBe(2);
        validationError.Errors.ShouldContain(error1);
        validationError.Errors.ShouldContain(error2);
    }
}
