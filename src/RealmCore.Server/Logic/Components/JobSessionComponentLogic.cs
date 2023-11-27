using RealmCore.Server.Components.Players.Jobs;
using RealmCore.Server.Concepts.Sessions;

namespace RealmCore.Server.Logic.Components;

internal sealed class JobSessionComponentLogic
{
    private readonly ILogger<JobSessionComponentLogic> _logger;

    public JobSessionComponentLogic(ILogger<JobSessionComponentLogic> logger, IPlayersService playersService)
    {
        _logger = logger;
        playersService.PlayerLoaded += HandlePlayerLoaded;
    }

    private void HandlePlayerLoaded(RealmPlayer player)
    {
        player.Sessions.Started += HandleStarted;
        player.Sessions.Ended += HandleEnded;
    }

    private void HandleStarted(IPlayerSessionsService sessionService, Session session)
    {
        if(session is JobSession jobSession)
        {
            jobSession.ObjectiveAdded += HandleObjectiveAdded;
        }
    }

    private void HandleEnded(IPlayerSessionsService sessionService, Session session)
    {
        if (session is JobSession jobSession)
        {
            jobSession.ObjectiveAdded -= HandleObjectiveAdded;
        }
    }

    private void HandleObjectiveAdded(JobSession jobSessionComponent, Objective objective)
    {
        try
        {
            objective.LoadInternal(jobSessionComponent.Player);
        }
        catch(Exception ex)
        {
            objective.Dispose();
            _logger.LogError(ex, "Failed to load objective.");
            throw;
        }
    }
}
