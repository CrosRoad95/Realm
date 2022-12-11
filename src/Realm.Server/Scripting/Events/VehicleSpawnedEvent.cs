namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class VehicleSpawnedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private RPGVehicle _rpgVehicle;
    private RPGSpawn? _spawn;

    public static string EventName => "onVehicleSpawn";

    [ScriptMember("vehicle")]
    public RPGVehicle RPGVehicle
    {
        get
        {
            CheckIfDisposed();
            return _rpgVehicle;
        }
    }

    [ScriptMember("spawn")]
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
        _rpgVehicle = vehicle;
        _spawn = spawn;
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
