namespace RealmCore.Server.Modules.Elements;

public class RealmBlip : Blip, IElementName
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();

    public RealmBlip(Vector3 position, BlipIcon icon, ushort visibleDistance = 16000, short ordering = 0) : base(position, icon, visibleDistance, ordering)
    {
    }

    public string? ElementName { get; set; }
}
