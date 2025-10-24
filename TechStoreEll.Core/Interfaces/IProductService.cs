
using TechStoreEll.Core.DTOs;

namespace TechStoreEll.Core.Interfaces;

public interface IProductService
{
    Task<List<ProductFullDto>> GetAllProductVariantsAsync();
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