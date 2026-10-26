using System.Net;
using System.Net.Http.Json;
using CleanArchitectureShowcase.Application.Common.Models;
using CleanArchitectureShowcase.Application.Features.Products.Contracts;
using CleanArchitectureShowcase.IntegrationTests.Common;
using CleanArchitectureShowcase.WebAPI.Contracts;
using Shouldly;
using Xunit;

namespace CleanArchitectureShowcase.IntegrationTests.Endpoints;

public sealed class ProductQueryTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task SeedQueryTestDataAsync()
    {
        CreateProductRequest[] items =
        [
            new("Budget Mouse", "Entry level wired optical mouse", 9.99m, 100),
            new("Mechanical Keyboard", "High end RGB gaming keyboard", 129.50m, 30),
            new("Studio Monitor Speakers", "Audiophile near-field studio monitors", 299.00m, 15),
            new("USB Flash Drive", "Fast USB 3.2 128GB flash storage", 19.95m, 80),
            new("Ergonomic Desk Chair", "Adjustable lumbar support office chair", 349.99m, 10)
        ];

        foreach (CreateProductRequest item in items)
        {
            await _client.PostAsJsonAsync("api/v1/products", item);
        }
    }

    [Fact]
    public async Task GetProducts_WithPagination_ReturnsCorrectPageAndMetadata()
    {
        // Arrange
        await SeedQueryTestDataAsync();

        // Act
        HttpResponseMessage response = await _client.GetAsync("api/v1/products?page=1&pageSize=2");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        PagedList<ProductResponse>? paged = await response.Content.ReadFromJsonAsync<PagedList<ProductResponse>>();

        paged.ShouldNotBeNull();
        paged.Items.Count.ShouldBe(2);
        paged.Page.ShouldBe(1);
        paged.PageSize.ShouldBe(2);
        paged.TotalCount.ShouldBeGreaterThanOrEqualTo(5);
        paged.HasNext.ShouldBeTrue();
        paged.HasPrevious.ShouldBeFalse();
    }

    [Fact]
    public async Task GetProducts_WithSearchTerm_FiltersCorrectly()
    {
        // Arrange
        await SeedQueryTestDataAsync();

        // Act
        HttpResponseMessage response = await _client.GetAsync("api/v1/products?search=Keyboard");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        PagedList<ProductResponse>? paged = await response.Content.ReadFromJsonAsync<PagedList<ProductResponse>>();

        paged.ShouldNotBeNull();
        paged.Items.ShouldNotBeEmpty();
        paged.Items.ShouldAllBe(p => p.Name.Contains("Keyboard", StringComparison.OrdinalIgnoreCase) ||
                                     p.Description.Contains("Keyboard", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetProducts_SortByPriceAscending_VerifiesSqliteDecimalToDoubleOrdering()
    {
        // Arrange: Seed test products with diverse decimal values
        // In raw SQLite, string comparison would sort "129.50" before "19.95" and "9.99"!
        // Our EF Core decimal-to-double value converter ensures true numeric ordering.
        await SeedQueryTestDataAsync();

        // Act
        HttpResponseMessage response = await _client.GetAsync("api/v1/products?sortBy=price&sortDirection=asc&pageSize=50");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        PagedList<ProductResponse>? paged = await response.Content.ReadFromJsonAsync<PagedList<ProductResponse>>();

        paged.ShouldNotBeNull();
        paged.Items.Count.ShouldBeGreaterThanOrEqualTo(5);

        for (int i = 0; i < paged.Items.Count - 1; i++)
        {
            paged.Items[i].Price.ShouldBeLessThanOrEqualTo(paged.Items[i + 1].Price);
        }
    }

    [Fact]
    public async Task GetProducts_SortByPriceDescending_SortsNumericallyDescending()
    {
        // Arrange
        await SeedQueryTestDataAsync();

        // Act
        HttpResponseMessage response = await _client.GetAsync("api/v1/products?sortBy=price&sortDirection=desc&pageSize=50");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        PagedList<ProductResponse>? paged = await response.Content.ReadFromJsonAsync<PagedList<ProductResponse>>();

        paged.ShouldNotBeNull();
        paged.Items.Count.ShouldBeGreaterThanOrEqualTo(5);

        for (int i = 0; i < paged.Items.Count - 1; i++)
        {
            paged.Items[i].Price.ShouldBeGreaterThanOrEqualTo(paged.Items[i + 1].Price);
        }
    }
}
