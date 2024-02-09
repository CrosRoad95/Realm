namespace RealmCore.Server.Modules.Inventories;

public class InventoryNotEnoughSpaceException : Exception
{
    public decimal InventorySize { get; }
    public decimal RequiredSpace { get; }

    public InventoryNotEnoughSpaceException(decimal inventorySize, decimal requiredSpace) : base("Inventory not have enough space.")
    {
        InventorySize = inventorySize;
        RequiredSpace = requiredSpace;
    }
}
