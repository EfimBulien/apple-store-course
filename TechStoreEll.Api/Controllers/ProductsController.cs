using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.DTOs;
using TechStoreEll.Core.Infrastructure.Data;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ProductService productService, AppDbContext context) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string name)
    {
        try
        {
            var productVariants = await productService
                .SearchProductVariantsAsync(name.ToLower());
                
            if (productVariants.Count == 0)
                return NotFound("Не найдено таких товаров");

            return Ok(productVariants);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка сервера: {ex.Message}");
        }
    }

    [HttpGet]
    [AuthorizeRole("Admin")] // админ
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            var productVariants = await productService.GetAllProductVariantsAsync();
                
            if (productVariants.Count == 0)
                return NotFound("Не найдено");

            return Ok(productVariants);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка сервера: {ex.Message}");
        }
    }
    
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetProductById(int id)
    {
        try
        {
            var product = await productService.GetProductByIdAsync(id);
                
            if (product == null)
                return NotFound($"Товары с ID={id} не найдено");

            return Ok(product);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка сервера: {ex.Message}");
        }
    }
    
    [HttpGet("category/{categoryId:int}")]
    [AuthorizeRole("Admin", "Customer")]
    public async Task<IActionResult> GetProductsByCategory(int categoryId)
    {
        try
        {
            var products = await productService.SearchProductsAsync(null);
            var filteredProducts = products.Where(p => p.CategoryId == categoryId)
                .ToList();
                
            if (filteredProducts.Count == 0)
            {
                return NotFound($"Не найдено товаров с ID категории {categoryId}");
            }

            return Ok(filteredProducts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка сервера: {ex.Message}");
        }
    }
    
    [HttpGet("filter")]
    public async Task<IActionResult> FilterProducts(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? ram,
        [FromQuery] int? storage)
    {
        try 
        {
            var query = context.ProductVariants
                .Include(v => v.Product)
                .Where(v => v.Product.Active);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(v =>
                    EF.Functions.ToTsVector("russian", 
                            v.Product.Name + " " + v.Product.Description + " " + v.Product.Sku + " " + v.VariantCode + " " + v.Color)
                        .Matches(EF.Functions.WebSearchToTsQuery("russian", search))
                );
            }

            if (categoryId.HasValue)
                query = query.Where(v => v.Product.CategoryId == categoryId);

            if (minPrice.HasValue)
                query = query.Where(v => v.Price >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(v => v.Price <= maxPrice);

            if (ram.HasValue)
                query = query.Where(v => v.Ram == ram);

            if (storage.HasValue)
                query = query.Where(v => v.StorageGb == storage);

            var result = await query.Select(v => new ProductFullDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                VariantCode = v.VariantCode,
                Price = v.Price,
                Color = v.Color,
                StorageGb = v.StorageGb,
                Ram = v.Ram,
                ProductSku = v.Product.Sku!,
                ProductName = v.Product.Name!,
                CategoryId = v.Product.CategoryId,
                ProductDescription = v.Product.Description,
                ProductActive = v.Product.Active,
                ProductCreatedAt = v.Product.CreatedAt,
                ProductAvgRating = v.Product.AvgRating,
                ProductReviewsCount = v.Product.ReviewsCount
            }).ToListAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка сервера: {ex.Message}");
        }
    }
}