namespace CleanArchitectureShowcase.WebAPI.Contracts;

/// <summary>
/// HTTP request body for adjusting catalog product inventory stock.
/// </summary>
/// <param name="Delta">Signed inventory delta (positive to restock, negative to reduce).</param>
public sealed record AdjustStockRequest(int Delta);
