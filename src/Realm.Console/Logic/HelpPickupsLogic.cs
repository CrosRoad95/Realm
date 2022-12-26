using Realm.Domain.Components.World;
using SlipeServer.Resources.Text3d;
using SlipeServer.Server.Elements;

namespace Realm.Console.Logic;

internal class HelpPickupsLogic
{
    private readonly ECS _ecs;

    public HelpPickupsLogic(ECS ecs)
    {
        _ecs = ecs;
        _ecs.EntityCreated += EntityCreated;
    }

    private void EntityCreated(Entity entity)
    {
        if (entity.Tag == Entity.PickupTag && entity.Name.StartsWith("help"))
        {
            entity.AddComponent(new Text3dComponent("Podejdz aby uzykac pomoc", new Vector3(0, 0, 0.75f)));
        }
    }
}
