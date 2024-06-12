namespace RealmCore.Server.Modules.Server.Loading;

public sealed class VehicleLoadingOptions
{
    public bool SkipVehicleLoading { get; set; }
}

internal sealed class LoadVehicleService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<VehicleLoadingOptions> _options;

    public LoadVehicleService(IServiceProvider serviceProvider, IOptions<VehicleLoadingOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.Value.SkipVehicleLoading)
            return;

        using var scope = _serviceProvider.CreateScope();
        var vehicleLoader = scope.ServiceProvider.GetRequiredService<IVehicleLoader>();
        await vehicleLoader.LoadAll(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}