using RealmCore.Sample.Components.Jobs;

namespace RealmCore.Sample.Logic;

internal class SamplePickupsLogic
{
    private readonly IElementFactory _elementFactory;
    private readonly ChatBox _chatBox;

    public SamplePickupsLogic(IElementFactory entityFactory, ChatBox chatBox)
    {
        _elementFactory = entityFactory;
        _chatBox = chatBox;
        entityFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(Element element)
    {
        {
        if (element is RealmPickup pickup && pickup.Components.TryGetComponent(out NameComponent nameComponent) && nameComponent.Name.StartsWith("fractionTestPickup"))
            {
                pickup.AddRule(new MustBePlayerInFractionRule(1));

                pickup.Entered += async (enteredPickup, element) =>
                {
                    var player = (RealmPlayer)element;
                    var sessionComponent = player.Components.GetComponent<SessionComponent>();
                    if (sessionComponent != null && sessionComponent is not FractionSessionComponent)
                        return;

                    if (player.Components.HasComponent<FractionSessionComponent>())
                    {
                        var fractionSessionComponent = player.Components.GetRequiredComponent<FractionSessionComponent>();
                        fractionSessionComponent.End();
                        _chatBox.OutputTo(player, $"Session ended in: {fractionSessionComponent.Elapsed}");
                        player.Components.DestroyComponent(fractionSessionComponent);
                    }
                    else
                    {
                        var fractionSessionComponent = player.Components.AddComponent<FractionSessionComponent>();
                        _chatBox.OutputTo(player, $"Session started");
                        fractionSessionComponent.Start();
                    }
                };
            }
        }

        {
            if (element is RealmPickup pickup && pickup.Components.TryGetComponent(out NameComponent nameComponent) && nameComponent.Name.StartsWith("jobTestPickup"))
            {
                pickup.Entered += (enteredPickup, element) =>
                {
                    var player = (RealmPlayer)element;

                    var sessionComponent = player.Components.GetComponent<SessionComponent>();
                    if (sessionComponent != null && sessionComponent is not JobSessionComponent)
                        return;

                    if (player.Components.HasComponent<JobSessionComponent>())
                    {
                        var jobSessionComponent = player.Components.GetRequiredComponent<JobSessionComponent>();
                        jobSessionComponent.End();
                        var elapsed = jobSessionComponent.Elapsed;
                        _chatBox.OutputTo(player, $"Job ended in: {elapsed.Hours:X2}:{elapsed.Minutes:X2}:{elapsed.Seconds:X2}, completed objectives: {jobSessionComponent.CompletedObjectives}");
                        player.Components.GetRequiredComponent<JobStatisticsComponent>().AddTimePlayed(jobSessionComponent.JobId, (ulong)jobSessionComponent.Elapsed.Seconds);
                        player.Components.DestroyComponent(jobSessionComponent);
                    }
                    else
                    {
                        var jobSessionComponent = player.Components.AddComponent(new TestJobComponent(_elementFactory));
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
                pickup.AddRule<MustBePlayerOnFootOnlyRule>();
                pickup.AddRule<MustNotHaveComponent<AttachedElementComponent>>();
                pickup.AddOpenGuiLogic<TestWindowComponent>();
            }
        }

        {
            if (element is RealmPickup pickup && pickup.Components.TryGetComponent(out NameComponent nameComponent) && nameComponent.Name.StartsWith("exampleShopPickup"))
            {
                pickup.AddRule<MustBePlayerOnFootOnlyRule>();
                pickup.AddOpenGuiLogic<TestShopGuiComponent, InventoryGuiComponent>();
            }
        }

        {
            if (element is RealmPickup pickup && pickup.Components.TryGetComponent(out NameComponent nameComponent) && nameComponent.Name.StartsWith("testMarker"))
            {
                pickup.AddRule<MustBePlayerOnFootOnlyRule>();
                pickup.Entered += (enteredPickup, element) =>
                {
                    _chatBox.OutputTo((RealmPlayer)element, $"Entered marker");
                };
                pickup.Left += (leftPickup, element) =>
                {
                    _chatBox.OutputTo((RealmPlayer)element, $"Left marker");
                };
            }
        }
    }
}
