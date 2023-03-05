namespace Realm.Domain.Interfaces;

public interface IIngameCommand
{
    Task Handle(Entity entity, string[] args);
}
