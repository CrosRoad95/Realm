namespace Realm.Domain.Interfaces;

public interface IIngameCommand
{
    Task Handle(Guid traceId, Entity entity, string[] args);
}
