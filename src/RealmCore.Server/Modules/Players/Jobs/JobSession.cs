namespace RealmCore.Server.Modules.Players.Jobs;

public abstract class JobSession : Session
{
    public abstract short JobId { get; }

    private readonly List<Objective> _objectives = [];
    protected readonly IScopedElementFactory _elementFactory;
    private int _completedObjectives = 0;
    private bool _disposing = false;

    public IEnumerable<Objective> Objectives => _objectives;
    public int CompletedObjectives => _completedObjectives;
    public event Action<JobSession>? CompletedAllObjectives;
    public event Action<JobSession, Objective>? ObjectiveAdded;
    public event Action<JobSession, Objective>? ObjectiveCompleted;
    public event Action<JobSession, Objective>? ObjectiveInCompleted;

    public JobSession(PlayerContext playerContext, IDateTimeProvider dateTimeProvider) : base(playerContext.Player, dateTimeProvider)
    {
        _elementFactory = Player.ElementFactory.CreateScope();

        Player.Scheduler.ScheduleJob(HandleUpdate, TimeSpan.FromMilliseconds(200), CreateCancellationToken());
    }

    private Task HandleUpdate()
    {
        lock (_lock)
        {
            foreach (var objective in _objectives)
            {
                objective.Update();
            }
        }

        return Task.CompletedTask;
    }

    protected override void OnEnded()
    {
        _disposing = true;

        _elementFactory?.Dispose();

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

    protected bool RemoveObjective(Objective objective)
    {
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
        lock (_lock)
            _objectives.Add(objective);

        try
        {
            objective.LoadInternal(Player);
        }
        catch (Exception)
        {
            lock (_lock)
                _objectives.Remove(objective);
            throw;
        }
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
        if (data is Element element)
            element.Destroy();

        ObjectiveCompleted?.Invoke(this, objective);
        objective.Dispose();
        _completedObjectives++;
    }

    private void HandleInCompleted(Objective objective)
    {
        ObjectiveInCompleted?.Invoke(this, objective);
        objective.Dispose();
    }
}
