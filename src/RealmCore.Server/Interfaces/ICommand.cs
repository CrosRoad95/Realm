namespace RealmCore.Server.Interfaces;

public interface ICommand
{
    Task Handle(RealmPlayer player, CommandArguments args);
}
