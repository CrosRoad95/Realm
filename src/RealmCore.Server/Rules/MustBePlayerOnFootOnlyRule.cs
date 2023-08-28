using RealmCore.ECS;

namespace RealmCore.Server.Rules;

public sealed class MustBePlayerOnFootOnlyRule : IEntityRule
{
    public bool Check(Entity entity)
    {
        if (!entity.HasComponent<PlayerTagComponent>())
            return false;

        var player = entity.GetPlayer();
        if (player.HasJetpack)
            return false;

        if (player.VehicleAction != VehicleAction.None)
            return false;

        return player.Vehicle == null;
    }
}
