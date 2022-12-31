using SlipeServer.Server.Concepts;

namespace Realm.Server.Services;

public class RPGCommandService
{
    private readonly CommandService _commandService;
    private readonly IInternalRPGServer _rpgServer;
    private readonly ILogger _logger;

    private readonly List<Command> _commands = new();
    public RPGCommandService(CommandService commandService, ILogger logger, IInternalRPGServer rpgServer)
    {
        _commandService = commandService;
        _rpgServer = rpgServer;
        _logger = logger.ForContext<RPGCommandService>();
    }

    public bool AddCommandHandler(string commandName, Func<Entity, string[], Task> callback, string[]? requiredPolicies = null)
    {
        if(_commands.Any(x => string.Equals(x.CommandText, commandName, StringComparison.OrdinalIgnoreCase))) {
            throw new Exception($"Command with name '{commandName}' already exists");
        }

        if (requiredPolicies != null)
            _logger.Verbose("Created command {commandName} with required policies: {requiredPolicies}", commandName, requiredPolicies);
        else
            _logger.Verbose("Created command {commandName}", commandName);

        var command = _commandService.AddCommand(commandName);
        _commands.Add(command);
        command.Triggered += async (source, args) =>
        {
            var player = args.Player;
            var entity = _rpgServer.ECS.GetEntityByPlayer(player);
            using var playerProperty = LogContext.PushProperty("player", player);
            using var commandNameProperty = LogContext.PushProperty("commandName", commandName);
            using var commandArgumentProperty = LogContext.PushProperty("commandArguments", args.Arguments);
            if (requiredPolicies != null)
            {
                //var account = player.Account;
                //if (account == null)
                //{
                //    _logger.Verbose("{player} failed to execute command {commandName} player is not logged in", player, commandName);
                //    return;
                //}

                //foreach (var policy in requiredPolicies)
                //    if (!await account.AuthorizePolicy(policy.ToString()))
                //    {
                //        _logger.Verbose("{player} failed to execute command {commandName} because failed to authorize for policy {policy}", player, commandName, policy);
                //        return;
                //    }
            }
            if (args.Arguments.Any())
                _logger.Verbose("{player} executed command {commandName} with arguments {commandArguments}.", entity);
            else
                _logger.Verbose("{player} executed command {commandName} with no arguments.", entity);
            try
            {
                if(entity.HasComponent<AccountComponent>())
                    await callback(entity, args.Arguments);
                else
                    _logger.Verbose("{player} executed command {commandName} with no arguments.", entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception thrown while executing command");
            }
        };
        return true;
    }
}
