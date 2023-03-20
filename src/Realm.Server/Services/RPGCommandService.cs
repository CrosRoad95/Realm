using Realm.Logging;
using SlipeServer.Server.Concepts;
using System.Diagnostics;

namespace Realm.Server.Services;

public class RPGCommandService
{
    private readonly CommandService _commandService;
    private readonly ECS _ecs;
    private readonly IRPGUserManager _rpgUserManager;
    private readonly ILogger<RPGCommandService> _logger;

    private readonly List<Command> _commands = new();
    public RPGCommandService(CommandService commandService, ILogger<RPGCommandService> logger, ECS ecs, IRPGUserManager rpgUserManager)
    {
        _logger = logger;
        _commandService = commandService;
        _ecs = ecs;
        _rpgUserManager = rpgUserManager;
    }

    public bool AddCommandHandler(string commandName, Func<Entity, string[], Task> callback, string[]? requiredPolicies = null)
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
            if (!entity.TryGetComponent<AccountComponent>(out var accountComponent) || !entity.TryGetComponent<PlayerElementComponent>(out var playerElementComponent))
                return;

            var start = Stopwatch.GetTimestamp();
            var activity = new Activity("CommandHandler");
            activity.Start();

            using var _1 = LogContext.PushProperty("serial", playerElementComponent.Client.Serial);
            using var _2 = LogContext.PushProperty("accountId", accountComponent.Id);
            using var _3 = LogContext.PushProperty("commandName", commandName);
            using var _4 = LogContext.PushProperty("commandArguments", args.Arguments);
            _logger.LogInformation("Begin command {commandName} execution with traceId={TraceId}", commandName);
            if (requiredPolicies != null)
            {
                foreach (var policy in requiredPolicies)
                    if (!await _rpgUserManager.AuthorizePolicy(accountComponent, policy))
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
                await callback(entity, args.Arguments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while executing command {commandName} with arguments {commandArguments}", commandName, args.Arguments);
            }
            finally
            {
                _logger.LogInformation("Ended command {commandName} execution with traceId={TraceId} in {totalMiliseconds}miliseconds", commandName, activity.GetTraceId(), (Stopwatch.GetTimestamp() - start) / (float)TimeSpan.TicksPerMillisecond);
                activity.Stop();
            }
        };
        return true;
    }
}
