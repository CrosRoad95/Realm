namespace RealmCore.Server.Components.Elements;

public class PedElementComponent : Ped, IElementComponent
{
    public PedElementComponent(PedModel model, Vector3 position) : base(model, position)
    {
    }

    public Entity Entity { get; set; }
}
