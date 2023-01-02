namespace Realm.Domain.Components.Common;

public class LastPositionComponent : Component
{
    public Vector3? LastPosition { get; set; }
    public Vector3? LastRotation { get; set; }
    public LastPositionComponent()
    {
    }

    public bool TrySpawn()
    {
        if (LastPosition == null || LastRotation == null)
            return false;

        var player = Entity.InternalGetRequiredComponent<PlayerElementComponent>().Player;
        player.Spawn(LastPosition.Value, LastRotation.Value.X, player.Model, 0, 0);
        return true;
    }
}
