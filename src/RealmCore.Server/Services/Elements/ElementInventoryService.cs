namespace RealmCore.Server.Services.Elements;

public interface IElementInventoryService
{
    Inventory? Primary { get; }

    event Action<IElementInventoryService, Inventory>? PrimarySet;

    Inventory CreatePrimaryInventory(uint defaultInventorySize);
    bool TryGetPrimary(out Inventory inventory);
}

public interface IPlayerInventoryService : IPlayerService, IElementInventoryService
{

}
public interface IVehicleInventoryService : IVehicleService, IElementInventoryService
{

}

internal sealed class PlayerInventoryService : ElementInventoryService, IPlayerInventoryService
{
    private readonly ItemsRegistry _itemsRegistry;

    public RealmPlayer Player { get; }
    protected override Element Element => Player;

    public PlayerInventoryService(PlayerContext playerContext, IPlayerUserService playerUserService, ItemsRegistry itemsRegistry)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        _itemsRegistry = itemsRegistry;
    }

    private void HandleSignedIn(IPlayerUserService userService, RealmPlayer player)
    {
        Load(Player, userService.User.Inventories, _itemsRegistry);
    }
}

internal sealed class VehicleInventoryService : ElementInventoryService, IVehicleInventoryService
{
    public RealmVehicle Vehicle { get; }
    protected override Element Element => Vehicle;
    public VehicleInventoryService(VehicleContext vehicleContext, IPlayerUserService playerUserService) : base()
    {

    }
}

internal abstract class ElementInventoryService : IElementInventoryService
{
    public Inventory? Primary { get; private set; }
    protected abstract Element Element { get; }

    public event Action<IElementInventoryService, Inventory>? PrimarySet;

    public bool TryGetPrimary(out Inventory inventory)
    {
        inventory = Primary!;
        return inventory != null;
    }

    protected bool Load(Element element, ICollection<InventoryData> inventories, ItemsRegistry itemsRegistry)
    {
        if (inventories != null && inventories.Count != 0)
        {
            var inventory = inventories.First();

            Primary = Inventory.CreateFromData(element, inventory, itemsRegistry);
            PrimarySet?.Invoke(this, Primary);
            return true;
        }
        return false;
    }

    public Inventory CreatePrimaryInventory(uint defaultInventorySize)
    {
        if (Primary == null)
        {
            Primary = new Inventory(Element, defaultInventorySize);
            PrimarySet?.Invoke(this, Primary);
        }
        return Primary;
    }
}
