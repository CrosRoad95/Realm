using RealmCore.Server.Components.Players.Jobs;

namespace RealmCore.Server.Logic.Components;

internal sealed class JobSessionComponentLogic : ComponentLogic<JobSessionComponent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public JobSessionComponentLogic(IElementFactory elementFactory, IServiceProvider serviceProvider, ILogger logger) : base(elementFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override void ComponentAdded(JobSessionComponent jobSessionComponent)
    {
        jobSessionComponent.ObjectiveAdded += HandleObjectiveAdded;
    }

    protected override void ComponentDetached(JobSessionComponent jobSessionComponent)
    {
        jobSessionComponent.ObjectiveAdded -= HandleObjectiveAdded;
    }

    private void HandleObjectiveAdded(JobSessionComponent jobSessionComponent, Objective objective)
    {
        objective.LoadInternal((RealmPlayer)jobSessionComponent.Element);
    }
}
