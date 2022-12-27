using Realm.Domain.Components.World;

namespace Realm.Console.Logic;

internal class SamplePickupsLogic
{
    private readonly ECS _ecs;

    public SamplePickupsLogic(ECS ecs)
    {
        _ecs = ecs;
        _ecs.EntityCreated += EntityCreated;
    }

    private void EntityCreated(Entity entity)
    {
        if (entity.Tag == Entity.PickupTag && entity.Name.StartsWith("withText3d"))
        {
            entity.AddComponent(new Text3dComponent("Example text 3d", new Vector3(0, 0, 0.75f)));
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();

            pickupElementComponent.EntityEntered = entity =>
            {
                if(entity.Tag == Entity.PlayerTag)
                {
                    if(!entity.HasComponent<GuiComponent>())
                        entity.AddComponent(new TestWindowComponent());
                }
            };
            pickupElementComponent.EntityLeft = entity =>
            {
                if(entity.Tag == Entity.PlayerTag)
                {
                    if (entity.HasComponent<TestWindowComponent>())
                        entity.DestroyComponent<TestWindowComponent>();
                }
            };
        }
    }
}
