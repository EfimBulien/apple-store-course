
using TechStoreEll.Core.DTOs;
using TechStoreEll.Core.Entities;

namespace TechStoreEll.Core.Interfaces;

public interface IProductService
{
    Task<Product> CreateProductAsync(Product product);
    Task UpdateProductAsync(int id, Product product);
    Task DeleteProductAsync(int id);
    Task<List<ProductFullDto>> GetAllActiveProductVariantsAsync();
    Task<List<ProductFullDto>> SearchProductVariantsAsync(string searchTerm);
    Task<List<ProductDto>> SearchProductsAsync(string? searchTerm);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<List<ProductFullDto>> FullTextSearchAsync(string searchTerm);
    Task<List<ProductFullDto>> FilterProductsAsync(
        string? search,
        int? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        int? ram,
        int? storage);
}