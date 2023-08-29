namespace RealmCore.Server.Interfaces;

public interface ICommand
{
    Task Handle(Entity entity, CommandArguments args);
}
