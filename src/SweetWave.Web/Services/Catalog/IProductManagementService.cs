namespace SweetWave.Web.Services.Catalog;

/// <summary>
/// Admin access to the catalogue: creation, editing and visibility of products.
/// </summary>
public interface IProductManagementService
{
    /// <summary>
    /// Returns all products including the hidden ones.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All products of the catalogue.</returns>
    Task<IReadOnlyList<ProductSummary>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="input">Product data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created product.</returns>
    Task<ProductSummary> CreateAsync(ProductInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="input">New product data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated product.</returns>
    Task<ProductSummary> UpdateAsync(string id, ProductInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Hides or returns a product to the catalogue.
    /// </summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="isVisible">Target visibility.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the operation.</returns>
    Task SetVisibilityAsync(string id, bool isVisible, CancellationToken cancellationToken);
}