namespace Realm.Domain.Components.Vehicles;

public abstract class VehicleAccessControllerComponent : Component
{
    protected abstract bool CanEnter(Ped ped, Vehicle vehicle);

    protected override void Load()
    {
        if (Entity.Tag != Entity.EntityTag.Vehicle)
            throw new NotSupportedException("This component only works on vehicles.");

        if (Entity.Components.OfType<VehicleAccessControllerComponent>().Where(x => x != this).Any())
            throw new InvalidOperationException("Vehicle already have vehicle access controller component");
    }

    public bool HasAccess(Entity entity)
    {
        switch (entity.Tag)
        {
            case Entity.EntityTag.Player:
                {
                    var player = entity.GetRequiredComponent<PlayerElementComponent>().Player;
                    var vehicle = Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle;
                    return CanEnter(player, vehicle);
                }
            default:
                throw new InvalidOperationException();
        }
    }
}
