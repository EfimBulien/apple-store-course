using Microsoft.EntityFrameworkCore;
using TechStoreEll.Core.DTOs;
using TechStoreEll.Core.Infrastructure.Data;

namespace TechStoreEll.Core.Services;

public class ProductService(AppDbContext context)
{
    public async Task<List<ProductFullDto>> GetAllProductVariantsAsync()
    {
        var variants = await context.ProductVariants
            .Include(v => v.Product)
            .Where(v => v.Product.Active) // пока только активные 
            .Select(v => new ProductFullDto()
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
            })
            .ToListAsync();

        return variants;
    }
    
    public async Task<List<ProductFullDto>> SearchProductVariantsAsync(string searchTerm)
    {
        var query = context.ProductVariants
            .Include(v => v.Product)
            .Where(v => v.Product.Active);
        
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(v => 
                v.Product.Name!.Contains(searchTerm) || 
                v.Product.Sku!.Contains(searchTerm) ||
                v.VariantCode.Contains(searchTerm) ||
                v.Color!.Contains(searchTerm) ||
                v.Product.Description!.Contains(searchTerm));
        }
        else
        {
            query = query.Where(v => v.Product.Active);
        }

        var variants = await query
            .Select(v => new ProductFullDto()
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
            })
            .ToListAsync();

        return variants;
    }
    
    public async Task<List<ProductDto>> SearchProductsAsync(string? searchTerm)
    {
        var query = context.Products.Where(p => p.Active)
            .Include(p => p.ProductVariants)
            .AsQueryable();
        
        if (!string.IsNullOrEmpty(searchTerm))
        {
            // описание может быть нул пока что и будет выгружено все равно
            query = query.Where(p => p.Name.Contains(searchTerm) || p.Sku.Contains(searchTerm) || p.Description
                .Contains(searchTerm));
        }
        
        var products = await query.ToListAsync();

        return products.Select(p => new ProductDto()
        {
            Id = p.Id,
            Sku = p.Sku,
            Name = p.Name,
            CategoryId = p.CategoryId,
            Description = p.Description,
            Active = p.Active,
            CreatedAt = p.CreatedAt,
            AvgRating = p.AvgRating,
            ReviewsCount = p.ReviewsCount,
            Variants = p.ProductVariants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                VariantCode = v.VariantCode,
                Price = v.Price,
                Color = v.Color,
                StorageGb = v.StorageGb,
                Ram = v.Ram
            }).ToList()
        }).ToList();
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id) 
    {
        var product = await context.Products
            .Include(p => p.ProductVariants)
            .FirstOrDefaultAsync(p => p.Id == id && p.Active);

        if (product == null)
            return null;

        return new ProductDto
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            CategoryId = product.CategoryId,
            Description = product.Description,
            Active = product.Active,
            CreatedAt = product.CreatedAt,
            AvgRating = product.AvgRating,
            ReviewsCount = product.ReviewsCount,
            Variants = product.ProductVariants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                VariantCode = v.VariantCode,
                Price = v.Price,
                Color = v.Color,
                StorageGb = v.StorageGb,
                Ram = v.Ram
            }).ToList()
        };
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        var products = await context.Products
            .Where(p => p.Active)
            .Include(p => p.ProductVariants)
            .ToListAsync();

        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Sku = p.Sku,
            Name = p.Name,
            CategoryId = p.CategoryId,
            Description = p.Description,
            Active = p.Active,
            CreatedAt = p.CreatedAt,
            AvgRating = p.AvgRating,
            ReviewsCount = p.ReviewsCount,
            Variants = p.ProductVariants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                VariantCode = v.VariantCode,
                Price = v.Price,
                Color = v.Color,
                StorageGb = v.StorageGb,
                Ram = v.Ram
            }).ToList()
        }).ToList();
    }
    
    public async Task<List<ProductFullDto>> FullTextSearchAsync(string searchTerm)
    {
        var query = context.ProductVariants
            .Include(v => v.Product)
            .Where(v => v.Product.Active);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(v =>
                EF.Functions.ToTsVector("russian", v.Product.Name + " " + v.Product.Description + " " 
                                                   + v.Product.Sku + " " + v.VariantCode + " " + v.Color)
                    .Matches(EF.Functions.WebSearchToTsQuery("russian", searchTerm))
            );
        }

        return await query.Select(v => new ProductFullDto
        {
            Id = v.Id,
            ProductId = v.ProductId,
            VariantCode = v.VariantCode,
            Price = v.Price,
            Color = v.Color,
            StorageGb = v.StorageGb,
            Ram = v.Ram,
            ProductSku = v.Product.Sku,
            ProductName = v.Product.Name,
            CategoryId = v.Product.CategoryId,
            ProductDescription = v.Product.Description,
            ProductActive = v.Product.Active,
            ProductCreatedAt = v.Product.CreatedAt,
            ProductAvgRating = v.Product.AvgRating,
            ProductReviewsCount = v.Product.ReviewsCount
        }).ToListAsync();
    }
}
