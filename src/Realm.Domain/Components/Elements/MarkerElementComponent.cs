namespace Realm.Domain.Components.Elements;

public class MarkerElementComponent : ElementComponent
{

    [Inject]
    private IRPGServer _rpgServer { get; set; } = default!;

    protected readonly Marker _marker;

    public override Element Element => _marker;

    public MarkerElementComponent(Marker marker)
    {
        _marker = marker;
    }

    private void HandleDestroyed(Entity entity)
    {
        _marker.Destroy();
    }

    public override Task Load()
    {
        base.Load();
        Entity.Transform.Bind(_marker);
        _rpgServer.AssociateElement(new ElementHandle(_marker));
        Entity.Destroyed += HandleDestroyed;
        return Task.CompletedTask;
    }

}
