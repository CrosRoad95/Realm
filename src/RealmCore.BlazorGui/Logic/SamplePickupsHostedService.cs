using RealmCore.BlazorGui.Concepts.Jobs;
using RealmCore.Server.Modules.Pickups;
using RealmCore.Server.Modules.Players.Fractions;

namespace RealmCore.BlazorGui.Logic;

internal class SamplePickupsHostedService : IHostedService
{
    private readonly IElementFactory _elementFactory;
    private readonly ChatBox _chatBox;

    public SamplePickupsHostedService(IElementFactory elementFactory, ChatBox chatBox)
    {
        _elementFactory = elementFactory;
        _chatBox = chatBox;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _elementFactory.ElementCreated += HandleElementCreated;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _elementFactory.ElementCreated -= HandleElementCreated;

        return Task.CompletedTask;
    }

    private void HandleElementCreated(IElementFactory elementFactory, Element element)
    {
        {
            if (element is RealmPickup pickup && pickup.ElementName != null && pickup.ElementName.StartsWith("fractionTestPickup"))
            {
                pickup.CollisionShape.ElementEntered += async (collisionShape, e) =>
                {
                    var player = (RealmPlayer)element;
                    if (player.Sessions.TryGet(out FractionSession pendingFractionSession))
                    {
                        pendingFractionSession.Dispose();
                        _chatBox.OutputTo(player, $"Session ended in: {pendingFractionSession.Elapsed}");
                        player.Sessions.TryEnd(pendingFractionSession);
                    }
                    else
                    {
                        var fractionSession = player.Sessions.Begin<FractionSession>();
                        _chatBox.OutputTo(player, $"Session started");
                        fractionSession.TryStart();
                    }
                };
            }
        }

        {
            if (element is RealmPickup pickup && pickup.ElementName != null && pickup.ElementName.StartsWith("jobTestPickup"))
            {
                pickup.CollisionShape.ElementEntered += (collisionShape, e) =>
                {
                    if (e.Element is not RealmPlayer player)
                        return;

                    if (player.Sessions.TryGet(out JobSession jobSession))
                    {
                        player.Sessions.TryEnd(jobSession);
                        var elapsed = jobSession.Elapsed;
                        _chatBox.OutputTo(player, $"Job ended in: {elapsed.Hours:X2}:{elapsed.Minutes:X2}:{elapsed.Seconds:X2}, completed objectives: {jobSession.CompletedObjectives}");
                        var elapsedSeconds = (ulong)jobSession.Elapsed.Seconds;
                        if (elapsedSeconds > 0)
                            player.JobStatistics.AddTimePlayed(jobSession.JobId, (ulong)jobSession.Elapsed.Seconds);
                    }
                    else
                    {
                        var testJob = player.Sessions.Begin<TestJob>();
                        _chatBox.OutputTo(player, $"Job started");
                        testJob.TryStart();

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
                pickup.CollisionShape.AddOpenGuiLogic<TestGui>();
            }
        }

        {
            if (element is RealmPickup pickup && pickup.ElementName != null && pickup.ElementName.StartsWith("testMarker"))
            {
                pickup.CollisionShape.ElementEntered += (enteredPickup, args) =>
                {
                    _chatBox.OutputTo((RealmPlayer)args.Element, $"Entered marker");
                };
                pickup.CollisionShape.ElementLeft += (leftPickup, args) =>
                {
                    _chatBox.OutputTo((RealmPlayer)args.Element, $"Left marker");
                };
            }
        }
    }
}
