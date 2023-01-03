using Realm.Interfaces.Server;
using SlipeServer.Server.Elements;

namespace Realm.Domain.Components.Elements;

public class BlipElementComponent : ElementComponent
{
    [Inject]
    private IRPGServer _rpgServer { get; set; } = default!;

    protected readonly Blip _blip;
    protected readonly Entity? _createForEntity;

    public override Element Element => _blip;

    public BlipElementComponent(Blip blip, Entity? createForEntity = null)
    {
        _blip = blip;
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
        _blip.Destroy();
    }

    public override Task Load()
    {
        _blip.Position = Entity.Transform.Position;
        if (_createForEntity != null)
        {
            var player = _createForEntity.GetRequiredComponent<PlayerElementComponent>().Player;
            _blip.CreateFor(player);
        }
        else
            _rpgServer.AssociateElement(new ElementHandle(_blip));
        Entity.Destroyed += HandleDestroyed;
        return Task.CompletedTask;
    }
}
