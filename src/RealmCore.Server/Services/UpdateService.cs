namespace RealmCore.Server.Services;

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
