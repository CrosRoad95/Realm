namespace Realm.Domain.Components.Elements;

public class MarkerElementComponent : ElementComponent
{
    protected readonly Marker _marker;
    public Action<Entity>? EntityEntered { get; set; }
    public Action<Entity>? EntityLeft { get; set; }

    public override Element Element => _marker;

    public MarkerElementComponent(Marker marker)
    {
        _marker = marker;
    }

    private void HandleElementEntered(Element element)
    {
        if(EntityEntered != null)
            EntityEntered(ElementById.GetByElement(element));
    }

    private void HandleElementLeft(Element element)
    {
        if(EntityLeft != null)
            EntityLeft(ElementById.GetByElement(element));
    }

    private void HandleDestroyed(Entity entity)
    {
        _marker.Destroy();
    }

    public override Task Load()
    {
        base.Load();
        Entity.Transform.Bind(_marker);
        _marker.Position = _marker.Position;
        var server = Entity.GetRequiredService<IRPGServer>();
        server.AssociateElement(new ElementHandle(_marker));
        Entity.Destroyed += HandleDestroyed;
        return Task.CompletedTask;
    }

}
