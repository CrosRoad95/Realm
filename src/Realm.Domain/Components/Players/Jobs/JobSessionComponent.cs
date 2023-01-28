using Realm.Domain.Concepts.Objectives;

namespace Realm.Domain.Components.Players.Jobs;

public abstract class JobSessionComponent : SessionComponent
{
    [Inject]
    private IEntityFactory EntityFactory { get; set; } = default!;

    private readonly List<Objective> _objectives = new();
    private readonly object _objectivesLock = new object();
    public IEnumerable<Objective> Objectives => _objectives;
    public JobSessionComponent()
    {

    }

    public void RemoveObjective(Objective objective)
    {
        lock(_objectivesLock)
            _objectives.Remove(objective);
    }

    public TObjective AddObjective<TObjective>(TObjective objective) where TObjective : Objective
    {
        objective.Entity = Entity;
        lock(_objectivesLock)
            _objectives.Add(objective);
        objective.Load(EntityFactory, Entity);
        objective.Completed += HandleCompleted;
        objective.Disposed += HandleDisposed;
        return objective;
    }

    private void HandleDisposed(Objective objective)
    {
        RemoveObjective(objective);
    }

    private void HandleCompleted(Objective objective)
    {
        if (!objective.IsFulfilled)
            objective.Incomplete();
        objective.Dispose();
    }

    public override void Dispose()
    {
        base.Dispose();
        lock(_objectivesLock)
        {
            while (_objectives.Count > 0)
            {
                RemoveObjective(_objectives.Last());
            }
        }
    }
}
