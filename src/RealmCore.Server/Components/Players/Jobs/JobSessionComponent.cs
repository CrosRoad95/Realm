namespace RealmCore.Server.Components.Players.Jobs;

public abstract class JobSessionComponent : SessionComponent
{
    public abstract short JobId { get; }

    private readonly List<Objective> _objectives = new();
    private readonly object _lock = new();
    private int _completedObjectives = 0;
    private bool _disposing = false;

    public IEnumerable<Objective> Objectives => _objectives;
    public int CompletedObjectives => _completedObjectives;
    public event Action<JobSessionComponent>? CompletedAllObjectives;
    public event Action<JobSessionComponent, Objective>? ObjectiveAdded;
    public JobSessionComponent()
    {

    }

    protected bool RemoveObjective(Objective objective)
    {
        ThrowIfDisposed();

        var empty = false;
        lock (_lock)
        {
            if (!_objectives.Remove(objective))
                return false;
            empty = _objectives.Count == 0;
        }

        if (empty && !_disposing)
            CompletedAllObjectives?.Invoke(this);
        return true;
    }

    protected TObjective AddObjective<TObjective>(TObjective objective) where TObjective : Objective
    {
        ThrowIfDisposed();

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

    protected override void Detach()
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
    }
}
