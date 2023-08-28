using RealmCore.Server.Components.Players.Jobs;
using RealmCore.Server.Concepts.Objectives;

namespace RealmCore.Server.Logic.Components;

internal sealed class JobSessionComponentLogic : ComponentLogic<JobSessionComponent>
{
    private readonly IEntityFactory _entityFactory;
    private readonly ILogger _logger;

    public JobSessionComponentLogic(IEntityEngine entityEngine, IEntityFactory entityFactory, ILogger logger) : base(entityEngine)
    {
        _entityFactory = entityFactory;
        _logger = logger;
    }

    protected override void ComponentAdded(JobSessionComponent jobSessionComponent)
    {
        base.ComponentAdded(jobSessionComponent);
        jobSessionComponent.ObjectiveAdded += HandleObjectiveAdded;
    }

    private void HandleObjectiveAdded(JobSessionComponent jobSessionComponent, Objective objective)
    {
        objective.LoadInternal(_entityFactory, jobSessionComponent.Entity, _logger);
    }

    protected override void ComponentDetached(JobSessionComponent jobSessionComponent)
    {
        base.ComponentDetached(jobSessionComponent);
    }
}
