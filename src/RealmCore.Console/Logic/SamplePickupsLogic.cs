using RealmCore.Server.Enums;
using RealmCore.Server.Extensions;
using RealmCore.Server.Rules;

namespace RealmCore.Console.Logic;

internal class SamplePickupsLogic
{
    private readonly IECS _ecs;
    private readonly IEntityFactory _entityFactory;

    public SamplePickupsLogic(IECS ecs, IEntityFactory entityFactory)
    {
        _ecs = ecs;
        _entityFactory = entityFactory;
        _ecs.EntityCreated += EntityCreated;
    }

    private void EntityCreated(Entity entity)
    {
        if (entity.Tag == EntityTag.Pickup && entity.Name.StartsWith("fractionTestPickup"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.AddRule(new MustBePlayerInFractionRule(1));
            pickupElementComponent.EntityRuleFailed = (entity, rule) =>
            {
                if (entity.Tag == EntityTag.Player)
                {
                    var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
                    playerElementComponent.SendChatMessage($"No permissions, rule: {rule.GetType().Name}");
                }

            };
            pickupElementComponent.EntityEntered = async entity =>
            {
                if (entity.Tag != EntityTag.Player)
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
                    var fractionSessionComponent = entity.AddComponent<FractionSessionComponent>();
                    playerElementComponent.SendChatMessage($"Session started");
                    fractionSessionComponent.Start();
                }
            };
        }

        if (entity.Tag == EntityTag.Pickup && entity.Name.StartsWith("jobTestPickup"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.EntityEntered = entity =>
            {
                if (entity.Tag != EntityTag.Player)
                    return;

                var sessionComponent = entity.GetComponent<SessionComponent>();
                if (sessionComponent != null && sessionComponent is not JobSessionComponent)
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

        if (entity.Tag == EntityTag.Pickup && entity.Name.StartsWith("withText3d"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.AddRule<MustBePlayerOnFootOnlyRule>();
            pickupElementComponent.AddRule<MustNotHaveComponent<AttachedEntityComponent>>();
            pickupElementComponent.AddOpenGuiLogic<TestWindowComponent>();
        }

        if (entity.Tag == EntityTag.Pickup && entity.Name.StartsWith("exampleShopPickup"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.AddRule<MustBePlayerOnFootOnlyRule>();
            pickupElementComponent.AddOpenGuiLogic<TestShopGuiComponent, InventoryGuiComponent>();
        }

        if (entity.Tag == EntityTag.Marker && entity.Name.StartsWith("testMarker"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<MarkerElementComponent>();
            pickupElementComponent.AddRule<MustBePlayerOnFootOnlyRule>();
            pickupElementComponent.EntityEntered = entity =>
            {
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Entered marker");
            };
            pickupElementComponent.EntityLeft = entity =>
            {
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Left marker");
            };
        }
    }
}
