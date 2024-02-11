namespace RealmCore.Server.Modules.Players.Jobs.Objectives;

public class OneOfObjective : Objective
{
    private readonly object _lock = new();
    private readonly Objective[] _objectives;
    private bool _completed = false;

    public override Vector3 Position => throw new NotSupportedException();

    public OneOfObjective(params Objective[] objectives)
    {
        _objectives = objectives;
    }

    protected override void Load()
    {
        foreach (var item in _objectives)
        {
            item.Completed += HandleCompleted;
            item.InCompleted += HandleInCompleted;
            item.Player = Player;
            item.LoadInternal(Player);
        }
    }

    public override void Update()
    {
        bool completed = false;
        void handleCompleted(Objective _1, object? _2)
        {
            completed = true;
        }
        Completed += handleCompleted;
        var objectives = new List<Objective>(_objectives);
        foreach (var item in objectives)
        {
            if (completed)
                break;
            item.Update();
        }
        Completed -= handleCompleted;
    }

    private void HandleInCompleted(Objective objective)
    {
        lock (_lock)
        {
            if (_completed)
                return;
            _completed = true;
        }

        Incomplete(this);
        DisposeChildObjectives(objective);
    }

    private void HandleCompleted(Objective objective, object? data)
    {
        lock (_lock)
        {
            if (_completed)
                return;
            _completed = true;
        }

        Complete(this, data);
        DisposeChildObjectives(objective);
    }

    private void DisposeChildObjectives(Objective? except = null)
    {
        foreach (var item in _objectives)
        {
            if (except != item || true)
            {
                item.Completed -= HandleCompleted;
                item.InCompleted -= HandleInCompleted;
                item.Dispose();
            }
        }
    }

    public override void Dispose()
    {
        lock (_lock)
        {
            if (!_completed)
                DisposeChildObjectives();
        }

        base.Dispose();
    }
}
