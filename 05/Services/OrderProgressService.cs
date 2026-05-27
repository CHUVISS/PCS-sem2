using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;

namespace ProductionManagement.Services;

/// <summary>
/// Фоновый сервис, который каждые 5 секунд автоматически обновляет
/// прогресс заказов на основе реального времени и завершает их.
/// </summary>
public class OrderProgressService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderProgressService> _logger;

    public OrderProgressService(IServiceScopeFactory scopeFactory, ILogger<OrderProgressService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderProgressService запущен");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateOrdersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении заказов");
            }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task UpdateOrdersAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.Now;

        var activeOrders = await db.WorkOrders
            .Where(w => w.Status == "Pending" || w.Status == "InProgress")
            .ToListAsync();

        if (activeOrders.Count == 0) return;

        bool anyChanged = false;

        foreach (var order in activeOrders)
        {
            // Заказ ещё не начался
            if (order.StartDate > now) continue;

            // Pending → InProgress
            if (order.Status == "Pending")
            {
                order.Status = "InProgress";
                anyChanged = true;
                _logger.LogInformation("Заказ #{Id} переведён в InProgress", order.Id);
            }

            // Считаем прогресс по реальному времени
            var totalSeconds = (order.EstimatedEndDate - order.StartDate).TotalSeconds;
            var elapsedSeconds = (now - order.StartDate).TotalSeconds;

            int newProgress = totalSeconds <= 0
                ? 100
                : (int)Math.Clamp(elapsedSeconds / totalSeconds * 100, 0, 100);

            if (newProgress != order.ProgressPercent)
            {
                order.ProgressPercent = newProgress;
                anyChanged = true;
            }

            // Завершаем заказ
            if (order.ProgressPercent >= 100)
            {
                order.ProgressPercent = 100;
                order.Status = "Completed";
                anyChanged = true;
                _logger.LogInformation("Заказ #{Id} автоматически завершён", order.Id);

                // Освобождаем производственную линию
                if (order.ProductionLineId.HasValue)
                {
                    var line = await db.ProductionLines.FindAsync(order.ProductionLineId.Value);
                    if (line != null && line.CurrentWorkOrderId == order.Id)
                    {
                        line.CurrentWorkOrderId = null;
                    }
                }
            }
        }

        if (anyChanged)
            await db.SaveChangesAsync();
    }
}
