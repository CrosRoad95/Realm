using RealmCore.Sample.Concepts.Gui;
using RealmCore.Sample.Concepts.Jobs;

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
        if (element is RealmPickup pickup && pickup.ElementName != null && pickup.ElementName.StartsWith("fractionTestPickup"))
            {
                pickup.CollisionDetection.AddRule(new MustBePlayerInFractionRule(1));

                pickup.CollisionDetection.Entered += async (enteredPickup, element) =>
                {
                    var player = (RealmPlayer)element;
                    if(player.Sessions.TryGetSession(out FractionSession pendingFractionSession))
                    {
                        pendingFractionSession.End();
                        _chatBox.OutputTo(player, $"Session ended in: {pendingFractionSession.Elapsed}");
                        player.Sessions.EndSession(pendingFractionSession);
                    }
                    else
                    {
                        var fractionSession = player.Sessions.BeginSession<FractionSession>();
                        _chatBox.OutputTo(player, $"Session started");
                        fractionSession.Start();
                    }
                };
            }
        }

        {
            if (element is RealmPickup pickup && pickup.ElementName != null && pickup.ElementName.StartsWith("jobTestPickup"))
            {
                pickup.CollisionDetection.Entered += (enteredPickup, element) =>
                {
                    var player = (RealmPlayer)element;

                    var jobSession = player.Sessions.GetSession<JobSession>();
                    if (jobSession != null || jobSession is not JobSession)
                        return;

                    if (player.Sessions.TryGetSession(out JobSession pendingJobSession))
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
                        var testJob = player.Sessions.BeginSession<TestJob>();
                        _chatBox.OutputTo(player, $"Job started");
                        testJob.Start();

                        testJob.CompletedAllObjectives += async e =>
                        {
                            _chatBox.OutputTo(player, $"All objectives completed!");
                            await Task.Delay(2000);
                            testJob.CreateObjectives();
                        };
                        testJob.CreateObjectives();
                    }
                };
            }
        }

        {
            if (element is RealmPickup pickup && pickup.ElementName != null && pickup.ElementName.StartsWith("withText3d"))
            {
                pickup.CollisionDetection.AddRule<MustBePlayerOnFootOnlyRule>();
                pickup.CollisionDetection.AddRule<MustHaveNoWorldObjectAttachedRule>();
                pickup.AddOpenGuiLogic<TestGui>();
            }
        }

        {
            if (element is RealmPickup pickup && pickup.ElementName != null && pickup.ElementName.StartsWith("testMarker"))
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
