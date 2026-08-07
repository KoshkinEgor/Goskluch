using Microsoft.EntityFrameworkCore;

namespace Smev;

public class SmevDeadRequestWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<SmevDeadRequestWorker> _logger;

    public SmevDeadRequestWorker(IServiceProvider services, ILogger<SmevDeadRequestWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Repo.ApplicationContext>();
            var smevIntegrationService = scope.ServiceProvider.GetRequiredService<SmevIntegrationService>();

            var pendingOrders = await context.Orders
                .Include(o => o.SmevOrder)
                .Include(o => o.Documents)
                .Where(o => o.SmevOrder != null && o.IsDeadRequest == true)
                .Take(10)
                .ToListAsync(stoppingToken);

            foreach (var order in pendingOrders)
            {
                try
                {
                    Console.WriteLine("Отправка запроса к СМЭВ");
                    var smevMessageId = await smevIntegrationService.SendOrderAsync(order);
                    order.SmevOrder.SmevMessageId = smevMessageId ?? "";
                    order.SmevOrder.OrderStatusId = "";
                    if (smevMessageId is not null)
                    {
                        Console.WriteLine("Запрос доставлен");
                        order.IsDeadRequest = false;
                    }
                    context.SaveChanges();
                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка фоновой отправки заказа {OrderId}", order.Id);
                    // Логика ретраев (экспоненциальная задержка)
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}