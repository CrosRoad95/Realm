namespace RealmCore.Server.Components.Players.Jobs;

public abstract class JobSessionComponent : SessionComponent, IUpdateCallback
{
    public abstract short JobId { get; }

    private readonly object _objectivesLock = new();
    private readonly List<Objective> _objectives = new();
    private int _completedObjectives = 0;
    private bool _disposing = false;

    public IEnumerable<Objective> Objectives => _objectives;
    public int CompletedObjectives => _completedObjectives;
    public event Action<JobSessionComponent>? CompletedAllObjectives;
    public event Action<JobSessionComponent, Objective>? ObjectiveAdded;
    public event Action<JobSessionComponent, Objective>? ObjectiveCompleted;
    public event Action<JobSessionComponent, Objective>? ObjectiveIncompleted;
    public JobSessionComponent()
    {

    }

    protected bool RemoveObjective(Objective objective)
    {
        var empty = false;
        lock (_objectivesLock)
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
        objective.Entity = Entity;
        lock (_objectivesLock)
            _objectives.Add(objective);

        ObjectiveAdded?.Invoke(this, objective);
        objective.Completed += HandleCompleted;
        objective.InCompleted += HandleInCompleted;
        objective.Disposed += HandleDisposed;
        return objective;
    }

    private void HandleDisposed(Objective objective)
    {
        objective.Completed -= HandleCompleted;
        objective.InCompleted -= HandleInCompleted;
        objective.Disposed -= HandleDisposed;
        RemoveObjective(objective);
    }

    private void HandleCompleted(Objective objective, object? data = null)
    {
        if (data is Entity entity)
            entity.Dispose();

        ObjectiveCompleted?.Invoke(this, objective);
        objective.Dispose();
        _completedObjectives++;
    }

    private void HandleInCompleted(Objective objective)
    {
        ObjectiveIncompleted?.Invoke(this, objective);
        objective.Dispose();
    }

    public override void Detach()
    {
        _disposing = true;

        lock (_objectivesLock)
        {
            while (_objectives.Count > 0)
            {
                var objective = _objectives.Last();
                objective.Dispose();
                RemoveObjective(objective);
            }
        }
        base.Detach();
    }

    public virtual void Update()
    {
        List<Objective> objectives;
        lock (_objectivesLock)
            objectives = new List<Objective>(_objectives);

        foreach (var item in objectives)
            item.Update();
    }
}
