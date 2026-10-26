using CleanArchitectureShowcase.Application.Common.Messaging;
using CleanArchitectureShowcase.Application.Common.Models;
using CleanArchitectureShowcase.Application.Features.Products.Commands.AdjustProductStock;
using CleanArchitectureShowcase.Application.Features.Products.Commands.CreateProduct;
using CleanArchitectureShowcase.Application.Features.Products.Commands.DeleteProduct;
using CleanArchitectureShowcase.Application.Features.Products.Commands.UpdateProduct;
using CleanArchitectureShowcase.Application.Features.Products.Contracts;
using CleanArchitectureShowcase.Application.Features.Products.Queries.GetProductById;
using CleanArchitectureShowcase.Application.Features.Products.Queries.GetProducts;
using CleanArchitectureShowcase.Domain.Common;
using CleanArchitectureShowcase.WebAPI.Contracts;
using CleanArchitectureShowcase.WebAPI.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureShowcase.WebAPI.Controllers;

/// <summary>
/// RESTful endpoint controller managing the Product Catalog.
/// In Clean Architecture, controllers act as thin entry gateways: they contain zero business logic
/// and zero try/catch blocks, purely translating HTTP payloads into application CQRS commands/queries
/// and mapping Result outcomes to standardized ProblemDetails responses.
/// </summary>
[ApiController]
[Route("api/v1/products")]
[Produces("application/json")]
public sealed class ProductsController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>
    /// Searches and retrieves paginated catalog products.
    /// </summary>
    /// <param name="search">Optional text search term filtering by name or description.</param>
    /// <param name="page">Page number (defaults to 1).</param>
    /// <param name="pageSize">Page size (defaults to 10, max 100).</param>
    /// <param name="sortBy">Sort column: name, price, stock, or createdAtUtc.</param>
    /// <param name="sortDirection">Sort direction: asc or desc.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of catalog products.</returns>
    [HttpGet(Name = "GetProducts")]
    [ProducesResponseType(typeof(PagedList<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery(search, page, pageSize, sortBy, sortDirection);
        Result<PagedList<ProductResponse>> result = await dispatcher.Query(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem(HttpContext);
    }

    /// <summary>
    /// Retrieves a single catalog product by its unique identifier.
    /// </summary>
    /// <param name="id">Product unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The detailed product view model.</returns>
    [HttpGet("{id:guid}", Name = "GetProductById")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductByIdQuery(id);
        Result<ProductResponse> result = await dispatcher.Query(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem(HttpContext);
    }

    /// <summary>
    /// Creates a new product in the catalog.
    /// </summary>
    /// <param name="request">Product creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created product identifier and location header.</returns>
    [HttpPost(Name = "CreateProduct")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.Stock);

        Result<Guid> result = await dispatcher.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtRoute("GetProductById", new { id = result.Value }, result.Value)
            : result.ToProblem(HttpContext);
    }

    /// <summary>
    /// Updates product attributes and price for an existing catalog product.
    /// </summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="request">Updated product details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HTTP 200 on success.</returns>
    [HttpPut("{id:guid}", Name = "UpdateProduct")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price);

        Result result = await dispatcher.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok()
            : result.ToProblem(HttpContext);
    }

    /// <summary>
    /// Adjusts catalog product inventory by a signed delta.
    /// </summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="request">Stock adjustment request carrying the integer delta.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HTTP 200 on success.</returns>
    [HttpPatch("{id:guid}/stock", Name = "AdjustProductStock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AdjustStock(
        Guid id,
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AdjustProductStockCommand(id, request.Delta);
        Result result = await dispatcher.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok()
            : result.ToProblem(HttpContext);
    }

    /// <summary>
    /// Deletes a catalog product by its unique identifier.
    /// </summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HTTP 204 on success.</returns>
    [HttpDelete("{id:guid}", Name = "DeleteProduct")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteProductCommand(id);
        Result result = await dispatcher.Send(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem(HttpContext);
    }
}
