namespace RealmCore.Sample.Components.Vehicles;

public class VehicleForSaleComponent : ComponentLifecycle
{
    public decimal Price { get; }

    public VehicleForSaleComponent(decimal price)
    {
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price));

        Price = price;
    }

    public override void Attach()
    {
        var vehicleElementComponent = (RealmVehicle)Element;
        vehicleElementComponent.IsFrozen = true;
        vehicleElementComponent.IsLocked = true;
    }
}
