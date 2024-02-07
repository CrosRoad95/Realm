namespace RealmCore.Server.Elements;

public class RealmBlip : Blip, IElementName
{
    public RealmBlip(Vector3 position, BlipIcon icon, ushort visibleDistance = 16000, short ordering = 0) : base(position, icon, visibleDistance, ordering)
    {
    }

    public string? ElementName { get; set; }
}
