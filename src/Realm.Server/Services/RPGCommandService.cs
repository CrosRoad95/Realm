using Realm.Logging;
using SlipeServer.Server.Concepts;
using System.Diagnostics;

namespace Realm.Server.Services;

public class RPGCommandService
{
    private class AsyncCommandInfo
    {
        public Func<Entity, string[], Task> Callback { get; set; }
        public string[]? RequiredPolicies { get; set; }
    }
    
    private class CommandInfo
    {
        public Action<Entity, string[]> Callback { get; set; }
        public string[]? RequiredPolicies { get; set; }
    }

    private readonly CommandService _commandService;
    private readonly IECS _ecs;
    private readonly IUsersService _rpgUserManager;
    private readonly ILogger<RPGCommandService> _logger;

    private readonly Dictionary<string, AsyncCommandInfo> _asyncCommands = new();
    private readonly Dictionary<string, CommandInfo> _commands = new();
    public RPGCommandService(CommandService commandService, ILogger<RPGCommandService> logger, IECS ecs, IUsersService rpgUserManager)
    {
        _logger = logger;
        _commandService = commandService;
        _ecs = ecs;
        _rpgUserManager = rpgUserManager;
    }

    public bool AddAsyncCommandHandler(string commandName, Func<Entity, string[], Task> callback, string[]? requiredPolicies = null)
    {
        if(_asyncCommands.Keys.Any(x => string.Equals(x, commandName, StringComparison.OrdinalIgnoreCase))) {
            throw new Exception($"Command with name '{commandName}' already exists");
        }

        if (requiredPolicies != null)
            _logger.LogInformation("Created async command {commandName} with required policies: {requiredPolicies}", commandName, requiredPolicies);
        else
            _logger.LogInformation("Created async command {commandName}", commandName);

        var command = _commandService.AddCommand(commandName);
        _asyncCommands.Add(commandName, new AsyncCommandInfo
        {
            Callback = callback,
            RequiredPolicies = requiredPolicies
        });
        command.Triggered += HandleAsyncTriggered;
        return true;
    }

    public bool AddCommandHandler(string commandName, Action<Entity, string[]> callback, string[]? requiredPolicies = null)
    {
        if(_commands.Keys.Any(x => string.Equals(x, commandName, StringComparison.OrdinalIgnoreCase))) {
            throw new Exception($"Command with name '{commandName}' already exists");
        }

        if (requiredPolicies != null)
            _logger.LogInformation("Created command {commandName} with required policies: {requiredPolicies}", commandName, requiredPolicies);
        else
            _logger.LogInformation("Created command {commandName}", commandName);

        var command = _commandService.AddCommand(commandName);
        _commands.Add(commandName, new CommandInfo
        {
            Callback = callback,
            RequiredPolicies = requiredPolicies
        });
        command.Triggered += HandleTriggered;
        return true;
    }

    private async void HandleTriggered(object? sender, SlipeServer.Server.Events.CommandTriggeredEventArgs args)
    {
        try
        {
            var commandText = ((Command)sender).CommandText;
            if (!_commands.TryGetValue(commandText, out var commandInfo))
                return;

            var player = args.Player;
            if (!_ecs.TryGetEntityByPlayer(player, out var entity))
                return;

            if (!entity.TryGetComponent<AccountComponent>(out var accountComponent) || !entity.TryGetComponent<PlayerElementComponent>(out var playerElementComponent))
                return;

            var activity = new Activity("CommandHandler");
            activity.Start();
            _logger.LogInformation("Begin command {commandText} execution with traceId={TraceId}", commandText, activity.GetTraceId());
            var start = Stopwatch.GetTimestamp();

            using var _1 = LogContext.PushProperty("serial", playerElementComponent.Client.Serial);
            using var _2 = LogContext.PushProperty("accountId", accountComponent.Id);
            using var _3 = LogContext.PushProperty("commandText", commandText);
            using var _4 = LogContext.PushProperty("commandArguments", args.Arguments);
            _logger.LogInformation("Begin command {commandText} execution with traceId={TraceId}", commandText);
            if (commandInfo.RequiredPolicies != null)
            {
                foreach (var policy in commandInfo.RequiredPolicies)
                    if (!await _rpgUserManager.AuthorizePolicy(accountComponent, policy))
                    {
                        _logger.LogInformation("{player} failed to execute command {commandText} because failed to authorize for policy {policy}", player, commandText, policy);
                        return;
                    }
            }

            if (args.Arguments.Any())
                _logger.LogInformation("{player} executed command {commandText} with arguments {commandArguments}.", entity);
            else
                _logger.LogInformation("{player} executed command {commandText} with no arguments.", entity);
            try
            {
                commandInfo.Callback(entity, args.Arguments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while executing command {commandText} with arguments {commandArguments}", commandText, args.Arguments);
            }
            finally
            {
                _logger.LogInformation("Ended command {commandText} execution with traceId={TraceId} in {totalMiliseconds}miliseconds", commandText, activity.GetTraceId(), (Stopwatch.GetTimestamp() - start) / (float)TimeSpan.TicksPerMillisecond);
                activity.Stop();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while executing command {commandText} with arguments {commandArguments}", ((Command)sender).CommandText, args.Arguments);
        }
    }

    private async void HandleAsyncTriggered(object? sender, SlipeServer.Server.Events.CommandTriggeredEventArgs args)
    {
        try
        {
            var commandText = ((Command)sender).CommandText;

            if (!_asyncCommands.TryGetValue(commandText, out var commandInfo))
                return;

            var player = args.Player;
            if (!_ecs.TryGetEntityByPlayer(player, out var entity))
                return;

            if (!entity.TryGetComponent<AccountComponent>(out var accountComponent) || !entity.TryGetComponent<PlayerElementComponent>(out var playerElementComponent))
                return;

            var activity = new Activity("CommandHandler");
            activity.Start();
            _logger.LogInformation("Begin command {commandText} execution with traceId={TraceId}", commandText, activity.GetTraceId());
            var start = Stopwatch.GetTimestamp();

            using var _1 = LogContext.PushProperty("serial", playerElementComponent.Client.Serial);
            using var _2 = LogContext.PushProperty("accountId", accountComponent.Id);
            using var _3 = LogContext.PushProperty("commandText", commandText);
            using var _4 = LogContext.PushProperty("commandArguments", args.Arguments);
            _logger.LogInformation("Begin command {commandText} execution with traceId={TraceId}", commandText);
            if (commandInfo.RequiredPolicies != null)
            {
                foreach (var policy in commandInfo.RequiredPolicies)
                    if (!await _rpgUserManager.AuthorizePolicy(accountComponent, policy))
                    {
                        _logger.LogInformation("{player} failed to execute command {commandText} because failed to authorize for policy {policy}", player, commandText, policy);
                        return;
                    }
            }

            if (args.Arguments.Any())
                _logger.LogInformation("{player} executed command {commandText} with arguments {commandArguments}.", entity);
            else
                _logger.LogInformation("{player} executed command {commandText} with no arguments.", entity);
            try
            {
                await commandInfo.Callback(entity, args.Arguments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while executing command {commandText} with arguments {commandArguments}", commandText, args.Arguments);
            }
            finally
            {
                _logger.LogInformation("Ended command {commandText} execution with traceId={TraceId} in {totalMiliseconds}miliseconds", commandText, activity.GetTraceId(), (Stopwatch.GetTimestamp() - start) / (float)TimeSpan.TicksPerMillisecond);
                activity.Stop();
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while executing command {commandText} with arguments {commandArguments}", ((Command)sender).CommandText, args.Arguments);
        }
    }
}
