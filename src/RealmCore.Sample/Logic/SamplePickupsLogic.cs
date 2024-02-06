using RealmCore.Sample.Components.Jobs;
using RealmCore.Server.Concepts.Sessions;

namespace RealmCore.Sample.Logic;

internal class SamplePickupsLogic
{
    private readonly ChatBox _chatBox;

    public SamplePickupsLogic(IElementFactory elementFactory, ChatBox chatBox)
    {
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
                    if(player.Sessions.TryGetSession(out FractionSession fractionSession))
                    {
                        fractionSession.End();
                        _chatBox.OutputTo(player, $"Session ended in: {fractionSession.Elapsed}");
                        player.Sessions.EndSession(fractionSession);
                    }
                    else
                    {
                        var fractionSessionComponent = player.Sessions.BeginSession<FractionSession>();
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

                    var sessionComponent = player.Sessions.GetSession<JobSession>();
                    if (sessionComponent != null || sessionComponent is not JobSession)
                        return;

                    if (player.Sessions.TryGetSession(out JobSession jobSession))
                    {
                        player.Sessions.EndSession(jobSession);
                        var elapsed = jobSession.Elapsed;
                        _chatBox.OutputTo(player, $"Job ended in: {elapsed.Hours:X2}:{elapsed.Minutes:X2}:{elapsed.Seconds:X2}, completed objectives: {jobSession.CompletedObjectives}");
                        var elapsedSeconds = (ulong)jobSession.Elapsed.Seconds;
                        if(elapsedSeconds > 0)
                            player.JobStatistics.AddTimePlayed(jobSession.JobId, (ulong)jobSession.Elapsed.Seconds);
                    }
                    else
                    {
                        var jobSessionComponent = player.Sessions.BeginSession<TestJob>();
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
                pickup.CollisionDetection.AddRule<MustNotHaveComponentRule<AttachedElementComponent>>();
                pickup.AddOpenGuiLogic<TestGui>();
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
