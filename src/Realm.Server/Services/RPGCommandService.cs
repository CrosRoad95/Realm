using SlipeServer.Server.Concepts;

namespace Realm.Server.Services;

public class RPGCommandService : IReloadable
{
    private readonly CommandService _commandService;
    private readonly ILogger _logger;

    private readonly List<Command> _commands = new();
    public RPGCommandService(CommandService commandService, ILogger logger)
    {
        _commandService = commandService;
        _logger = logger.ForContext<RPGCommandService>();
    }

    public bool AddCommandHandler(string commandName, Func<Player, string[], Task> callback, string[]? requiredPolicies = null)
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
                _logger.Verbose("{player} executed command {commandName} with arguments {commandArguments}.", player);
            else
                _logger.Verbose("{player} executed command {commandName} with no arguments.", player);
            try
            {
                await callback(player, args.Arguments);
            }
            catch (ScriptEngineException scriptEngineException)
            {
                var scriptException = scriptEngineException as IScriptEngineException;
                if (scriptException != null)
                {
                    using var errorDetails = LogContext.PushProperty("errorDetails", scriptException.ErrorDetails);
                    _logger.Error(scriptEngineException, "Exception thrown while executing command");
                }
                else
                    _logger.Error(scriptEngineException, "Exception thrown while executing command");
            }
        };
        return true;
    }

    public int GetPriority() => 39;

    public Task Reload()
    {
        foreach (var item in _commands)
            _commandService.RemoveCommand(item);
        _commands.Clear();
        return Task.CompletedTask;
    }
}
