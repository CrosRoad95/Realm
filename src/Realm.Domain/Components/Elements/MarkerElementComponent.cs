namespace Realm.Domain.Components.Elements;

public class MarkerElementComponent : ElementComponent
{

    [Inject]
    private IRPGServer _rpgServer { get; set; } = default!;

    protected readonly Marker _marker;
    protected readonly Entity? _createForEntity;

    public override Element Element => _marker;

    public MarkerElementComponent(Marker marker, Entity? createForEntity = null)
    {
        _marker = marker;
        _createForEntity = createForEntity;
        if(_createForEntity != null)
            _createForEntity.Destroyed += HandleCreateForEntityDestroyed;
    }

    private void HandleCreateForEntityDestroyed(Entity entity)
    {
        Destroy();
    }

    private void HandleDestroyed(Entity entity)
    {
        _marker.Destroy();
    }

    public override Task Load()
    {
        base.Load();
        Entity.Transform.Bind(_marker);
        if(_createForEntity != null)
        {
            var player = _createForEntity.GetRequiredComponent<PlayerElementComponent>().Player;
            _marker.CreateFor(player);
        }
        else
            _rpgServer.AssociateElement(new ElementHandle(_marker));
        Entity.Destroyed += HandleDestroyed;
        return Task.CompletedTask;
    }

}
