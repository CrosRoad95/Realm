namespace RealmCore.Server.Modules.Elements;

public class RealmVehicle : Vehicle, IFocusableElement, IAsyncDisposable
{
    private readonly object _focuseLock = new();
    private readonly List<RealmPlayer> _focusedPlayers = [];
    private readonly IServiceProvider _serviceProvider;
    private readonly AsyncServiceScope _serviceScope;
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

    public ElementSaveService Saving { get; init; }
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public VehicleAccessFeature Access { get; init; }
    public VehiclePersistenceFeature Persistence { get; init; }
    public VehicleMileageCounterFeature MileageCounter { get; init; }
    public new VehicleUpgradesFeature Upgrades { get; init; }
    public VehiclePartDamageFeature PartDamage { get; init; }
    public VehicleEnginesFeature Engines { get; init; }
    public VehicleEventsFeature Events { get; init; }
    public VehicleFuelFeature Fuel { get; init; }
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
        _serviceScope = serviceProvider.CreateAsyncScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        GetRequiredService<VehicleContext>().Vehicle = this;
        GetRequiredService<ElementContext>().Element = this;

        Saving = GetRequiredService<ElementSaveService>();

        #region Initialize scope services
        Access = GetRequiredService<VehicleAccessFeature>();
        Persistence = GetRequiredService<VehiclePersistenceFeature>();
        MileageCounter = GetRequiredService<VehicleMileageCounterFeature>();
        Upgrades = GetRequiredService<VehicleUpgradesFeature>();
        PartDamage = GetRequiredService<VehiclePartDamageFeature>();
        Engines = GetRequiredService<VehicleEnginesFeature>();
        Events = GetRequiredService<VehicleEventsFeature>();
        Fuel = GetRequiredService<VehicleFuelFeature>();
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

    public event Action<RealmVehicle>? Disposed;

    public async ValueTask DisposeAsync()
    {
        if (Destroy())
        {
            await _serviceScope.DisposeAsync();
            Disposed?.Invoke(this);
        }
    }
}
