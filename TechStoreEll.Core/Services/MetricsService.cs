using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using TechStoreEll.Core.Infrastructure.Data;

namespace TechStoreEll.Core.Services;

public class MetricsService : BackgroundService
{
    private const int UpdateSeconds = 15;
    private const string MetricsFilePath = "metrics_state.json";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MetricsService> _logger;

    
    private static readonly Gauge ActiveUsers = Metrics.CreateGauge(
        "tech_store_active_users", "Текущее количество активных пользователей");

    private static readonly Gauge DbRecordCount = Metrics.CreateGauge(
        "tech_store_db_record_count", "Количество записей в таблице Products");

    private static readonly Counter TotalReserve = Metrics.CreateCounter(
        "tech_store_total_reserve", "Общее количество зарезервированных единиц товара");

    private static readonly Gauge AverageProductPrice = Metrics.CreateGauge(
        "tech_store_avg_product_price", "Средняя цена всех товарных вариантов");

    private static readonly Gauge ProductsWithoutImages = Metrics.CreateGauge(
        "tech_store_products_without_images", "Количество вариантов товаров без изображений");

    private static readonly Gauge AverageRating = Metrics.CreateGauge(
        "tech_store_avg_rating", "Средний рейтинг по всем товарам");

    private static readonly Gauge PendingReviews = Metrics.CreateGauge(
        "tech_store_pending_reviews", "Количество отзывов, ожидающих модерации");

    private static readonly Gauge LowRatingProducts = Metrics.CreateGauge(
        "tech_store_low_rating_products", "Процент товаров с рейтингом ниже 4.0");

    private static readonly Gauge TotalStockQuantity = Metrics.CreateGauge(
        "tech_store_total_stock_quantity", "Общее количество единиц товара на складе");

    private static readonly Gauge ReserveRatio = Metrics.CreateGauge(
        "tech_store_inventory_reserve_ratio", "Процент зарезервированных товаров");

    private static readonly Gauge ProductCountByCategory = Metrics.CreateGauge(
        "tech_store_products_by_category",
        "Количество товарных вариантов по категориям",
        labelNames: ["category"]);

    private static readonly Gauge AvgPriceByCategory = Metrics.CreateGauge(
        "tech_store_avg_price_by_category",
        "Средняя цена товарных вариантов по категориям",
        labelNames: ["category"]);

    private static readonly Gauge ProductsWithoutImagesByCategory = Metrics.CreateGauge(
        "tech_store_products_without_images_by_category",
        "Количество товарных вариантов без изображений по категориям",
        labelNames: ["category"]);

    private static readonly Gauge StockQuantityByCategory = Metrics.CreateGauge(
        "tech_store_stock_quantity_by_category",
        "Общее количество товара на складе по категориям",
        labelNames: ["category"]);

    private static readonly Gauge ReservedQuantityByCategory = Metrics.CreateGauge(
        "tech_store_reserved_quantity_by_category",
        "Количество зарезервированных единиц по категориям",
        labelNames: ["category"]);

    
    private static readonly Dictionary<string, double> LastCategoryProductCounts = new();
    private static readonly Dictionary<string, double> LastCategoryAvgPrices = new();
    private static readonly Dictionary<string, double> LastCategoryWithoutImages = new();
    private static readonly Dictionary<string, double> LastCategoryStockQuantities = new();
    private static readonly Dictionary<string, double> LastCategoryReservedQuantities = new();

    public MetricsService(IServiceScopeFactory scopeFactory, ILogger<MetricsService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        LoadPersistentMetrics();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await UpdateMetricsAsync(db, stoppingToken);
                SavePersistentMetrics();
                
                _logger.LogInformation("Метрики обновлены: {time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления метрик");
            }

            await Task.Delay(TimeSpan.FromSeconds(UpdateSeconds), stoppingToken);
        }
    }

    private async Task UpdateMetricsAsync(AppDbContext db, CancellationToken stoppingToken)
    {
        
        var productsCount = await db.ProductVariants.CountAsync(stoppingToken);
        DbRecordCount.Set(productsCount);

        var usersCount = await db.Users.CountAsync(u => u.IsActive, stoppingToken);
        ActiveUsers.Set(usersCount);

        var reserveCount = await db.Inventories.SumAsync(i => (int?)i.Reserve, stoppingToken) ?? 0;
        if (reserveCount > TotalReserve.Value)
            TotalReserve.Inc(reserveCount - TotalReserve.Value);

        var avgPrice = await db.ProductVariants.AverageAsync(v => (double?)v.Price, stoppingToken) ?? 0;
        AverageProductPrice.Set(avgPrice);

        var withoutImages = await db.ProductVariants
            .Where(v => !db.ProductImages.Any(i => i.ProductVariantId == v.Id))
            .CountAsync(stoppingToken);
        ProductsWithoutImages.Set(withoutImages);

        var avgRating = await db.Products.AverageAsync(p => (double?)p.AvgRating, stoppingToken) ?? 0;
        AverageRating.Set(avgRating);

        var pending = await db.Reviews.CountAsync(r => !r.IsModerated, stoppingToken);
        PendingReviews.Set(pending);

        var totalProducts = await db.Products.CountAsync(stoppingToken);
        if (totalProducts > 0)
        {
            var lowRated = await db.Products.CountAsync(p => p.AvgRating < (decimal?)4.0, stoppingToken);
            LowRatingProducts.Set((double)lowRated / totalProducts * 100);
        }

        var totalStock = await db.Inventories.SumAsync(i => (int?)i.Quantity, stoppingToken) ?? 0;
        TotalStockQuantity.Set(totalStock);

        var totalReserve = await db.Inventories.SumAsync(i => (int?)i.Reserve, stoppingToken) ?? 0;
        if (totalStock > 0)
            ReserveRatio.Set((double)totalReserve / totalStock * 100);

        
        await UpdateCategoryMetricsAsync(db, stoppingToken);
    }

    private async Task UpdateCategoryMetricsAsync(AppDbContext db, CancellationToken stoppingToken)
    {
        var categoryMetrics = await (
            from variant in db.ProductVariants
            join product in db.Products on variant.ProductId equals product.Id
            join category in db.Categories on product.CategoryId equals category.Id into catGroup
            from cat in catGroup.DefaultIfEmpty()
            let categoryName = cat != null ? cat.Name : "Без категории"
            join inventory in db.Inventories on variant.Id equals inventory.ProductVariantId into invGroup
            from inv in invGroup.DefaultIfEmpty()
            join image in db.ProductImages on variant.Id equals image.ProductVariantId into imgGroup
            group new { variant, inv, HasImage = imgGroup.Any() } by categoryName into g
            select new
            {
                Category = g.Key,
                ProductCount = g.Count(),
                AvgPrice = g.Average(x => (double?)x.variant.Price) ?? 0,
                WithoutImages = g.Count(x => !x.HasImage),
                TotalStock = g.Sum(x => x.inv != null ? x.inv.Quantity : 0),
                TotalReserved = g.Sum(x => x.inv != null ? x.inv.Reserve : 0)
            }
        ).ToDictionaryAsync(x => x.Category, x => x, stoppingToken);

        
        foreach (var kvp in categoryMetrics)
        {
            var category = kvp.Key;
            var metrics = kvp.Value;

            
            LastCategoryProductCounts[category] = metrics.ProductCount;
            LastCategoryAvgPrices[category] = metrics.AvgPrice;
            LastCategoryWithoutImages[category] = metrics.WithoutImages;
            LastCategoryStockQuantities[category] = metrics.TotalStock;
            LastCategoryReservedQuantities[category] = metrics.TotalReserved;

            
            ProductCountByCategory.WithLabels(category).Set(metrics.ProductCount);
            AvgPriceByCategory.WithLabels(category).Set(metrics.AvgPrice);
            ProductsWithoutImagesByCategory.WithLabels(category).Set(metrics.WithoutImages);
            StockQuantityByCategory.WithLabels(category).Set(metrics.TotalStock);
            ReservedQuantityByCategory.WithLabels(category).Set(metrics.TotalReserved);
        }

        
        RemoveObsoleteCategoryMetrics(categoryMetrics.Keys);
    }

    private void RemoveObsoleteCategoryMetrics(IEnumerable<string> currentCategories)
    {
        var currentSet = new HashSet<string>(currentCategories);

        RemoveFromGauge(ProductCountByCategory, LastCategoryProductCounts, currentSet);
        RemoveFromGauge(AvgPriceByCategory, LastCategoryAvgPrices, currentSet);
        RemoveFromGauge(ProductsWithoutImagesByCategory, LastCategoryWithoutImages, currentSet);
        RemoveFromGauge(StockQuantityByCategory, LastCategoryStockQuantities, currentSet);
        RemoveFromGauge(ReservedQuantityByCategory, LastCategoryReservedQuantities, currentSet);
    }

    private void RemoveFromGauge(Gauge gauge, Dictionary<string, double> lastValues, HashSet<string> currentCategories)
    {
        var obsoleteCategories = lastValues.Keys.Except(currentCategories).ToList();
        foreach (var category in obsoleteCategories)
        {
            gauge.RemoveLabelled(category);
            lastValues.Remove(category);
        }
    }

    private void SavePersistentMetrics()
    {
        try
        {
            var state = new MetricsState
            {
                TotalReserve = TotalReserve.Value,
                CategoryProductCounts = LastCategoryProductCounts,
                CategoryAvgPrices = LastCategoryAvgPrices,
                CategoryWithoutImages = LastCategoryWithoutImages,
                CategoryStockQuantities = LastCategoryStockQuantities,
                CategoryReservedQuantities = LastCategoryReservedQuantities,
                SavedAt = DateTime.UtcNow
            };

            var json = System.Text.Json.JsonSerializer.Serialize(state, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(MetricsFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка сохранения метрик");
        }
    }

    private void LoadPersistentMetrics()
    {
        if (!File.Exists(MetricsFilePath)) return;

        try
        {
            var json = File.ReadAllText(MetricsFilePath);
            var state = System.Text.Json.JsonSerializer.Deserialize<MetricsState>(json);
            
            if (state != null)
            {
                
                if (state.TotalReserve > TotalReserve.Value)
                    TotalReserve.Inc(state.TotalReserve - TotalReserve.Value);

                
                RestoreCategoryMetrics(state);
                
                _logger.LogInformation("Метрики восстановлены из файла: {time}", state.SavedAt);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка восстановления метрик");
        }
    }

    private void RestoreCategoryMetrics(MetricsState state)
    {
        RestoreGaugeMetrics(ProductCountByCategory, state.CategoryProductCounts);
        RestoreGaugeMetrics(AvgPriceByCategory, state.CategoryAvgPrices);
        RestoreGaugeMetrics(ProductsWithoutImagesByCategory, state.CategoryWithoutImages);
        RestoreGaugeMetrics(StockQuantityByCategory, state.CategoryStockQuantities);
        RestoreGaugeMetrics(ReservedQuantityByCategory, state.CategoryReservedQuantities);

        
        LastCategoryProductCounts.Clear();
        foreach (var kvp in state.CategoryProductCounts)
            LastCategoryProductCounts[kvp.Key] = kvp.Value;

        LastCategoryAvgPrices.Clear();
        foreach (var kvp in state.CategoryAvgPrices)
            LastCategoryAvgPrices[kvp.Key] = kvp.Value;

        LastCategoryWithoutImages.Clear();
        foreach (var kvp in state.CategoryWithoutImages)
            LastCategoryWithoutImages[kvp.Key] = kvp.Value;

        LastCategoryStockQuantities.Clear();
        foreach (var kvp in state.CategoryStockQuantities)
            LastCategoryStockQuantities[kvp.Key] = kvp.Value;

        LastCategoryReservedQuantities.Clear();
        foreach (var kvp in state.CategoryReservedQuantities)
            LastCategoryReservedQuantities[kvp.Key] = kvp.Value;
    }

    private void RestoreGaugeMetrics(Gauge gauge, Dictionary<string, double> values)
    {
        foreach (var kvp in values)
        {
            gauge.WithLabels(kvp.Key).Set(kvp.Value);
        }
    }

    private class MetricsState
    {
        public double TotalReserve { get; set; }
        public Dictionary<string, double> CategoryProductCounts { get; set; } = new();
        public Dictionary<string, double> CategoryAvgPrices { get; set; } = new();
        public Dictionary<string, double> CategoryWithoutImages { get; set; } = new();
        public Dictionary<string, double> CategoryStockQuantities { get; set; } = new();
        public Dictionary<string, double> CategoryReservedQuantities { get; set; } = new();
        public DateTime SavedAt { get; set; }
    }
}