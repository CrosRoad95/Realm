namespace Realm.Server.Interfaces;

public interface IIngameCommand
{
    Task Handle(Entity entity, string[] args);
}
