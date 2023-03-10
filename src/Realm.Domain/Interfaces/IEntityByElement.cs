namespace Realm.Domain.Interfaces;

public interface IEntityByElement
{
    Entity GetByElement(Element element);
    bool TryGetByElement(Element element, out Entity result);
    bool TryGetEntityByPlayer(Player player, out Entity result);
}
