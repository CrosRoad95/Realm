using Realm.Domain.Concepts.Objectives;

namespace Realm.Domain.Components.Players.Jobs;

public abstract class JobSessionComponent : SessionComponent
{
    [Inject]
    private IEntityFactory EntityFactory { get; set; } = default!;

    private readonly List<Objective> _objectives = new();
    public IEnumerable<Objective> Objectives => _objectives;
    public JobSessionComponent()
    {

    }

    public void RemoveObjective(Objective objective)
    {
        if (!objective.IsFulfilled)
            objective.Incomplete();
        objective.Dispose();
        _objectives.Remove(objective);
    }

    public TObjective AddObjective<TObjective>(TObjective objective) where TObjective : Objective
    {
        objective.Entity = Entity;
        _objectives.Add(objective);
        objective.Load(EntityFactory, Entity);
        objective.Completed += HandleCompleted;
        return objective;
    }

    private void HandleCompleted(Objective objective)
    {
        RemoveObjective(objective);
    }


    public override void Destroy()
    {
        while (_objectives.Count > 0)
        {
            RemoveObjective(_objectives.Last());
        }
    }
}
