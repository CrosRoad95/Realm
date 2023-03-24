namespace Realm.Console.Components.Vehicles;

public class VehicleForSaleComponent : Component
{
    public decimal Price { get; }

    public VehicleForSaleComponent(decimal price)
    {
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price));

        Price = price;
    }

    protected override void Load()
    {
        var vehicleElementComponent = Entity.GetRequiredComponent<VehicleElementComponent>();
        vehicleElementComponent.IsFrozen = true;
        vehicleElementComponent.IsLocked = true;
        Entity.GetRequiredComponent<ElementComponent>().AddFocusable();
    }

    public override void Dispose()
    {
        Entity.GetRequiredComponent<ElementComponent>().RemoveFocusable();
        base.Dispose();
    }
}
