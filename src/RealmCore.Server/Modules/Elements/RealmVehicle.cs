namespace RealmCore.Server.Modules.Elements;

public class RealmVehicle : Vehicle, IFocusableElement
{
    private readonly object _focuseLock = new();
    private readonly List<RealmPlayer> _focusedPlayers = [];
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScope _serviceScope;
    private VehicleAccessController _accessController = VehicleDefaultAccessController.Instance;

    public event Action<Element, RealmPlayer>? PlayerFocused;
    public event Action<Element, RealmPlayer>? PlayerLostFocus;

    public int FocusedPlayerCount
    {
        get
        {
            lock (_focuseLock)
                return _focusedPlayers.Count;
        }
    }

    public IEnumerable<RealmPlayer> FocusedPlayers
    {
        get
        {
            lock (_focuseLock)
            {
                foreach (var focusedPlayer in _focusedPlayers)
                {
                    yield return focusedPlayer;
                }
            }
        }
    }
    public IServiceProvider ServiceProvider => _serviceProvider;
    public int VehicleId => Persistence.Id;

    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public IVehicleAccessFeature Access { get; init; }
    public IVehiclePersistenceFeature Persistence { get; init; }
    public IVehicleMileageCounterFeature MileageCounter { get; init; }
    public new IVehicleUpgradesFeature Upgrades { get; init; }
    public IVehiclePartDamageFeature PartDamage { get; init; }
    public IVehicleEnginesFeature Engines { get; init; }
    public IVehicleEventsFeature Events { get; init; }
    public IVehicleFuelFeature Fuel { get; init; }
    public IVehicleInventoryFeature Inventory { get; init; }

    public VehicleAccessController AccessController
    {
        get => _accessController; set
        {
            if(value == null)
                throw new ArgumentNullException(nameof(value));

            if (_accessController != value)
            {
                _accessController = value;
                AccessControllerChanged?.Invoke(this, _accessController, value);
            }
        }
    }

    public bool IsInMove => Velocity.LengthSquared() > 0.001f;
    public event Action<RealmVehicle, VehicleAccessController, VehicleAccessController>? AccessControllerChanged;

    public RealmVehicle(IServiceProvider serviceProvider, ushort model, Vector3 position) : base(model, position)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        GetRequiredService<VehicleContext>().Vehicle = this;
        GetRequiredService<ElementContext>().Element = this;

        #region Initialize scope services
        Access = GetRequiredService<IVehicleAccessFeature>();
        Persistence = GetRequiredService<IVehiclePersistenceFeature>();
        MileageCounter = GetRequiredService<IVehicleMileageCounterFeature>();
        Upgrades = GetRequiredService<IVehicleUpgradesFeature>();
        PartDamage = GetRequiredService<IVehiclePartDamageFeature>();
        Engines = GetRequiredService<IVehicleEnginesFeature>();
        Events = GetRequiredService<IVehicleEventsFeature>();
        Fuel = GetRequiredService<IVehicleFuelFeature>();
        Inventory = GetRequiredService<IVehicleInventoryFeature>();
        #endregion
    }

    public T GetRequiredService<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();
    public object GetRequiredService(Type type) => _serviceProvider.GetRequiredService(type);

    public bool AddFocusedPlayer(RealmPlayer player)
    {
        lock (_focuseLock)
        {
            if (!_focusedPlayers.Contains(player))
            {
                _focusedPlayers.Add(player);
                player.Destroyed += HandlePlayerDestroyed;
                PlayerFocused?.Invoke(this, player);
                return true;
            }
        }
        return false;
    }

    public bool RemoveFocusedPlayer(RealmPlayer player)
    {
        lock (_focuseLock)
        {
            if (_focusedPlayers.Remove(player))
            {
                player.Destroyed -= HandlePlayerDestroyed;
                PlayerLostFocus?.Invoke(this, player);
                return true;
            }
        }
        return false;
    }

    private void HandlePlayerDestroyed(Element element)
    {
        lock (_focuseLock)
        {
            var player = (RealmPlayer)element;
            if (_focusedPlayers.Remove(player))
            {
                element.Destroyed -= HandlePlayerDestroyed;
                PlayerLostFocus?.Invoke(this, player);
            }
        }
    }
    public override bool Destroy()
    {
        if (base.Destroy())
        {
            _serviceScope.Dispose();
            return true;
        }
        return false;
    }
}
