namespace RealmCore.Server.Modules.Commands;

public interface IInGameCommand
{
    string[] RequiredPolicies { get; }
    Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken);
}
