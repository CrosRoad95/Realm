namespace RealmCore.Sample.Logic;

public class RealmVehicleForSale : RealmVehicle
{
    public RealmVehicleForSale(IServiceProvider serviceProvider, ushort model, Vector3 position, decimal price) : base(serviceProvider, model, position)
    {
        Price = price;
    }

    private readonly object _lock = new();
    public decimal Price { get; }
    public bool Sold { get; private set; }

    public bool TrySell()
    {
        lock (_lock)
        {
            if (Sold)
                return false;
            Sold = true;
            return true;
        }
    }
}