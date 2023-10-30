namespace RealmCore.Server.Components.Elements;

public class WorldObjectComponent : WorldObject, IElementComponent
{
    public WorldObjectComponent(ObjectModel model, Vector3 position) : base(model, position)
    {
    }

    public Entity? Entity { get; set; }
}
