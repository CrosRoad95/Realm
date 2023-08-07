namespace RealmCore.Server.Components.Vehicles;

public class VehicleFocusableComponent : Component
{
    protected override void Load()
    {
        Entity.GetRequiredComponent<ElementComponent>().AddFocusable();
    }

    protected override void Detached()
    {
        Entity.GetRequiredComponent<ElementComponent>().RemoveFocusable();
    }
}
