namespace RealmCore.Server.Modules.Server.Loading;

internal sealed class VehicleServerLoader : IServerLoader
{
    private readonly IVehicleLoader _vehicleLoader;

    public VehicleServerLoader(IVehicleLoader vehicleLoader)
    {
        _vehicleLoader = vehicleLoader;
    }

    public async Task Load()
    {
        await _vehicleLoader.LoadAll();
    }
}