namespace RealmCore.Server.Services;

public interface IUpdateService
{
    event Action? Update;
    event Action? RareUpdate;

    internal void RelayRareUpdate();
    internal void RelayUpdate();
}

internal class UpdateService : IUpdateService
{
    public event Action? Update;
    public event Action? RareUpdate;

    public void RelayUpdate()
    {
        Update?.Invoke();
    }

    public void RelayRareUpdate()
    {
        RareUpdate?.Invoke();
    }
}
