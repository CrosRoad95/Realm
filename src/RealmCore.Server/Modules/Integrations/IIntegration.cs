namespace RealmCore.Server.Modules.Integrations;

public interface IIntegration
{
    RealmPlayer Player { get; }

    bool IsIntegrated();
    void Remove();
}
