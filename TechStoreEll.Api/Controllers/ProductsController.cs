using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string name)
    {
        try
        {
            var productVariants = await productService.SearchProductVariantsAsync(name.ToLower());
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
            var productVariants = await productService.GetAllActiveProductVariantsAsync();
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
                return NotFound($"Товар с ID={id} не найден");
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
            var filteredProducts = products.Where(p => p.CategoryId == categoryId).ToList();
            if (filteredProducts.Count == 0)
                return NotFound($"Не найдено товаров с ID категории {categoryId}");
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
            var result = await productService.FilterProductsAsync(search, categoryId, minPrice, maxPrice, ram, storage);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка сервера: {ex.Message}");
        }
    }

    [HttpPost]
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> CreateProduct([FromBody] Product? product)
    {
        try
        {
            if (product == null)
                return BadRequest("Данные товара не могут быть пустыми");

            var createdProduct = await productService.CreateProductAsync(product);
            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Не удалось создать товар: {ex.Message}");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product? product)
    {
        try
        {
            if (product == null)
                return BadRequest("Данные товара не могут быть пустыми");

            if (product.Id != id)
                return BadRequest("ID в URL не совпадает с ID в теле запроса");

            await productService.UpdateProductAsync(id, product);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Товар с ID={id} не найден");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Не удалось обновить товар: {ex.Message}");
        }
    }

    [HttpDelete("{id:int}")]
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            await productService.DeleteProductAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Товар с ID={id} не найден");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Не удалось удалить товар: {ex.Message}");
        }
    }
}