namespace RealmCore.Server.Modules.Inventories;

public interface IElementInventoryFeature
{
    Inventory? Primary { get; }

    event Action<IElementInventoryFeature, Inventory>? PrimarySet;

    Inventory CreatePrimaryInventory(uint defaultInventorySize);
    bool TryGetPrimary(out Inventory inventory);
}

public interface IPlayerInventoryFeature : IPlayerFeature, IElementInventoryFeature
{

}
public interface IVehicleInventoryFeature : IVehicleFeature, IElementInventoryFeature
{

}

internal sealed class PlayerInventoryFeature : ElementInventoryFeature, IPlayerInventoryFeature, IUsesUserPersistentData
{
    private readonly ItemsCollection _itemsCollection;

    public event Action? VersionIncreased;

    public RealmPlayer Player { get; }
    protected override Element Element => Player;

    public PlayerInventoryFeature(PlayerContext playerContext, ItemsCollection itemsCollection)
    {
        Player = playerContext.Player;
        _itemsCollection = itemsCollection;
    }

    public void SignIn(UserData userData)
    {
        Load(Player, userData.Inventories, _itemsCollection);
    }

    public void SignOut()
    {

    }
}

internal sealed class VehicleInventoryFeature : ElementInventoryFeature, IVehicleInventoryFeature
{
    public RealmVehicle Vehicle { get; }
    protected override Element Element => Vehicle;
    public VehicleInventoryFeature(VehicleContext vehicleContext) : base()
    {
        Vehicle = vehicleContext.Vehicle;
    }
}

internal abstract class ElementInventoryFeature : IElementInventoryFeature
{
    public Inventory? Primary { get; private set; }
    protected abstract Element Element { get; }

    public event Action<IElementInventoryFeature, Inventory>? PrimarySet;

    public bool TryGetPrimary(out Inventory inventory)
    {
        inventory = Primary!;
        return inventory != null;
    }

    protected bool Load(Element element, ICollection<InventoryData> inventories, ItemsCollection itemsCollection)
    {
        if (inventories != null && inventories.Count != 0)
        {
            var inventory = inventories.First();

            Primary = Inventory.CreateFromData(element, inventory, itemsCollection);
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
