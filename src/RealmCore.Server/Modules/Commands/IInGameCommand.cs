namespace RealmCore.Server.Modules.Commands;

public interface IInGameCommand
{
    Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken);
}
