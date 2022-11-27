namespace Realm.Server.Interfaces;

public interface IElementComponent : ISerializable
{
    string Name { get; }

    void SetLogger(ILogger logger);
    void SetOwner(Element element);
}
