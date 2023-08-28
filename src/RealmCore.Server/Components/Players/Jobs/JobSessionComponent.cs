namespace RealmCore.Server.Components.Players.Jobs;

public abstract class JobSessionComponent : SessionComponent
{
    public abstract short JobId { get; }

    private readonly List<Objective> _objectives = new();
    private readonly object _lock = new();
    private bool _disposing = false;
    private int _completedObjectives = 0;

    public IEnumerable<Objective> Objectives => _objectives;
    public int CompletedObjectives => _completedObjectives;
    public event Action<JobSessionComponent>? CompletedAllObjectives;
    public event Action<JobSessionComponent, Objective> ObjectiveAdded;
    public JobSessionComponent()
    {

    }

    protected void RemoveObjective(Objective objective)
    {
        var empty = false;
        lock (_lock)
        {
            _objectives.Remove(objective);
            if (!_disposing)
                empty = !_objectives.Any();
        }

        if (empty && !_disposing)
            CompletedAllObjectives?.Invoke(this);
    }

    protected TObjective AddObjective<TObjective>(TObjective objective) where TObjective : Objective
    {
        objective.Entity = Entity;
        lock (_lock)
            _objectives.Add(objective);
        ObjectiveAdded?.Invoke(this, objective);
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

    private void HandleCompleted(Objective objective, object? data = null)
    {
        if (!objective.IsFulfilled)
            objective.Incomplete(objective);
        objective.Dispose();
        _completedObjectives++;
    }

    public override void Dispose()
    {
        _disposing = true;
        lock (_lock)
        {
            while (_objectives.Count > 0)
            {
                var objective = _objectives.Last();
                objective.Dispose();
                RemoveObjective(objective);
            }
        }
        base.Dispose();
    }
}
