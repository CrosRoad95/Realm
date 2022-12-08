namespace Realm.Domain.Components;

[Serializable]
public class LastPositionComponent : IElementComponent
{
    private RPGPlayer _player = default!;

    [ScriptMember("name")]
    public string Name => "LastPosition";

    [ScriptMember("lastPosition")]
    public Vector3? LastPosition { get; set; }
    [ScriptMember("lastRotation")]
    public Vector3? LastRotation { get; set; }
    public LastPositionComponent()
    {
    }

    public LastPositionComponent(SerializationInfo info, StreamingContext context)
    {
        LastPosition = (Vector3?)info.GetValue(nameof(LastPosition), typeof(Vector3?));
        LastRotation = (Vector3?)info.GetValue(nameof(LastRotation), typeof(Vector3?));
    }

    [NoScriptAccess]
    public void SetOwner(Element element)
    {
        if (_player != null)
            throw new Exception("Component already attached to element");
        if (element is not RPGPlayer rpgPlayer)
            throw new Exception("Not supported element type, expected: RPGPlayer");
        _player = rpgPlayer;
    }

    [ScriptMember("trySpawn")]
    public bool TrySpawn()
    {
        if (LastPosition == null || LastRotation == null)
            return false;

        _player.Spawn(LastPosition.Value, LastRotation.Value);
        return true;
    }

    [NoScriptAccess]
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(LastPosition), _player.Position);
        info.AddValue(nameof(LastRotation), _player.Rotation);
    }
}
