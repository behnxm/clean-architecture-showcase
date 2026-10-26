using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using CleanArchitectureShowcase.Application.Features.Products.Contracts;
using CleanArchitectureShowcase.IntegrationTests.Common;
using CleanArchitectureShowcase.WebAPI.Contracts;
using Shouldly;
using Xunit;

namespace CleanArchitectureShowcase.IntegrationTests.Endpoints;

public sealed class ProductsCrudTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task FullCrudLifecycle_WorksEndToEnd()
    {
        // 1. CREATE
        var createRequest = new CreateProductRequest("Lifecycle Product", "A comprehensive lifecycle test.", 199.99m, 50);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("api/v1/products", createRequest);

        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        createResponse.Headers.Location.ShouldNotBeNull();

        Guid productId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        productId.ShouldNotBe(Guid.Empty);

        // 2. READ BY ID
        HttpResponseMessage getResponse = await _client.GetAsync($"api/v1/products/{productId}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        ProductResponse? product = await getResponse.Content.ReadFromJsonAsync<ProductResponse>();
        product.ShouldNotBeNull();
        product.Id.ShouldBe(productId);
        product.Name.ShouldBe("Lifecycle Product");
        product.Price.ShouldBe(199.99m);
        product.Stock.ShouldBe(50);

        // 3. UPDATE
        var updateRequest = new UpdateProductRequest("Updated Lifecycle Product", "Updated description.", 249.99m);
        HttpResponseMessage updateResponse = await _client.PutAsJsonAsync($"api/v1/products/{productId}", updateRequest);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        ProductResponse? updatedProduct = await _client.GetFromJsonAsync<ProductResponse>($"api/v1/products/{productId}");
        updatedProduct.ShouldNotBeNull();
        updatedProduct.Name.ShouldBe("Updated Lifecycle Product");
        updatedProduct.Price.ShouldBe(249.99m);

        // 4. ADJUST STOCK (Positive Delta)
        var stockRequest = new AdjustStockRequest(25);
        HttpResponseMessage stockResponse = await _client.PatchAsJsonAsync($"api/v1/products/{productId}/stock", stockRequest);
        stockResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        ProductResponse? stockedProduct = await _client.GetFromJsonAsync<ProductResponse>($"api/v1/products/{productId}");
        stockedProduct.ShouldNotBeNull();
        stockedProduct.Stock.ShouldBe(75); // 50 + 25

        // 5. DELETE
        HttpResponseMessage deleteResponse = await _client.DeleteAsync($"api/v1/products/{productId}");
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 6. VERIFY NOT FOUND
        HttpResponseMessage verifyResponse = await _client.GetAsync($"api/v1/products/{productId}");
        verifyResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidPayload_Returns400ProblemDetailsWithErrorsDictionary()
    {
        // Arrange: invalid name (empty) and negative price
        var invalidRequest = new CreateProductRequest("", "Description", -10m, -5);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("api/v1/products", invalidRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        string content = await response.Content.ReadAsStringAsync();
        JsonNode? json = JsonNode.Parse(content);
        json.ShouldNotBeNull();

        json["status"]?.GetValue<int>().ShouldBe(400);
        json["title"]?.GetValue<string>().ShouldBe("Bad Request");
        json["type"]?.GetValue<string>().ShouldNotBeNull();
        json["code"]?.GetValue<string>().ShouldBe("Validation.General");
        json["traceId"]?.GetValue<string>().ShouldNotBeNull();

        JsonNode? errors = json["errors"];
        errors.ShouldNotBeNull();
        errors["Name"].ShouldNotBeNull();
        errors["Price"].ShouldNotBeNull();
        errors["Stock"].ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateProduct_WhenNameAlreadyExists_Returns409ConflictProblemDetails()
    {
        // Arrange
        var request1 = new CreateProductRequest("Duplicate Candidate", "Desc", 50m, 10);
        HttpResponseMessage response1 = await _client.PostAsJsonAsync("api/v1/products", request1);
        response1.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Act: try to create another product with same name
        var request2 = new CreateProductRequest("Duplicate Candidate", "Other Desc", 60m, 15);
        HttpResponseMessage response2 = await _client.PostAsJsonAsync("api/v1/products", request2);

        // Assert
        response2.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        string content = await response2.Content.ReadAsStringAsync();
        JsonNode? json = JsonNode.Parse(content);
        json.ShouldNotBeNull();

        json["status"]?.GetValue<int>().ShouldBe(409);
        json["title"]?.GetValue<string>().ShouldBe("Conflict");
        json["code"]?.GetValue<string>().ShouldBe("Product.DuplicateName");
    }

    [Fact]
    public async Task GetProductById_WhenNotFound_Returns404ProblemDetails()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"api/v1/products/{nonExistentId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        string content = await response.Content.ReadAsStringAsync();
        JsonNode? json = JsonNode.Parse(content);
        json.ShouldNotBeNull();

        json["status"]?.GetValue<int>().ShouldBe(404);
        json["title"]?.GetValue<string>().ShouldBe("Not Found");
        json["code"]?.GetValue<string>().ShouldBe("Product.NotFound");
    }

    [Fact]
    public async Task AdjustStock_WhenDepletingBelowZero_Returns400InsufficientStock()
    {
        // Arrange
        var createRequest = new CreateProductRequest("Limited Stock Item", "Desc", 20m, 5);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("api/v1/products", createRequest);
        Guid productId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act: try to reduce by 10 when stock is 5
        var stockRequest = new AdjustStockRequest(-10);
        HttpResponseMessage response = await _client.PatchAsJsonAsync($"api/v1/products/{productId}/stock", stockRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        string content = await response.Content.ReadAsStringAsync();
        JsonNode? json = JsonNode.Parse(content);
        json.ShouldNotBeNull();

        json["status"]?.GetValue<int>().ShouldBe(400);
        json["code"]?.GetValue<string>().ShouldBe("Product.InsufficientStock");
    }
}
