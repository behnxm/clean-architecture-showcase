using CleanArchitectureShowcase.Application.Features.Products.Commands.AdjustProductStock;
using CleanArchitectureShowcase.Application.Features.Products.Commands.CreateProduct;
using CleanArchitectureShowcase.Application.Features.Products.Commands.UpdateProduct;
using CleanArchitectureShowcase.Application.Features.Products.Queries.GetProducts;
using FluentValidation.Results;
using Shouldly;
using Xunit;

namespace CleanArchitectureShowcase.UnitTests.Application;

public sealed class ValidatorTests
{
    [Fact]
    public async Task CreateProductValidator_WithValidInput_PassesValidation()
    {
        // Arrange
        var validator = new CreateProductCommandValidator();
        var command = new CreateProductCommand("Valid Name", "Description", 19.99m, 10);

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateProductValidator_WithEmptyNameAndNegativePrice_FailsValidation()
    {
        // Arrange
        var validator = new CreateProductCommandValidator();
        var command = new CreateProductCommand("", "Description", -5m, -2);

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Name));
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Price));
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Stock));
    }

    [Fact]
    public async Task UpdateProductValidator_WithEmptyId_FailsValidation()
    {
        // Arrange
        var validator = new UpdateProductCommandValidator();
        var command = new UpdateProductCommand(Guid.Empty, "Name", "Desc", 10m);

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Id));
    }

    [Fact]
    public async Task AdjustStockValidator_WithZeroDelta_FailsValidation()
    {
        // Arrange
        var validator = new AdjustProductStockCommandValidator();
        var command = new AdjustProductStockCommand(Guid.NewGuid(), 0);

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Delta));
    }

    [Theory]
    [InlineData("name", "asc", true)]
    [InlineData("price", "desc", true)]
    [InlineData("stock", "ASC", true)]
    [InlineData("createdAtUtc", "DESC", true)]
    [InlineData("sql_injection", "asc", false)]
    [InlineData("name", "invalid_dir", false)]
    public async Task GetProductsValidator_SortValidation(string sortBy, string sortDirection, bool expectedValid)
    {
        // Arrange
        var validator = new GetProductsQueryValidator();
        var query = new GetProductsQuery(null, 1, 10, sortBy, sortDirection);

        // Act
        ValidationResult result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.ShouldBe(expectedValid);
    }

    [Theory]
    [InlineData(0, 10, false)] // Page < 1
    [InlineData(1, 0, false)]  // PageSize < 1
    [InlineData(1, 101, false)] // PageSize > 100
    [InlineData(1, 50, true)]
    public async Task GetProductsValidator_PaginationValidation(int page, int pageSize, bool expectedValid)
    {
        // Arrange
        var validator = new GetProductsQueryValidator();
        var query = new GetProductsQuery(null, page, pageSize, "name", "asc");

        // Act
        ValidationResult result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.ShouldBe(expectedValid);
    }
}
