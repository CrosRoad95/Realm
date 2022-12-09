namespace Realm.Server.Scripting.Events;

public class VehicleSpawnedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private RPGVehicle _vehicle;
    private RPGSpawn? _spawn;

    public static string EventName => "onVehicleSpawn";

    public RPGVehicle Vehicle
    {
        get
        {
            CheckIfDisposed();
            return _vehicle;
        }
    }

    public RPGSpawn? Spawn
    {
        get
        {
            CheckIfDisposed();
            return _spawn;
        }
    }

    public VehicleSpawnedEvent(RPGVehicle vehicle, RPGSpawn? spawn)
    {
        _vehicle = vehicle;
        _spawn = spawn;
    }

    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    [NoScriptAccess]
    public void Dispose()
    {
        _disposed = true;
    }
}
