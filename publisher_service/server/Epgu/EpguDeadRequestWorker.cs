
using Microsoft.EntityFrameworkCore;

namespace Epgu;

public class EpguDeadRequestWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<EpguDeadRequestWorker> _logger;

    public EpguDeadRequestWorker(IServiceProvider services, ILogger<EpguDeadRequestWorker> logger)
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
            var smevIntegrationService = scope.ServiceProvider.GetRequiredService<EpguIntegrationService>();

            var pendingOrders = await context.Orders
                .Include(o => o.EpguOrder)
                .Include(o => o.Documents)
                .Where(o => o.EpguOrder != null && o.IsDeadRequest == true)
                .Take(10)
                .ToListAsync(stoppingToken);

            foreach (var order in pendingOrders)
            {
                try
                {
                    // Отправка запрос в ЕПГУ
                    Console.WriteLine("отправка запроса к ЕПГУ");
                    var epguOrderId = await smevIntegrationService.FetchOrderIdAsync();
                    epguOrderId = await smevIntegrationService.SendOrderAsync(order);

                    if (epguOrderId is not null)
                    {
                        order.EpguOrder.EpguOrderCode = epguOrderId;
                        order.IsDeadRequest = false;
                    }

                    context.SaveChanges();
                    // ----

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка фоновой отправки заказа {OrderId}", order.Id);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}