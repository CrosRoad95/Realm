namespace Realm.Server.Interfaces;

public interface IComponent
{
    string Name { get; }

    void SetLogger(ILogger logger);
    void SetOwner(Element element);
}
