using Realm.Domain.Interfaces;
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
        if (entity.Tag == Entity.EntityTag.Pickup && entity.Name.StartsWith("fractionTestPickup"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.AddRule(new MustBePlayerInFractionRule(1));
            pickupElementComponent.EntityRuleFailed = (entity, rule) =>
            {
                var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
                playerElementComponent.SendChatMessage($"No permissions, rule: {rule.GetType().Name}");

            };
            pickupElementComponent.EntityEntered = async entity =>
            {
                if (entity.Tag != Entity.EntityTag.Player)
                    return;

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
        
        if (entity.Tag == Entity.EntityTag.Pickup && entity.Name.StartsWith("jobTestPickup"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.EntityEntered = entity =>
            {
                if (entity.Tag != Entity.EntityTag.Player)
                    return;

                var sessionComponent = entity.GetComponent<SessionComponent>();
                if(sessionComponent != null && sessionComponent is not JobSessionComponent)
                    return;

                var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
                if (entity.HasComponent<JobSessionComponent>())
                {
                    var jobSessionComponent = entity.GetRequiredComponent<JobSessionComponent>();
                    jobSessionComponent.End();
                    var elapsed = jobSessionComponent.Elapsed;
                    playerElementComponent.SendChatMessage($"Job ended in: {elapsed.Hours:X2}:{elapsed.Minutes:X2}:{elapsed.Seconds:X2}, completed objectives: {jobSessionComponent.CompletedObjectives}");
                    entity.GetRequiredComponent<JobStatisticsComponent>().AddTimePlayed(jobSessionComponent.JobId, (ulong)jobSessionComponent.Elapsed.Seconds);
                    entity.DestroyComponent(jobSessionComponent);
                }
                else
                {
                    var jobSessionComponent = entity.AddComponent<TestJobComponent>();
                    playerElementComponent.SendChatMessage($"Job started");
                    jobSessionComponent.Start();

                    jobSessionComponent.CompletedAllObjectives += async e =>
                    {
                        playerElementComponent.SendChatMessage($"All objectives completed!");
                        await Task.Delay(2000);
                        jobSessionComponent.CreateObjectives();
                    };
                    jobSessionComponent.CreateObjectives();
                }
            };
        }

        if (entity.Tag == Entity.EntityTag.Pickup && entity.Name.StartsWith("withText3d"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.AddRule(new MustBePlayerOnFootOnlyRule());
            pickupElementComponent.AddOpenGuiLogic<TestWindowComponent>();
        }

        if (entity.Tag == Entity.EntityTag.Pickup && entity.Name.StartsWith("exampleShopPickup"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.AddRule(new MustBePlayerOnFootOnlyRule());
            pickupElementComponent.AddOpenGuiLogic<TestShopGuiComponent, InventoryGuiComponent>();
        }
    }
}
