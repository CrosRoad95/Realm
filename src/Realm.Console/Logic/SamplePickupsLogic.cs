using Realm.Domain.Components.Elements;
using Realm.Domain.Components.World;
using YamlDotNet.Core.Tokens;

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
        if (entity.Tag == Entity.PickupTag && entity.Name.StartsWith("fractionTestPickup"))
        {
            entity.AddComponent(new Text3dComponent("Example fraction pickup", new Vector3(0, 0, 0.75f)));
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.EntityEntered = async entity =>
            {
                var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
                if (entity.HasComponent<FractionSessionComponent>())
                {
                    var fractionSessionComponent = entity.GetRequiredComponent<FractionSessionComponent>();
                    fractionSessionComponent.End();
                    playerElementComponent.SendChatMessage($"Session ended in: {fractionSessionComponent.Elapsed}");
                    entity.DestroyComponent(fractionSessionComponent);
                }
                else
                {
                    var fractionSessionComponent = await entity.AddComponentAsync(new FractionSessionComponent());
                    playerElementComponent.SendChatMessage($"Session started");
                    fractionSessionComponent.Start();
                }
            };
        }

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
