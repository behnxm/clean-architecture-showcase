using System.Net;
using System.Text.Json.Nodes;
using CleanArchitectureShowcase.IntegrationTests.Common;
using Shouldly;
using Xunit;

namespace CleanArchitectureShowcase.IntegrationTests.Endpoints;

public sealed class HealthAndOpenApiTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task LivenessHealthCheck_Returns200Ok()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReadinessHealthCheck_Returns200Ok()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OpenApiDocument_GeneratesSuccessfully_WithUniqueOperationIds()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/openapi/v1.json");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        string content = await response.Content.ReadAsStringAsync();
        var doc = JsonNode.Parse(content);
        doc.ShouldNotBeNull();

        var paths = doc["paths"] as JsonObject;
        paths.ShouldNotBeNull();
        paths.Count.ShouldBeGreaterThan(0);

        var operationIds = new List<string>();

        foreach (KeyValuePair<string, JsonNode?> path in paths)
        {
            if (path.Value is not JsonObject methods)
            {
                continue;
            }

            foreach (KeyValuePair<string, JsonNode?> method in methods)
            {
                if (method.Value is JsonObject operation && operation.TryGetPropertyValue("operationId", out JsonNode? opIdNode))
                {
                    string? opId = opIdNode?.GetValue<string>();
                    opId.ShouldNotBeNullOrWhiteSpace();
                    operationIds.Add(opId);
                }
            }
        }

        operationIds.ShouldNotBeEmpty();

        // Verify all operationIds are strictly unique
        var distinctOperationIds = operationIds.Distinct().ToList();
        operationIds.Count.ShouldBe(distinctOperationIds.Count,
            $"Found duplicate operationIds in OpenAPI spec: {string.Join(", ", operationIds.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key))}");

        // Expected endpoint operation IDs
        distinctOperationIds.ShouldContain("GetProducts");
        distinctOperationIds.ShouldContain("GetProductById");
        distinctOperationIds.ShouldContain("CreateProduct");
        distinctOperationIds.ShouldContain("UpdateProduct");
        distinctOperationIds.ShouldContain("AdjustProductStock");
        distinctOperationIds.ShouldContain("DeleteProduct");
    }

    [Fact]
    public async Task ScalarApiReference_ReturnsSuccessStatusCode()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/scalar");

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue($"Failed with status {response.StatusCode}");
        string content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("scalar");
    }

    [Fact]
    public async Task ScalarApiReference_V1Route_ReturnsSuccessStatusCode()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/scalar/v1");

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue($"Failed with status {response.StatusCode}");
        string content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("scalar");
    }
}
