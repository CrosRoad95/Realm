namespace RealmCore.Server.Concepts.Sessions;

public abstract class JobSession : Session
{
    public abstract short JobId { get; }

    private readonly object _objectivesLock = new();
    private readonly List<Objective> _objectives = [];
    protected readonly IScopedElementFactory _elementFactory;
    private readonly IUpdateService _updateService;
    private int _completedObjectives = 0;
    private bool _disposing = false;

    public IEnumerable<Objective> Objectives => _objectives;
    public int CompletedObjectives => _completedObjectives;
    public event Action<JobSession>? CompletedAllObjectives;
    public event Action<JobSession, Objective>? ObjectiveAdded;
    public event Action<JobSession, Objective>? ObjectiveCompleted;
    public event Action<JobSession, Objective>? ObjectiveInCompleted;
    public JobSession(IScopedElementFactory scopedElementFactory, PlayerContext playerContext, IUpdateService updateService) : base(playerContext.Player)
    {
        _elementFactory = scopedElementFactory.CreateScope();
        _updateService = updateService;
    }

    protected override void OnStarted()
    {
        _updateService.Update += HandleUpdate;
    }

    protected override void OnEnded()
    {
        _updateService.Update -= HandleUpdate;
        _disposing = true;

        _elementFactory?.Dispose();

        lock (_objectivesLock)
        {
            while (_objectives.Count > 0)
            {
                var objective = _objectives.Last();
                objective.Dispose();
                RemoveObjective(objective);
            }
        }
    }

    private void HandleUpdate()
    {
        Update();
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
        objective.Player = Player;
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

    public virtual void Update()
    {
        List<Objective> objectives;
        lock (_objectivesLock)
            objectives = new List<Objective>(_objectives);

        foreach (var item in objectives)
            item.Update();
    }
}
