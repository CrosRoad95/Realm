namespace RealmCore.Server.Interfaces.Integrations;

public interface IIntegration
{
    RealmPlayer Player { get; }

    bool IsIntegrated();
    void Remove();
}
