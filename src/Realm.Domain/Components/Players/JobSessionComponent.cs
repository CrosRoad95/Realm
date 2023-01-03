using Realm.Domain.Concepts.Objectives;
using System.Collections.Generic;

namespace Realm.Domain.Components.Players;

public class JobSessionComponent : SessionComponent
{
    private readonly List<Objective> _objectives = new();
    public IEnumerable<Objective> Objectives => _objectives;
    public JobSessionComponent()
    {

    }

    public void RemoveObjective(Objective objective)
    {
        if(!objective.IsFulfilled)
            objective.Incomplete();
        objective.Dispose();
        _objectives.Remove(objective);
    }

    public TObjective AddObjective<TObjective>(TObjective objective) where TObjective : Objective
    {
        objective.Entity = Entity;
        _objectives.Add(objective);
        objective.Completed += HandleCompleted;
        return objective;
    }

    private void HandleCompleted(Objective objective)
    {
        RemoveObjective(objective);
    }

    public override void Destroy()
    {
        while(_objectives.Count > 0)
        {
            RemoveObjective(_objectives.Last());
        }
    }
}
