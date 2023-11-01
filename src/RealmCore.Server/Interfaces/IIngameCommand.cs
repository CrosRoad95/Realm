namespace RealmCore.Server.Interfaces;

public interface IInGameCommand
{
    Task Handle(RealmPlayer player, CommandArguments args);
}
