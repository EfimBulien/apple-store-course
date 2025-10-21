// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using Prometheus;
// using TechStoreEll.Core.Infrastructure.Data;
//
// namespace TechStoreEll.Core.Services;
//
// public class MetricsService : BackgroundService
// {
//     private const int UpdateSeconds = 15;
//     private const string MetricsFilePath = "metrics_state.json";
//         
//     private readonly IServiceScopeFactory _scopeFactory;
//     private readonly ILogger<MetricsService> _logger;
//         
//     private static readonly Gauge ActiveUsers = Metrics.CreateGauge(
//         "techstore_active_users", "Текущее количество активных пользователей");
//
//     private static readonly Gauge DbRecordCount = Metrics.CreateGauge(
//         "techstore_db_record_count", "Количество записей в таблице Products");
//
//     private static readonly Counter TotalOrders = Metrics.CreateCounter(
//         "techstore_total_orders", "Общее количество заказов");
//
//     public MetricsService(IServiceScopeFactory scopeFactory, ILogger<MetricsService> logger)
//     {
//         _scopeFactory = scopeFactory;
//         _logger = logger;
//         LoadPersistentMetrics();
//     }
//
//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         while (!stoppingToken.IsCancellationRequested)
//         {
//             try
//             {
//                 using var scope = _scopeFactory.CreateScope();
//                 var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//
//                 var productsCount = await db.ProductVariants.CountAsync(stoppingToken);
//                 DbRecordCount.Set(productsCount);
//
//                 var usersCount = await db.Users.CountAsync(u => u.IsActive, stoppingToken);
//                 ActiveUsers.Set(usersCount);
//
//                 var ordersCount = await db.Orders.CountAsync(stoppingToken);
//                 if (ordersCount > TotalOrders.Value)
//                     TotalOrders.Inc(ordersCount - (int)TotalOrders.Value);
//
//                 SavePersistentMetrics();
//
//                 _logger.LogInformation("Метрики обновлены: {time}", DateTimeOffset.Now);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Ошибка обновления метрик");
//             }
//
//             await Task.Delay(TimeSpan.FromSeconds(UpdateSeconds), stoppingToken);
//         }
//     }
//         
//     private static void SavePersistentMetrics()
//     {
//         var state = new
//         {
//             totalOrders = TotalOrders.Value
//         };
//         File.WriteAllText(MetricsFilePath, System.Text.Json.JsonSerializer.Serialize(state));
//     }
//
//     private void LoadPersistentMetrics()
//     {
//         if (!File.Exists(MetricsFilePath)) return;
//
//         try
//         {
//             var json = File.ReadAllText(MetricsFilePath);
//             var state = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double>>(json);
//             if (state != null && state.TryGetValue("totalOrders", out var value))
//                 TotalOrders.Inc(value);
//             
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Ошибка восстановления метрик");
//         }
//     }
// }