namespace RealmCore.Server.Rules;

public sealed class MustBePlayerOnFootOnlyRule : IEntityRule
{
    public bool Check(Entity entity)
    {
        if (!entity.HasComponent<PlayerTagComponent>())
            return false;

        if(entity.Player.HasJetpack)
            return false;

        if (entity.Player.VehicleAction != VehicleAction.None)
            return false;

        return entity.Player.Vehicle == null;
    }
}
