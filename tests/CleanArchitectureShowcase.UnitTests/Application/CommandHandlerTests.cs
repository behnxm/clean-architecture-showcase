using CleanArchitectureShowcase.Application.Features.Products.Commands.AdjustProductStock;
using CleanArchitectureShowcase.Application.Features.Products.Commands.CreateProduct;
using CleanArchitectureShowcase.Application.Features.Products.Commands.DeleteProduct;
using CleanArchitectureShowcase.Application.Features.Products.Commands.UpdateProduct;
using CleanArchitectureShowcase.Domain.Abstractions;
using CleanArchitectureShowcase.Domain.Common;
using CleanArchitectureShowcase.Domain.Entities;
using CleanArchitectureShowcase.Domain.Errors;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CleanArchitectureShowcase.UnitTests.Application;

public sealed class CommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateProduct_WhenNameIsUnique_SavesAndReturnsGuid()
    {
        // Arrange
        var handler = new CreateProductCommandHandler(_productRepository, _unitOfWork);
        var command = new CreateProductCommand("Unique Product", "Description", 99.99m, 10);

        _productRepository.ExistsByNameAsync(command.Name, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);

        await _productRepository.Received(1).AddAsync(
            Arg.Is<Product>(p => p.Name == command.Name && p.Price == command.Price),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateProduct_WhenNameAlreadyExists_ReturnsDuplicateNameConflict()
    {
        // Arrange
        var handler = new CreateProductCommandHandler(_productRepository, _unitOfWork);
        var command = new CreateProductCommand("Existing Product", "Description", 99.99m, 10);

        _productRepository.ExistsByNameAsync(command.Name, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.DuplicateName);

        await _productRepository.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateProduct_WhenProductExistsAndNameIsUnique_UpdatesAndSaves()
    {
        // Arrange
        var handler = new UpdateProductCommandHandler(_productRepository, _unitOfWork);
        Product product = Product.Create("Old Name", "Old Desc", 20m, 5).Value;
        var command = new UpdateProductCommand(product.Id, "New Name", "New Desc", 35m);

        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        _productRepository.ExistsByNameAsync(command.Name, command.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Name.ShouldBe("New Name");
        product.Price.ShouldBe(35m);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateProduct_WhenProductNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var handler = new UpdateProductCommandHandler(_productRepository, _unitOfWork);
        var missingId = Guid.NewGuid();
        var command = new UpdateProductCommand(missingId, "New Name", "New Desc", 35m);

        _productRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.NotFound(missingId));

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateProduct_WhenNewNameConflictsWithAnotherProduct_ReturnsDuplicateNameError()
    {
        // Arrange
        var handler = new UpdateProductCommandHandler(_productRepository, _unitOfWork);
        Product product = Product.Create("Product A", "Desc", 20m, 5).Value;
        var command = new UpdateProductCommand(product.Id, "Product B", "Desc", 35m);

        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        _productRepository.ExistsByNameAsync(command.Name, command.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.DuplicateName);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteProduct_WhenProductExists_RemovesAndSaves()
    {
        // Arrange
        var handler = new DeleteProductCommandHandler(_productRepository, _unitOfWork);
        Product product = Product.Create("To Delete", "Desc", 10m, 5).Value;
        var command = new DeleteProductCommand(product.Id);

        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _productRepository.Received(1).Remove(product);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteProduct_WhenProductNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var handler = new DeleteProductCommandHandler(_productRepository, _unitOfWork);
        var missingId = Guid.NewGuid();
        var command = new DeleteProductCommand(missingId);

        _productRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.NotFound(missingId));
        _productRepository.DidNotReceive().Remove(Arg.Any<Product>());
    }

    [Fact]
    public async Task AdjustStock_WhenStockSufficient_UpdatesAndSaves()
    {
        // Arrange
        var handler = new AdjustProductStockCommandHandler(_productRepository, _unitOfWork);
        Product product = Product.Create("Stock Item", "Desc", 15m, 20).Value;
        var command = new AdjustProductStockCommand(product.Id, 10);

        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Stock.ShouldBe(30);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdjustStock_WhenDeltaCausesNegativeStock_ReturnsInsufficientStockError()
    {
        // Arrange
        var handler = new AdjustProductStockCommandHandler(_productRepository, _unitOfWork);
        Product product = Product.Create("Stock Item", "Desc", 15m, 5).Value;
        var command = new AdjustProductStockCommand(product.Id, -10);

        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.InsufficientStock);
        product.Stock.ShouldBe(5);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
