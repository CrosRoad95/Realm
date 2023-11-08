
namespace RealmCore.Server.Interfaces;

public interface IUpdateService
{
    event Action? Update;
    event Action? RareUpdate;

    internal void RelayRareUpdate();
    internal void RelayUpdate();
}
