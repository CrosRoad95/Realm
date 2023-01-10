using Realm.Domain.Rules;
using Realm.Server.Extensions;

namespace Realm.Console.Logic;

internal class SamplePickupsLogic
{
    private readonly ECS _ecs;
    private readonly IEntityFactory _entityFactory;

    public SamplePickupsLogic(ECS ecs, IEntityFactory entityFactory)
    {
        _ecs = ecs;
        _entityFactory = entityFactory;
        _ecs.EntityCreated += EntityCreated;
    }

    private void EntityCreated(Entity entity)
    {
        if (entity.Tag == Entity.PickupTag && entity.Name.StartsWith("fractionTestPickup"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.EntityEntered = async entity =>
            {
                var sessionComponent = entity.GetComponent<SessionComponent>();
                if (sessionComponent != null && sessionComponent is not FractionSessionComponent)
                    return;

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
        
        if (entity.Tag == Entity.PickupTag && entity.Name.StartsWith("jobTestPickup"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.EntityEntered = async entity =>
            {
                var sessionComponent = entity.GetComponent<SessionComponent>();
                if(sessionComponent != null && sessionComponent is not JobSessionComponent)
                    return;

                var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
                if (entity.HasComponent<JobSessionComponent>())
                {
                    var jobSessionComponent = entity.GetRequiredComponent<JobSessionComponent>();
                    jobSessionComponent.End();
                    playerElementComponent.SendChatMessage($"Session ended in: {jobSessionComponent.Elapsed}");
                    entity.DestroyComponent(jobSessionComponent);
                }
                else
                {
                    var jobSessionComponent = await entity.AddComponentAsync(new TestJobComponent());
                    playerElementComponent.SendChatMessage($"Session started");
                    jobSessionComponent.Start();

                    var marker = _entityFactory.CreateMarker(MarkerType.Arrow, new Vector3(383.6543f, -82.01953f, 3.914598f));
                    var collisionSphere = _entityFactory.CreateCollisionSphere(new Vector3(383.6543f, -82.01953f, 3.914598f), 2);
                    var objective = jobSessionComponent.AddObjective(new MarkerEnterObjective(marker, collisionSphere));
                    objective.Completed += e =>
                    {
                        e.Entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage("kk");
                    };
                }
            };
        }

        if (entity.Tag == Entity.PickupTag && entity.Name.StartsWith("withText3d"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.AddRule(new MustBePlayerOnFootOnlyRule());
            pickupElementComponent.AddOpenGuiLogic<TestWindowComponent>();
        }

        if (entity.Tag == Entity.PickupTag && entity.Name.StartsWith("exampleShopPickup"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.AddRule(new MustBePlayerOnFootOnlyRule());
            pickupElementComponent.AddOpenGuiLogic<TestShopGuiComponent>();
        }
    }
}
