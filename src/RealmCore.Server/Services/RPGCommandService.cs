using Polly;
using Polly.RateLimit;
using RealmCore.Logging;
using System.Diagnostics;

namespace RealmCore.Server.Services;

public class RPGCommandService
{
    private class AsyncCommandInfo
    {
        public Func<Entity, string[], Task> Callback { get; set; }
        public string[]? RequiredPolicies { get; set; }
        public bool NoTracing { get; set; }
    }

    private class CommandInfo
    {
        public Action<Entity, string[]> Callback { get; set; }
        public string[]? RequiredPolicies { get; set; }
        public bool NoTracing { get; set; }
    }

    private readonly CommandService _commandService;
    private readonly IECS _ecs;
    private readonly IUsersService _rpgUserManager;
    private readonly IPolicyDrivenCommandExecutor _policyDrivenCommandExecutor;
    private readonly ILogger<RPGCommandService> _logger;

    private readonly Dictionary<string, AsyncCommandInfo> _asyncCommands = new();
    private readonly Dictionary<string, CommandInfo> _commands = new();
    public RPGCommandService(CommandService commandService, ILogger<RPGCommandService> logger, IECS ecs, IUsersService rpgUserManager, IPolicyDrivenCommandExecutor policyDrivenCommandExecutor)
    {
        _logger = logger;
        _commandService = commandService;
        _ecs = ecs;
        _rpgUserManager = rpgUserManager;
        _policyDrivenCommandExecutor = policyDrivenCommandExecutor;
    }

    public bool AddAsyncCommandHandler(string commandName, Func<Entity, string[], Task> callback, string[]? requiredPolicies = null)
    {
        if (_asyncCommands.Keys.Any(x => string.Equals(x, commandName, StringComparison.OrdinalIgnoreCase)))
        {
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

    public bool AddCommandHandler(string commandName, Action<Entity, string[]> callback, string[]? requiredPolicies = null, bool noTracing = false)
    {
        if (_commands.Keys.Any(x => string.Equals(x, commandName, StringComparison.OrdinalIgnoreCase)))
        {
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
            RequiredPolicies = requiredPolicies,
            NoTracing = noTracing
        });
        command.Triggered += HandleTriggered;
        return true;
    }

    private async void HandleTriggered(object? sender, CommandTriggeredEventArgs args)
    {
        try
        {
            var commandText = ((Command)sender!).CommandText;
            if (!_commands.TryGetValue(commandText, out var commandInfo))
                return;

            var player = args.Player;
            if (!_ecs.TryGetEntityByPlayer(player, out var entity))
                return;

            if (!entity.TryGetComponent<UserComponent>(out var userComponent) || !entity.TryGetComponent<PlayerElementComponent>(out var playerElementComponent))
                return;

            var activity = new Activity("CommandHandler");
            activity.Start();
            var start = Stopwatch.GetTimestamp();

            using var _1 = _logger.BeginEntity(entity);
            using var _2 = LogContext.PushProperty("commandText", commandText);
            using var _3 = LogContext.PushProperty("commandArguments", args.Arguments);
            if(!commandInfo.NoTracing)
                _logger.LogInformation("Begin command {commandText} execution with traceId={TraceId}", commandText);

            if (commandInfo.RequiredPolicies != null)
            {
                foreach (var policy in commandInfo.RequiredPolicies)
                    if (!await _rpgUserManager.AuthorizePolicy(userComponent, policy))
                    {
                        _logger.LogInformation("{player} failed to execute command {commandText} because failed to authorize for policy {policy}", player, commandText, policy);
                        return;
                    }
            }

            if (!commandInfo.NoTracing)
            {
                if (args.Arguments.Any())
                    _logger.LogInformation("{player} executed command {commandText} with arguments {commandArguments}.", entity);
                else
                    _logger.LogInformation("{player} executed command {commandText} with no arguments.", entity);
            }
            try
            {
                if(userComponent.HasClaim("commandsNoLimit"))
                    commandInfo.Callback(entity, args.Arguments);
                else
                    _policyDrivenCommandExecutor.Execute(() =>
                    {
                        commandInfo.Callback(entity, args.Arguments);
                    }, userComponent.Id.ToString());
            }
            catch (RateLimitRejectedException rateLimitRejectedException)
            {
                playerElementComponent.SendChatMessage("Zbyt szybko wysyłasz komendy. Poczekaj chwilę.");
                _logger.LogError(rateLimitRejectedException, "Exception thrown while executing command {commandText} with arguments {commandArguments}", commandText, args.Arguments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while executing command {commandText} with arguments {commandArguments}", commandText, args.Arguments);
            }
            finally
            {
                if (!commandInfo.NoTracing)
                {
                    _logger.LogInformation("Ended command {commandText} execution with traceId={TraceId} in {totalMiliseconds}miliseconds", commandText, activity.GetTraceId(), (Stopwatch.GetTimestamp() - start) / (float)TimeSpan.TicksPerMillisecond);
                }
                activity.Stop();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while executing command {commandText} with arguments {commandArguments}", ((Command)sender!).CommandText, args.Arguments);
        }
    }

    private async void HandleAsyncTriggered(object? sender, CommandTriggeredEventArgs args)
    {
        try
        {
            var commandText = ((Command)sender!).CommandText;

            if (!_asyncCommands.TryGetValue(commandText, out var commandInfo))
                return;

            var player = args.Player;
            if (!_ecs.TryGetEntityByPlayer(player, out var entity))
                return;

            if (!entity.TryGetComponent<UserComponent>(out var userComponent) || !entity.TryGetComponent<PlayerElementComponent>(out var playerElementComponent))
                return;

            var activity = new Activity("CommandHandler");
            activity.Start();
            var start = Stopwatch.GetTimestamp();

            using var _1 = _logger.BeginEntity(entity);
            using var _2 = LogContext.PushProperty("commandText", commandText);
            using var _3 = LogContext.PushProperty("commandArguments", args.Arguments);
            if (!commandInfo.NoTracing)
                _logger.LogInformation("Begin async command {commandText} execution with traceId={TraceId}", commandText);

            if (commandInfo.RequiredPolicies != null)
            {
                foreach (var policy in commandInfo.RequiredPolicies)
                    if (!await _rpgUserManager.AuthorizePolicy(userComponent, policy))
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

                if (userComponent.HasClaim("commandsNoLimit"))
                    await commandInfo.Callback(entity, args.Arguments);
                else
                    await _policyDrivenCommandExecutor.ExecuteAsync(async () =>
                    {
                        await commandInfo.Callback(entity, args.Arguments);
                    }, userComponent.Id.ToString());
            }
            catch (RateLimitRejectedException rateLimitRejectedException)
            {
                playerElementComponent.SendChatMessage("Zbyt szybko wysyłasz komendy. Poczekaj chwilę.");
                _logger.LogError(rateLimitRejectedException, "Exception thrown while executing command {commandText} with arguments {commandArguments}", commandText, args.Arguments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while executing command {commandText} with arguments {commandArguments}", commandText, args.Arguments);
            }
            finally
            {
                if (!commandInfo.NoTracing)
                    _logger.LogInformation("Ended async command {commandText} execution with traceId={TraceId} in {totalMiliseconds}miliseconds", commandText, activity.GetTraceId(), (Stopwatch.GetTimestamp() - start) / (float)TimeSpan.TicksPerMillisecond);
                activity.Stop();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while executing command {commandText} with arguments {commandArguments}", ((Command)sender!).CommandText, args.Arguments);
        }
    }
}
