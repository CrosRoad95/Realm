﻿namespace RealmCore.Server.Modules.Inventories;

public interface IElementInventoryFeature
{
    ElementInventory? Primary { get; }

    event Action<IElementInventoryFeature, ElementInventory>? PrimarySet;

    ElementInventory CreatePrimary(uint defaultInventorySize);
    bool TryGetPrimary(out ElementInventory inventory);
}

public interface IPlayerInventoryFeature : IPlayerFeature, IElementInventoryFeature
{

}
public interface IVehicleInventoryFeature : IVehicleFeature, IElementInventoryFeature
{

}

public abstract class ElementInventoryFeature : IElementInventoryFeature
{
    protected readonly ItemsCollection _itemsCollection;

    public ElementInventory? Primary { get; private set; }
    protected abstract Element Element { get; }

    public event Action<IElementInventoryFeature, ElementInventory>? PrimarySet;

    public ElementInventoryFeature(ItemsCollection itemsCollection)
    {
        _itemsCollection = itemsCollection;
    }

    public bool TryGetPrimary(out ElementInventory inventory)
    {
        inventory = Primary!;
        return inventory != null;
    }

    protected bool Load(Element element, ICollection<InventoryData> inventories, ItemsCollection itemsCollection)
    {
        if (inventories != null && inventories.Count != 0)
        {
            var inventory = inventories.First();

            Primary = PersistentElementInventory.CreateFromData(element, inventory, itemsCollection);
            PrimarySet?.Invoke(this, Primary);
            return true;
        }
        return false;
    }

    public ElementInventory CreatePrimary(uint inventorySize)
    {
        if (Primary == null)
        {
            Primary = new PersistentElementInventory(0, Element, inventorySize, _itemsCollection);
            PrimarySet?.Invoke(this, Primary);
        }
        return Primary;
    }
}

public sealed class PlayerInventoryFeature : ElementInventoryFeature, IPlayerInventoryFeature, IUsesUserPersistentData
{
    public event Action? VersionIncreased;

    public RealmPlayer Player { get; }
    protected override Element Element => Player;

    public PlayerInventoryFeature(PlayerContext playerContext, ItemsCollection itemsCollection) : base(itemsCollection)
    {
        Player = playerContext.Player;
    }

    public void LogIn(UserData userData)
    {
        Load(Player, userData.Inventories, _itemsCollection);
    }
}

public sealed class VehicleInventoryFeature : ElementInventoryFeature, IVehicleInventoryFeature, IUsesVehiclePersistentData
{
    public RealmVehicle Vehicle { get; }
    protected override Element Element => Vehicle;
    public VehicleInventoryFeature(VehicleContext vehicleContext, ItemsCollection itemsCollection) : base(itemsCollection)
    {
        Vehicle = vehicleContext.Vehicle;
    }

    public event Action? VersionIncreased;

    public void Loaded(VehicleData vehicleData, bool preserveData = false)
    {
        Load(Vehicle, vehicleData.Inventories, _itemsCollection);
    }

    public void Unloaded()
    {

    }
}
