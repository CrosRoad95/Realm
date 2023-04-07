namespace Realm.Server.Exceptions;

public class InventoryDoNotEnoughSpaceException : Exception
{
    public InventoryDoNotEnoughSpaceException() : base("Inventory not have enough space.") { }
}
