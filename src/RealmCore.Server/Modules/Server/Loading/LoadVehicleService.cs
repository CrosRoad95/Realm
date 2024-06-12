namespace RealmCore.Server.Modules.Server.Loading;

public sealed class VehicleLoadingOptions
{
    public bool SkipVehicleLoading { get; set; }
}

internal sealed class LoadVehicleService : IHostedService
{
    private readonly IVehicleLoader _vehicleLoader;
    private readonly IOptions<VehicleLoadingOptions> _options;

    public LoadVehicleService(IVehicleLoader vehicleLoader, IOptions<VehicleLoadingOptions> options)
    {
        _vehicleLoader = vehicleLoader;
        _options = options;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.Value.SkipVehicleLoading)
            return;

        await _vehicleLoader.LoadAll(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}