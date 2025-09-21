using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController(ProductService productService) : ControllerBase
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
    public async Task<IActionResult> GetProductsByCategory(int categoryId)
    {
        try
        {
            var products = await productService.SearchProductsAsync(null);
            var filteredProducts = products.Where(p => p.CategoryId == categoryId)
                .ToList();
                
            if (filteredProducts.Count == 0)
                return NotFound($"Не найдено товаров с ID категории {categoryId}");

            return Ok(filteredProducts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка сервера: {ex.Message}");
        }
    }
}