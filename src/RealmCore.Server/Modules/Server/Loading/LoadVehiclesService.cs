namespace RealmCore.Server.Modules.Server.Loading;

public sealed class VehicleLoadingOptions
{
    public bool SkipVehicleLoading { get; set; }
}

internal sealed class LoadVehiclesService : IHostedLifecycleService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<VehicleLoadingOptions> _options;

    public LoadVehiclesService(IServiceProvider serviceProvider, IOptions<VehicleLoadingOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        if (_options.Value.SkipVehicleLoading)
            return;

        using var scope = _serviceProvider.CreateScope();
        var vehicleLoader = scope.ServiceProvider.GetRequiredService<VehicleLoader>();
        await vehicleLoader.LoadAll(cancellationToken);
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}