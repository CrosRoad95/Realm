namespace Realm.Domain.New;

public class LastPositionComponent : Component
{
    [ScriptMember("lastPosition")]
    public Vector3? LastPosition { get; set; }
    [ScriptMember("lastRotation")]
    public Vector3? LastRotation { get; set; }
    public LastPositionComponent()
    {
    }

    [ScriptMember("trySpawn")]
    public bool TrySpawn()
    {
        if (LastPosition == null || LastRotation == null)
            return false;

        var player = Entity.InternalGetRequiredComponent<PlayerElementCompoent>().Player;
        player.Spawn(LastPosition.Value, LastRotation.Value.X, player.Model, 0, 0);
        return true;
    }
}
