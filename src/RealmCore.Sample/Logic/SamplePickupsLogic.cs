namespace RealmCore.Console.Logic;

internal class SamplePickupsLogic
{
    private readonly IECS _ecs;
    private readonly IEntityFactory _entityFactory;
    private readonly ChatBox _chatBox;

    public SamplePickupsLogic(IECS ecs, IEntityFactory entityFactory, ChatBox chatBox)
    {
        _ecs = ecs;
        _entityFactory = entityFactory;
        _chatBox = chatBox;
        _ecs.EntityCreated += EntityCreated;
    }

    private void EntityCreated(Entity entity)
    {
        if (entity.HasComponent<PickupTagComponent>() && entity.Name.StartsWith("fractionTestPickup"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.AddRule(new MustBePlayerInFractionRule(1));
            pickupElementComponent.EntityRuleFailed = (entity, rule) =>
            {
                var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
                _chatBox.OutputTo(entity, $"No permissions, rule: {rule.GetType().Name}");
            };
            pickupElementComponent.EntityEntered = async (enteredPickup, entity) =>
            {
                if (!entity.HasComponent<PlayerTagComponent>())
                    return;

                var sessionComponent = entity.GetComponent<SessionComponent>();
                if (sessionComponent != null && sessionComponent is not FractionSessionComponent)
                    return;

                var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
                if (entity.HasComponent<FractionSessionComponent>())
                {
                    var fractionSessionComponent = entity.GetRequiredComponent<FractionSessionComponent>();
                    fractionSessionComponent.End();
                    _chatBox.OutputTo(entity, $"Session ended in: {fractionSessionComponent.Elapsed}");
                    entity.DestroyComponent(fractionSessionComponent);
                }
                else
                {
                    var fractionSessionComponent = entity.AddComponent<FractionSessionComponent>();
                    _chatBox.OutputTo(entity, $"Session started");
                    fractionSessionComponent.Start();
                }
            };
        }

        if (entity.HasComponent<PickupTagComponent>() && entity.Name.StartsWith("jobTestPickup"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.EntityEntered = (enteredPickup, entity) =>
            {
                if (!entity.HasComponent<PlayerTagComponent>())
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
                    _chatBox.OutputTo(entity, $"Job ended in: {elapsed.Hours:X2}:{elapsed.Minutes:X2}:{elapsed.Seconds:X2}, completed objectives: {jobSessionComponent.CompletedObjectives}");
                    entity.GetRequiredComponent<JobStatisticsComponent>().AddTimePlayed(jobSessionComponent.JobId, (ulong)jobSessionComponent.Elapsed.Seconds);
                    entity.DestroyComponent(jobSessionComponent);
                }
                else
                {
                    var jobSessionComponent = entity.AddComponent<TestJobComponent>();
                    _chatBox.OutputTo(entity, $"Job started");
                    jobSessionComponent.Start();

                    jobSessionComponent.CompletedAllObjectives += async e =>
                    {
                        _chatBox.OutputTo(entity, $"All objectives completed!");
                        await Task.Delay(2000);
                        jobSessionComponent.CreateObjectives();
                    };
                    jobSessionComponent.CreateObjectives();
                }
            };
        }

        if (entity.HasComponent<PickupTagComponent>() && entity.Name.StartsWith("withText3d"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.AddRule<MustBePlayerOnFootOnlyRule>();
            pickupElementComponent.AddRule<MustNotHaveComponent<AttachedEntityComponent>>();
            pickupElementComponent.AddOpenGuiLogic<TestWindowComponent>();
        }

        if (entity.HasComponent<PickupTagComponent>() && entity.Name.StartsWith("exampleShopPickup"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<PickupElementComponent>();
            pickupElementComponent.AddRule<MustBePlayerOnFootOnlyRule>();
            pickupElementComponent.AddOpenGuiLogic<TestShopGuiComponent, InventoryGuiComponent>();
        }

        if (entity.HasComponent<MarkerTagComponent>() && entity.Name.StartsWith("testMarker"))
        {
            var pickupElementComponent = entity.GetRequiredComponent<MarkerElementComponent>();
            pickupElementComponent.AddRule<MustBePlayerOnFootOnlyRule>();
            pickupElementComponent.EntityEntered = (markerElementComponent, enteredPickup, entity) =>
            {
                _chatBox.OutputTo(entity, $"Entered marker");
            };
            pickupElementComponent.EntityLeft = (markerElementComponent, leftPickup, entity) =>
            {
                _chatBox.OutputTo(entity, $"Left marker");
            };
        }
    }
}
