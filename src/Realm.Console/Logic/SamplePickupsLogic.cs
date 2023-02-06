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
            pickupElementComponent.EntityEntered = async entity =>
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
                    entity.GetRequiredComponent<JobStatisticsComponent>().AddTimePlayed(1, (ulong)jobSessionComponent.Elapsed.Seconds);
                    entity.DestroyComponent(jobSessionComponent);
                }
                else
                {
                    var jobSessionComponent = await entity.AddComponentAsync(new TestJobComponent());
                    playerElementComponent.SendChatMessage($"Job started");
                    jobSessionComponent.Start();

                    var createObjectives = () =>
                    {
                        var objective = jobSessionComponent.AddObjective(new MarkerEnterObjective(new Vector3(383.6543f, -82.01953f, 3.914598f)));
                        objective.AddBlip(BlipIcon.North, _entityFactory);
                        objective.Completed += e =>
                        {
                            e.Entity.GetRequiredComponent<JobStatisticsComponent>().AddPoints(1, 1);
                            e.Entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Entered marker, objectives left: {jobSessionComponent.Objectives.Count()}");
                        };

                        var objectEntity = _entityFactory.CreateObject(SlipeServer.Server.Enums.ObjectModel.Gunbox, new Vector3(379.00f, -102.77f, 1.24f), Vector3.Zero);
                        objectEntity.AddComponent(new LiftableWorldObjectComponent());
                        var objective2 = jobSessionComponent.AddObjective(new TransportEntityObjective(objectEntity, new Vector3(379.00f, -112.77f, 2.0f)));
                        objective2.Completed += e =>
                        {
                            e.Entity.GetRequiredComponent<JobStatisticsComponent>().AddPoints(1, 2);
                            e.Entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Box delivered, objectives left: {jobSessionComponent.Objectives.Count()}");
                        };
                    };

                    jobSessionComponent.CompletedAllObjectives += async e =>
                    {
                        playerElementComponent.SendChatMessage($"All objectives completed!");
                        await Task.Delay(2000);
                        createObjectives();
                    };
                    createObjectives();
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
