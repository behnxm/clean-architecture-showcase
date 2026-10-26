using CleanArchitectureShowcase.Domain.Common;
using CleanArchitectureShowcase.Domain.Entities;
using CleanArchitectureShowcase.Domain.Errors;
using CleanArchitectureShowcase.Domain.Events;
using Shouldly;
using Xunit;

namespace CleanArchitectureShowcase.UnitTests.Domain;

public sealed class ProductTests
{
    [Fact]
    public void Create_WithValidArguments_ReturnsSuccessAndProduct()
    {
        // Arrange
        const string name = "Ergonomic Mechanical Keyboard";
        const string description = "High performance mechanical keyboard.";
        const decimal price = 149.99m;
        const int stock = 25;

        // Act
        Result<Product> result = Product.Create(name, description, price, stock);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Product product = result.Value;
        product.Id.ShouldNotBe(Guid.Empty);
        product.Id.Version.ShouldBe(7); // UUIDv7
        product.Name.ShouldBe(name);
        product.Description.ShouldBe(description);
        product.Price.ShouldBe(price);
        product.Stock.ShouldBe(stock);

        product.DomainEvents.Count.ShouldBe(1);
        IDomainEvent domainEvent = product.DomainEvents.First();
        domainEvent.ShouldBeOfType<ProductCreatedDomainEvent>();
        var createdEvent = (ProductCreatedDomainEvent)domainEvent;
        createdEvent.ProductId.ShouldBe(product.Id);
        createdEvent.Name.ShouldBe(name);
        createdEvent.Price.ShouldBe(price);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceName_ReturnsEmptyNameError(string? invalidName)
    {
        // Act
        Result<Product> result = Product.Create(invalidName!, "Description", 10m, 5);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.EmptyName);
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ReturnsNameTooLongError()
    {
        // Arrange
        string longName = new('a', Product.NameMaxLength + 1);

        // Act
        Result<Product> result = Product.Create(longName, "Description", 10m, 5);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.NameTooLong);
    }

    [Fact]
    public void Create_WithDescriptionExceedingMaxLength_ReturnsDescriptionTooLongError()
    {
        // Arrange
        string longDescription = new('d', Product.DescriptionMaxLength + 1);

        // Act
        Result<Product> result = Product.Create("Valid Name", longDescription, 10m, 5);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.DescriptionTooLong);
    }

    [Fact]
    public void Create_WithNegativePrice_ReturnsInvalidPriceError()
    {
        // Act
        Result<Product> result = Product.Create("Valid Name", "Description", -0.01m, 5);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.InvalidPrice);
    }

    [Fact]
    public void Create_WithNegativeStock_ReturnsInvalidStockError()
    {
        // Act
        Result<Product> result = Product.Create("Valid Name", "Description", 10m, -1);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.InvalidStock);
    }

    [Fact]
    public void Update_WithValidArguments_ReturnsSuccessAndUpdatesFields()
    {
        // Arrange
        Product product = Product.Create("Original Name", "Original Desc", 50m, 10).Value;

        // Act
        Result result = product.Update("Updated Name", "Updated Desc", 75.50m);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Name.ShouldBe("Updated Name");
        product.Description.ShouldBe("Updated Desc");
        product.Price.ShouldBe(75.50m);
    }

    [Fact]
    public void Update_WithInvalidName_ReturnsError()
    {
        // Arrange
        Product product = Product.Create("Original Name", "Original Desc", 50m, 10).Value;

        // Act
        Result result = product.Update("   ", "Updated Desc", 75.50m);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.EmptyName);
    }

    [Fact]
    public void AdjustStock_WithPositiveDelta_IncreasesStock()
    {
        // Arrange
        Product product = Product.Create("Product", "Desc", 10m, 20).Value;

        // Act
        Result result = product.AdjustStock(15);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Stock.ShouldBe(35);
    }

    [Fact]
    public void AdjustStock_WithValidNegativeDelta_DecreasesStock()
    {
        // Arrange
        Product product = Product.Create("Product", "Desc", 10m, 20).Value;

        // Act
        Result result = product.AdjustStock(-10);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Stock.ShouldBe(10);
    }

    [Fact]
    public void AdjustStock_WhenDeltaDepletesBelowZero_ReturnsInsufficientStockError()
    {
        // Arrange
        Product product = Product.Create("Product", "Desc", 10m, 5).Value;

        // Act
        Result result = product.AdjustStock(-6);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.InsufficientStock);
        product.Stock.ShouldBe(5); // Unchanged
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllRecordedEvents()
    {
        // Arrange
        Product product = Product.Create("Product", "Desc", 10m, 5).Value;
        product.DomainEvents.Count.ShouldBe(1);

        // Act
        product.ClearDomainEvents();

        // Assert
        product.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Equality_BasedOnId()
    {
        // Arrange
        Product product1 = Product.Create("Product 1", "Desc 1", 10m, 5).Value;
        Product product2 = Product.Create("Product 2", "Desc 2", 20m, 10).Value;

        // Assert
        product1.Equals(product1).ShouldBeTrue();
        (product1 == product1).ShouldBeTrue();
        product1.Equals(product2).ShouldBeFalse();
        (product1 == product2).ShouldBeFalse();
        (product1 != product2).ShouldBeTrue();
    }
}
