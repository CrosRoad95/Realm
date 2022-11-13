namespace Realm.Server.Elements;

public class RPGBlip : Blip
{
    public bool IsVariant { get; private set; }
    public RPGBlip() : base(Vector3.Zero, BlipIcon.Marker, 250, 0)
    {
    }

    [NoScriptAccess]
    public void SetIsVariant()
    {
        IsVariant = true;
    }
}
