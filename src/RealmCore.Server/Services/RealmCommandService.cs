using Polly.RateLimit;
using RealmCore.Logging;

namespace RealmCore.Server.Services;

public class RealmCommandService
{
    private class AsyncCommandInfo
    {
        public Func<Entity, CommandArguments, Task> Callback { get; set; }
        public string[]? RequiredPolicies { get; set; }
        public bool NoTracing { get; set; }
    }

    private class CommandInfo
    {
        public Action<Entity, CommandArguments> Callback { get; set; }
        public string[]? RequiredPolicies { get; set; }
        public bool NoTracing { get; set; }
    }

    private readonly CommandService _commandService;
    private readonly IECS _ecs;
    private readonly IUsersService _usersService;
    private readonly IPolicyDrivenCommandExecutor _policyDrivenCommandExecutor;
    private readonly ChatBox _chatBox;
    private readonly ILogger<RealmCommandService> _logger;

    private readonly Dictionary<string, AsyncCommandInfo> _asyncCommands = new();
    private readonly Dictionary<string, CommandInfo> _commands = new();

    public List<string> Commands => _commands.Keys.ToList();

    public RealmCommandService(CommandService commandService, ILogger<RealmCommandService> logger, IECS ecs, IUsersService usersService, IPolicyDrivenCommandExecutor policyDrivenCommandExecutor, ChatBox chatBox)
    {
        _logger = logger;
        _commandService = commandService;
        _ecs = ecs;
        _usersService = usersService;
        _policyDrivenCommandExecutor = policyDrivenCommandExecutor;
        _chatBox = chatBox;
    }

    internal void ClearCommands()
    {
        _asyncCommands.Clear();
        _commands.Clear();
    }

    private void CheckIfCommandExists(string commandName)
    {
        if (_commands.Keys.Any(x => string.Equals(x, commandName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception($"Command with name '{commandName}' already exists");
        }
        if (_asyncCommands.Keys.Any(x => string.Equals(x, commandName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception($"Async command with name '{commandName}' already exists");
        }
    }

    public void AddAsyncCommandHandler(string commandName, Func<Entity, CommandArguments, Task> callback, string[]? requiredPolicies = null)
    {
        CheckIfCommandExists(commandName);

        var command = _commandService.AddCommand(commandName);
        _asyncCommands.Add(commandName, new AsyncCommandInfo
        {
            Callback = callback,
            RequiredPolicies = requiredPolicies
        });
        command.Triggered += HandleAsyncTriggered;

        if (requiredPolicies != null)
            _logger.LogInformation("Created async command {commandName} with required policies: {requiredPolicies}", commandName, requiredPolicies);
        else
            _logger.LogInformation("Created async command {commandName}", commandName);
    }

    public void AddCommandHandler(string commandName, Action<Entity, CommandArguments> callback, string[]? requiredPolicies = null, bool noTracing = false)
    {
        CheckIfCommandExists(commandName);

        var command = _commandService.AddCommand(commandName);
        _commands.Add(commandName, new CommandInfo
        {
            Callback = callback,
            RequiredPolicies = requiredPolicies,
            NoTracing = noTracing
        });
        command.Triggered += HandleTriggered;

        if (requiredPolicies != null)
            _logger.LogInformation("Created command {commandName} with required policies: {requiredPolicies}", commandName, requiredPolicies);
        else
            _logger.LogInformation("Created command {commandName}", commandName);
    }

    internal async Task InternalHandleTriggered(object? sender, CommandTriggeredEventArgs args)
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
        if (!commandInfo.NoTracing)
            _logger.LogInformation("Begin command {commandText} execution with traceId={TraceId}", commandText);

        if (commandInfo.RequiredPolicies != null)
        {
            foreach (var policy in commandInfo.RequiredPolicies)
                if (!await _usersService.AuthorizePolicy(userComponent, policy))
                {
                    _logger.LogInformation("Failed to execute command {commandText} because failed to authorize for policy {policy}", commandText, policy);
                    return;
                }
        }

        if (!commandInfo.NoTracing)
        {
            if (args.Arguments.Any())
                _logger.LogInformation("Executed command {commandText} with arguments {commandArguments}.", entity);
            else
                _logger.LogInformation("Executed command {commandText} with no arguments.", entity);
        }
        try
        {
            if (userComponent.HasClaim("commandsNoLimit"))
                commandInfo.Callback(entity, new CommandArguments(args.Arguments, _usersService));
            else
                _policyDrivenCommandExecutor.Execute(() =>
                {
                    commandInfo.Callback(entity, new CommandArguments(args.Arguments, _usersService));
                }, userComponent.Id.ToString());
        }
        catch (RateLimitRejectedException rateLimitRejectedException)
        {
            _chatBox.OutputTo(entity, "Zbyt szybko wysyłasz komendy. Poczekaj chwilę.");
            _logger.LogError(rateLimitRejectedException, "Exception thrown while executing command {commandText} with arguments {commandArguments}", commandText, args.Arguments);
        }
        catch (CommandArgumentException ex)
        {
            if (ex.Argument != null)
            {
                _chatBox.OutputTo(entity, $"Wystąpił błąd podczas wykonywania komendy '{commandText}'");
                if (string.IsNullOrWhiteSpace(ex.Argument))
                    _chatBox.OutputTo(entity, $"Argument {ex.Index} jest niepoprawny ponieważ: {ex.Message}");
                else
                    _chatBox.OutputTo(entity, $"Argument {ex.Index} '{ex.Argument}' jest niepoprawny ponieważ: {ex.Message}");
            }
            else
            {
                _chatBox.OutputTo(entity, $"Wystąpił błąd podczas wykonywania komendy '{commandText}'. {ex.Message}");
            }
            _logger.LogWarning("Command argument exception was thrown while executing command {commandText} with arguments {commandArguments}, argument index: {argumentIndex}", commandText, args.Arguments, ex.Index);
        }
        catch (Exception ex)
        {
            _chatBox.OutputTo(entity, "Wystąpił nieznany błąd. Jeśli się powtórzy zgłoś się do administracji.");
            _logger.LogError(ex, "Exception thrown while executing command {commandText} with arguments {commandArguments}", commandText, args.Arguments);
        }
        finally
        {
            if (!commandInfo.NoTracing)
            {
                _logger.LogInformation("Ended command {commandText} execution with traceId={TraceId} in {totalMilliseconds}milliseconds", commandText, activity.GetTraceId(), (Stopwatch.GetTimestamp() - start) / (float)TimeSpan.TicksPerMillisecond);
            }
            activity.Stop();
        }
    }

    private async void HandleTriggered(object? sender, CommandTriggeredEventArgs args)
    {
        try
        {
            await InternalHandleTriggered(sender, args);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unexpected exception was thrown while executing command {commandText} with arguments {commandArguments}", ((Command)sender!).CommandText, args.Arguments);
        }
    }

    internal async Task InternalHandleAsyncTriggered(object? sender, CommandTriggeredEventArgs args)
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
                if (!await _usersService.AuthorizePolicy(userComponent, policy))
                {
                    _logger.LogInformation("failed to execute command {commandText} because failed to authorize for policy {policy}", commandText, policy);
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
                await commandInfo.Callback(entity, new CommandArguments(args.Arguments, _usersService));
            else
                await _policyDrivenCommandExecutor.ExecuteAsync(async () =>
                {
                    await commandInfo.Callback(entity, new CommandArguments(args.Arguments, _usersService));
                }, userComponent.Id.ToString());
        }
        catch (RateLimitRejectedException ex)
        {
            _chatBox.OutputTo(entity, "Zbyt szybko wysyłasz komendy. Poczekaj chwilę.");
            _logger.LogWarning("Exception thrown while executing command {commandText} with arguments {commandArguments}", commandText, args.Arguments);
        }
        catch (CommandArgumentException ex)
        {
            if (ex.Argument != null)
            {
                _chatBox.OutputTo(entity, $"Wystąpił błąd podczas wykonywania komendy '{commandText}'");
                _chatBox.OutputTo(entity, $"Argument {ex.Index} '{ex.Argument}' jest niepoprawny ponieważ: {ex.Message}");
            }
            else
            {
                _chatBox.OutputTo(entity, $"Wystąpił błąd podczas wykonywania komendy '{commandText}'");
                if (string.IsNullOrWhiteSpace(ex.Argument))
                    _chatBox.OutputTo(entity, $"Argument {ex.Index} jest niepoprawny ponieważ: {ex.Message}");
                else
                    _chatBox.OutputTo(entity, $"Argument {ex.Index} '{ex.Argument}' jest niepoprawny ponieważ: {ex.Message}");
            }
            _logger.LogWarning("Command argument exception was thrown while executing command {commandText} with arguments {commandArguments}, argument index: {argumentIndex}", commandText, args.Arguments, ex.Index);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while executing command {commandText} with arguments {commandArguments}", commandText, args.Arguments);
        }
        finally
        {
            if (!commandInfo.NoTracing)
                _logger.LogInformation("Ended async command {commandText} execution with traceId={TraceId} in {totalMilliseconds}milliseconds", commandText, activity.GetTraceId(), (Stopwatch.GetTimestamp() - start) / (float)TimeSpan.TicksPerMillisecond);
            activity.Stop();
        }
    }

    private async void HandleAsyncTriggered(object? sender, CommandTriggeredEventArgs args)
    {
        try
        {
            await InternalHandleAsyncTriggered(sender, args);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unexpected exception was thrown while executing async command {commandText} with arguments {commandArguments}", ((Command)sender!).CommandText, args.Arguments);
        }
    }
}
