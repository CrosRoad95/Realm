namespace RealmCore.Server.Interfaces;

public interface IInGameCommand
{
    Task Handle(Entity entity, CommandArguments args);
}
