using RealmCore.Sample.Components.Jobs;

namespace RealmCore.Sample.Logic;

internal class SamplePickupsLogic
{
    private readonly IElementFactory _elementFactory;
    private readonly ChatBox _chatBox;

    public SamplePickupsLogic(IElementFactory elementFactory, ChatBox chatBox)
    {
        _elementFactory = elementFactory;
        _chatBox = chatBox;
        elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(Element element)
    {
        {
        if (element is RealmPickup pickup && pickup.Components.TryGetComponent(out NameComponent nameComponent) && nameComponent.Name.StartsWith("fractionTestPickup"))
            {
                pickup.CollisionDetection.AddRule(new MustBePlayerInFractionRule(1));

                pickup.CollisionDetection.Entered += async (enteredPickup, element) =>
                {
                    var player = (RealmPlayer)element;
                    var sessionComponent = player.Components.GetComponent<SessionComponent>();
                    if (sessionComponent != null && sessionComponent is not FractionSessionComponent)
                        return;

                    if (player.HasComponent<FractionSessionComponent>())
                    {
                        var fractionSessionComponent = player.GetRequiredComponent<FractionSessionComponent>();
                        fractionSessionComponent.End();
                        _chatBox.OutputTo(player, $"Session ended in: {fractionSessionComponent.Elapsed}");
                        player.DestroyComponent(fractionSessionComponent);
                    }
                    else
                    {
                        var fractionSessionComponent = player.AddComponent<FractionSessionComponent>();
                        _chatBox.OutputTo(player, $"Session started");
                        fractionSessionComponent.Start();
                    }
                };
            }
        }

        {
            if (element is RealmPickup pickup && pickup.Components.TryGetComponent(out NameComponent nameComponent) && nameComponent.Name.StartsWith("jobTestPickup"))
            {
                pickup.CollisionDetection.Entered += (enteredPickup, element) =>
                {
                    var player = (RealmPlayer)element;

                    var sessionComponent = player.Components.GetComponent<SessionComponent>();
                    if (sessionComponent != null && sessionComponent is not JobSessionComponent)
                        return;

                    if (player.HasComponent<JobSessionComponent>())
                    {
                        var jobSessionComponent = player.GetRequiredComponent<JobSessionComponent>();
                        jobSessionComponent.End();
                        var elapsed = jobSessionComponent.Elapsed;
                        _chatBox.OutputTo(player, $"Job ended in: {elapsed.Hours:X2}:{elapsed.Minutes:X2}:{elapsed.Seconds:X2}, completed objectives: {jobSessionComponent.CompletedObjectives}");
                        player.JobStatistics.AddTimePlayed(jobSessionComponent.JobId, (ulong)jobSessionComponent.Elapsed.Seconds);
                        player.DestroyComponent(jobSessionComponent);
                    }
                    else
                    {
                        var jobSessionComponent = player.AddComponent(new TestJobComponent(_elementFactory));
                        _chatBox.OutputTo(player, $"Job started");
                        jobSessionComponent.Start();

                        jobSessionComponent.CompletedAllObjectives += async e =>
                        {
                            _chatBox.OutputTo(player, $"All objectives completed!");
                            await Task.Delay(2000);
                            jobSessionComponent.CreateObjectives();
                        };
                        jobSessionComponent.CreateObjectives();
                    }
                };
            }
        }

        {
            if (element is RealmPickup pickup && pickup.Components.TryGetComponent(out NameComponent nameComponent) && nameComponent.Name.StartsWith("withText3d"))
            {
                pickup.CollisionDetection.AddRule<MustBePlayerOnFootOnlyRule>();
                pickup.CollisionDetection.AddRule<MustNotHaveComponent<AttachedElementComponent>>();
                pickup.AddOpenGuiLogic<TestWindowComponent>();
            }
        }

        {
            if (element is RealmPickup pickup && pickup.Components.TryGetComponent(out NameComponent nameComponent) && nameComponent.Name.StartsWith("exampleShopPickup"))
            {
                pickup.CollisionDetection.AddRule<MustBePlayerOnFootOnlyRule>();
                pickup.AddOpenGuiLogic<TestShopGuiComponent, InventoryGuiComponent>();
            }
        }

        {
            if (element is RealmPickup pickup && pickup.Components.TryGetComponent(out NameComponent nameComponent) && nameComponent.Name.StartsWith("testMarker"))
            {
                pickup.CollisionDetection.AddRule<MustBePlayerOnFootOnlyRule>();
                pickup.CollisionDetection.Entered += (enteredPickup, element) =>
                {
                    _chatBox.OutputTo((RealmPlayer)element, $"Entered marker");
                };
                pickup.CollisionDetection.Left += (leftPickup, element) =>
                {
                    _chatBox.OutputTo((RealmPlayer)element, $"Left marker");
                };
            }
        }
    }
}
