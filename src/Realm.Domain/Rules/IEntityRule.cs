namespace Realm.Domain.Rules;

public interface IEntityRule
{
    bool Check(Entity entity);
}
