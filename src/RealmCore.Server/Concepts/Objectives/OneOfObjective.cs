namespace RealmCore.Server.Concepts.Objectives;

public class OneOfObjective : Objective
{
    private readonly Objective[] _objectives;
    private bool _completed = false;
    private object _completedLock = new();
    public override Vector3 Position => throw new NotImplementedException();

    public OneOfObjective(params Objective[] objectives)
    {
        _objectives = objectives;
    }

    protected override void Load(IEntityFactory entityFactory, Entity playerEntity)
    {
        foreach (var item in _objectives)
        {
            item.Completed += HandleCompleted;
            item.Incompleted += HandleIncompleted;
            item.Entity = playerEntity;
            item.LoadInternal(entityFactory, playerEntity, Logger);
        }
    }

    private void HandleIncompleted(Objective objective)
    {
        lock (_completedLock)
        {
            if (_completed)
                return;
            _completed = true;
        }

        Incomplete(this);
        DisposeChildObjectives(objective);
    }

    private void HandleCompleted(Objective objective)
    {
        lock (_completedLock)
        {
            if (_completed)
                return;
            _completed = true;
        }

        Complete(this);
        DisposeChildObjectives(objective);
    }

    private void DisposeChildObjectives(Objective? except = null)
    {
        foreach (var item in _objectives)
        {
            if (except != item || true)
            {
                item.Completed -= HandleCompleted;
                item.Incompleted -= HandleIncompleted;
                item.Dispose();
            }
        }
    }

    public override void Dispose()
    {
        lock (_completedLock)
        {
            if (!_completed)
                DisposeChildObjectives();
        }

        base.Dispose();
    }
}
