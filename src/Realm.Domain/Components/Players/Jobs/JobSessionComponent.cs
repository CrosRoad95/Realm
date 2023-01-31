using Realm.Domain.Concepts.Objectives;
using System.Security.AccessControl;

namespace Realm.Domain.Components.Players.Jobs;

public abstract class JobSessionComponent : SessionComponent
{
    [Inject]
    private IEntityFactory EntityFactory { get; set; } = default!;

    private readonly List<Objective> _objectives = new();
    private readonly object _objectivesLock = new();
    private bool _disposing = false;
    private int _completedObjectives = 0;

    public IEnumerable<Objective> Objectives => _objectives;
    public event Action<JobSessionComponent>? CompletedAllObjectives;
    public int CompletedObjectives => _completedObjectives;

    public JobSessionComponent()
    {

    }

    public void RemoveObjective(Objective objective)
    {
        var empty = false;
        lock (_objectivesLock)
        {
            _objectives.Remove(objective);
            if(!_disposing)
                empty = !_objectives.Any();
        }

        if(empty && !_disposing)
            CompletedAllObjectives?.Invoke(this);
    }

    public TObjective AddObjective<TObjective>(TObjective objective) where TObjective : Objective
    {
        objective.Entity = Entity;
        lock(_objectivesLock)
            _objectives.Add(objective);
        try
        {
            objective.LoadInternal(EntityFactory, Entity);
        }
        catch(Exception)
        {
            objective.Entity = null!;
            objective.Dispose();
            lock (_objectivesLock)
                _objectives.Remove(objective);
            throw;
        }
        objective.Completed += HandleCompleted;
        objective.Disposed += HandleDisposed;
        return objective;
    }

    private void HandleDisposed(Objective objective)
    {
        objective.Completed -= HandleCompleted;
        objective.Disposed -= HandleDisposed;
        RemoveObjective(objective);
    }

    private void HandleCompleted(Objective objective)
    {
        if (!objective.IsFulfilled)
            objective.Incomplete();
        objective.Dispose();
        _completedObjectives++;
    }

    public override void Dispose()
    {
        _disposing = true;
        base.Dispose();
        lock(_objectivesLock)
        {
            while (_objectives.Count > 0)
            {
                var objective = _objectives.Last();
                objective.Dispose();
                RemoveObjective(objective);
            }
        }
    }
}
