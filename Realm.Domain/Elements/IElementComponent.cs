namespace Realm.Server.Interfaces;

public interface IElementComponent : ISerializable
{
    string Name { get; }

    void SetOwner(Element element);
}
