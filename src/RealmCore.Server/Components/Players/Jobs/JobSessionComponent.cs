namespace RealmCore.Server.Components.Players.Jobs;

public abstract class JobSessionComponent : SessionComponent
{
    [Inject]
    protected IEntityFactory EntityFactory { get; set; } = default!;
    [Inject]
    private ILogger<JobSessionComponent> Logger { get; set; } = default!;

    public abstract short JobId { get; }

    private readonly List<Objective> _objectives = new();
    private readonly object _lock = new();
    private bool _disposing = false;
    private int _completedObjectives = 0;

    public IEnumerable<Objective> Objectives => _objectives;
    public event Action<JobSessionComponent>? CompletedAllObjectives;
    public int CompletedObjectives => _completedObjectives;

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
        try
        {
            objective.LoadInternal(EntityFactory, Entity, Logger);
        }
        catch (Exception ex)
        {
            objective.Dispose();
            lock (_lock)
                _objectives.Remove(objective);
            objective.Entity = null!;
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
