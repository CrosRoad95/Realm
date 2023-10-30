namespace RealmCore.Server.Components.Elements;

public class BlipElementComponent : Blip, IElementComponent
{
    public BlipElementComponent(Vector3 position, BlipIcon icon, ushort visibleDistance = 16000, short ordering = 0) : base(position, icon, visibleDistance, ordering)
    {
    }

    public Entity? Entity { get; set; }
}
