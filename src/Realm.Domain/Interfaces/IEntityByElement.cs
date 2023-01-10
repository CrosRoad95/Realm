namespace Realm.Domain.Interfaces;

public interface IEntityByElement
{
    Entity GetByElement(Element element);
    Entity? TryGetByElement(Element element);
    Entity? TryGetEntityByPlayer(Player player);
}
