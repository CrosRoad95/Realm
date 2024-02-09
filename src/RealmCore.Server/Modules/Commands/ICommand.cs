namespace RealmCore.Server.Modules.Commands;

public interface ICommand
{
    Task Handle(RealmPlayer player, CommandArguments args);
}
