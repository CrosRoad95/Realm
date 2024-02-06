namespace RealmCore.Server.Interfaces;

public interface IIntegration
{
    RealmPlayer Player { get; }

    bool IsIntegrated();
    void Remove();
}
