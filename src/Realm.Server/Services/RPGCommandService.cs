using SlipeServer.Server.Concepts;

namespace Realm.Server.Services;

public class RPGCommandService
{
    private readonly CommandService _commandService;
    private readonly ECS _ecs;
    private readonly ILogger<RPGCommandService> _logger;

    private readonly List<Command> _commands = new();
    public RPGCommandService(CommandService commandService, ILogger<RPGCommandService> logger, ECS ecs)
    {
        _logger = logger;
        _commandService = commandService;
        _ecs = ecs;
    }

    public bool AddCommandHandler(string commandName, Func<Guid, Entity, string[], Task> callback, string[]? requiredPolicies = null)
    {
        if(_commands.Any(x => string.Equals(x.CommandText, commandName, StringComparison.OrdinalIgnoreCase))) {
            throw new Exception($"Command with name '{commandName}' already exists");
        }

        if (requiredPolicies != null)
            _logger.LogInformation("Created command {commandName} with required policies: {requiredPolicies}", commandName, requiredPolicies);
        else
            _logger.LogInformation("Created command {commandName}", commandName);

        var command = _commandService.AddCommand(commandName);
        _commands.Add(command);
        command.Triggered += async (source, args) =>
        {
            var player = args.Player;
            var entity = _ecs.GetEntityByPlayer(player);
            if (!entity.TryGetComponent<AccountComponent>(out var accountComponent))
                return;

            using var playerProperty = LogContext.PushProperty("player", player);
            using var commandNameProperty = LogContext.PushProperty("commandName", commandName);
            using var commandArgumentProperty = LogContext.PushProperty("commandArguments", args.Arguments);
            var traceId = Guid.NewGuid();
            _logger.LogInformation("Begin command {commandName} execution with traceId={traceId}", commandName, traceId);
            if (requiredPolicies != null)
            {
                foreach (var policy in requiredPolicies)
                    if (!await accountComponent.AuthorizePolicy(policy))
                    {
                        _logger.LogInformation("{player} failed to execute command {commandName} because failed to authorize for policy {policy}", player, commandName, policy);
                        return;
                    }
            }

            if (args.Arguments.Any())
                _logger.LogInformation("{player} executed command {commandName} with arguments {commandArguments}.", entity);
            else
                _logger.LogInformation("{player} executed command {commandName} with no arguments.", entity);
            try
            {
                await callback(traceId, entity, args.Arguments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while executing command {commandName} with arguments {commandArguments}", commandName, args.Arguments);
            }
            finally
            {
                _logger.LogInformation("Ended command {commandName} execution with traceId={traceId}", commandName, traceId);
            }
        };
        return true;
    }
}
