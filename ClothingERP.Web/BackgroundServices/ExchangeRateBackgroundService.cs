namespace ClothingERP.Web.BackgroundServices;

public class ExchangeRateBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExchangeRateBackgroundService> _logger;

    public ExchangeRateBackgroundService(IServiceProvider serviceProvider, ILogger<ExchangeRateBackgroundService> logger)
        => (_serviceProvider, _logger) = (serviceProvider, logger);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
  
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var rateSvc = scope.ServiceProvider.GetRequiredService<IExchangeRateService>();

                if (await rateSvc.ShouldAutoUpdateAsync())
                {
                    _logger.LogInformation("Auto-refreshing exchange rates...");
                    var result = await rateSvc.RefreshFromApiAsync();
                    _logger.LogInformation("Exchange rate refresh result: {Success} — {Message}", result.Success, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExchangeRateBackgroundService loop.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}