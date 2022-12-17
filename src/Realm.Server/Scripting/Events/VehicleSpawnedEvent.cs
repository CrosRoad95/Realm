namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class VehicleSpawnedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private Vehicle _vehicle;
    private Vector3 _position;

    public static string EventName => "onVehicleSpawn";

    [ScriptMember("vehicle")]
    public Vehicle RPGVehicle
    {
        get
        {
            CheckIfDisposed();
            return _vehicle;
        }
    }

    [ScriptMember("spawn")]
    public Vector3 Spawn
    {
        get
        {
            CheckIfDisposed();
            return _position;
        }
    }

    public VehicleSpawnedEvent(Vehicle vehicle, Vector3 position)
    {
        _vehicle = vehicle;
        _position = position;
    }

    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    public void Dispose()
    {
        _disposed = true;
    }
}
