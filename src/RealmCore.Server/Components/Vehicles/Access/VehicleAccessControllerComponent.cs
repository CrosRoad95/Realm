namespace RealmCore.Server.Components.Vehicles.Access;

public abstract class VehicleAccessControllerComponent : Component
{
    protected abstract bool CanEnter(Ped ped, Vehicle vehicle);

    protected override void Load()
    {
        if (Entity.Tag != EntityTag.Vehicle)
            throw new NotSupportedException("This component only works on vehicles.");

        if (Entity.Components.OfType<VehicleAccessControllerComponent>().Where(x => x != this).Any())
            throw new InvalidOperationException("Vehicle already have vehicle access controller component");
    }

    public bool HasAccess(Entity entity)
    {
        ThrowIfDisposed();

        switch (entity.Tag)
        {
            case EntityTag.Player:
                {
                    var player = entity.Player;
                    var vehicle = (Vehicle)Entity.Element;
                    return CanEnter(player, vehicle);
                }
            default:
                throw new InvalidOperationException();
        }
    }
}
