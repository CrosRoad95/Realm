namespace Realm.Server.Elements;

public class RPGVehicle : Vehicle, IDisposable
{
    private bool _disposed = false;
    private string? _id = null;
    private readonly ILogger _logger;
    private readonly MtaServer _mtaServer;
    private readonly EventScriptingFunctions _eventFunctions;
    private readonly bool _isPersistant = PersistantScope.IsPersistant;

    public RPGVehicle(MtaServer mtaServer, ILogger logger, EventScriptingFunctions eventFunctions) : base(404, new Vector3(0,0, 10000))
    {
        _mtaServer = mtaServer;
        _eventFunctions = eventFunctions;
        _logger = logger
            .ForContext<RPGVehicle>()
            .ForContext(new RPGVehicleEnricher(this));
        IsFrozen = true;
    }

    [NoScriptAccess]
    public void AssignId(string id)
    {
        _id = id;
    }

    public new bool Destroy()
    {
        if (_isPersistant)
            return false;

        Dispose();
        return base.Destroy();
    }

    public virtual async Task<bool> Spawn(Spawn spawn)
    {
        Position = spawn.Position;
        Rotation = spawn.Rotation;
        using var vehicleSpawnedEvent = new VehicleSpawnedEvent(this, spawn);
        await _eventFunctions.InvokeEvent(vehicleSpawnedEvent);
        _logger.Verbose("Spawned at {spawn}", spawn);
        return true;
    }

    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    public bool IsPersistant()
    {
        CheckIfDisposed();
        return _isPersistant;
    }

    public string LongUserFriendlyName() => Name;
    public override string ToString() => Name;

    public void Dispose()
    {
        _disposed = true;
    }
}
