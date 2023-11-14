using Polly.RateLimit;

namespace RealmCore.Server.Services;

public sealed class RealmCommandService
{
    public abstract class CommandInfo
    {
        public string CommandName { get; init; }
        public string[]? RequiredPolicies { get; init; }
        public string? Description { get; init; }
        public string? Usage { get; init; }
        public string? Category { get; init; }
        public abstract bool IsAsync { get; }
    }

    internal class SyncCommandInfo : CommandInfo
    {
        public override bool IsAsync => false;

        internal Action<RealmPlayer, CommandArguments> Callback { get; }

        public SyncCommandInfo(Action<RealmPlayer, CommandArguments> callback)
        {
            Callback = callback;
        }
    }

    internal class AsyncCommandInfo : CommandInfo
    {
        public override bool IsAsync => true;
        internal Func<RealmPlayer, CommandArguments, CancellationToken, Task> Callback { get; }

        public AsyncCommandInfo(Func<RealmPlayer, CommandArguments, CancellationToken, Task> callback)
        {
            Callback = callback;
        }
    }

    private readonly ChatBox _chatBox;
    private readonly ILogger<RealmCommandService> _logger;

    private readonly Dictionary<string, CommandInfo> _commands = new();

    public List<CommandInfo> Commands => _commands.Select(x => x.Value).ToList();
    public List<string> CommandNames => _commands.Keys.Concat(_commands.Keys).ToList();
    public int Count => _commands.Count;

    public RealmCommandService(ILogger<RealmCommandService> logger, ChatBox chatBox, MtaServer mtaServer)
    {
        _logger = logger;
        _chatBox = chatBox;
        mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player player)
    {
        player.Destroyed += HandleDestroyed;
        player.CommandEntered += HandleCommandEntered;
    }

    private async void HandleCommandEntered(Player player, PlayerCommandEventArgs eventArgs)
    {
        try
        {
            await InternalHandleAsyncTriggered((RealmPlayer)eventArgs.Source, eventArgs.Command, eventArgs.Arguments);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unexpected exception was thrown while executing async command {command} with arguments {commandArguments}", eventArgs.Command, eventArgs.Arguments);
        }
    }

    private void HandleDestroyed(Element element)
    {
        if(element is RealmPlayer player)
        {
            player.Destroyed -= HandleDestroyed;
            player.CommandEntered += HandleCommandEntered;
        }
    }

    internal void ClearCommands()
    {
        _commands.Clear();
    }

    private void CheckIfCommandExists(string commandName)
    {
        if (_commands.Keys.Any(x => string.Equals(x, commandName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new CommandExistsException($"Command with name '{commandName}' already exists");
        }
    }

    public void AddAsyncCommandHandler(string commandName, Func<RealmPlayer, CommandArguments, CancellationToken, Task> callback, string[]? requiredPolicies = null, string? description = null, string? usage = null, string? category = null)
    {
        CheckIfCommandExists(commandName);

        _commands.Add(commandName, new AsyncCommandInfo(callback)
        {
            CommandName = commandName,
            RequiredPolicies = requiredPolicies,
            Description = description,
            Usage = usage,
            Category = category
        });

        if (requiredPolicies != null)
            _logger.LogInformation("Created async command {commandName} with required policies: {requiredPolicies}", commandName, requiredPolicies);
        else
            _logger.LogInformation("Created async command {commandName}", commandName);
    }

    public void AddCommandHandler(string commandName, Action<RealmPlayer, CommandArguments> callback, string[]? requiredPolicies = null, string? description = null, string? usage = null, string? category = null)
    {
        CheckIfCommandExists(commandName);

        _commands.Add(commandName, new SyncCommandInfo(callback)
        {
            CommandName = commandName,
            RequiredPolicies = requiredPolicies,
            Description = description,
            Usage = usage,
            Category = category
        });

        if (requiredPolicies != null)
            _logger.LogInformation("Created sync command {commandName} with required policies: {requiredPolicies}", commandName, requiredPolicies);
        else
            _logger.LogInformation("Created sync command {commandName}", commandName);
    }

    internal async Task InternalHandleAsyncTriggered(RealmPlayer player, string command, string[] arguments)
    {
        if (!_commands.TryGetValue(command, out var commandInfo))
            return;

        if (!player.IsSignedIn)
            return;

        var activity = new Activity("CommandHandler").Start();
        var start = Stopwatch.GetTimestamp();

        using var _ = _logger.BeginElement(player);
        using var _commandContextScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["command"] = command,
            ["commandArguments"] = arguments
        });

        _logger.LogInformation("Begin command {command} execution", command);
        if (commandInfo.RequiredPolicies != null && commandInfo.RequiredPolicies.Length > 0)
        {
            var authorized = player.User.HasAuthorizedPolicies(commandInfo.RequiredPolicies);
            if (!authorized)
            {
                _logger.LogInformation("Failed to execute command {command} because one of authorized policy failed", command);
                return;
            }
        }
        _logger.LogInformation("{player} executed command {command} with arguments {commandArguments}.", player, command, arguments);
        try
        {
            var commandThrottlingPolicy = player.GetRequiredService<ICommandThrottlingPolicy>();
            var commandArguments = new CommandArguments(arguments, player.ServiceProvider);
            if(commandInfo is SyncCommandInfo syncCommandInfo)
            {
                if (player.User.HasClaim("commandsNoLimit"))
                    syncCommandInfo.Callback(player, commandArguments);
                else
                    commandThrottlingPolicy.Execute(() =>
                    {
                        syncCommandInfo.Callback(player, commandArguments);
                    });
            }
            else if(commandInfo is AsyncCommandInfo asyncCommandInfo)
            {
                if (player.User.HasClaim("commandsNoLimit"))
                    await asyncCommandInfo.Callback(player, commandArguments, player.CancellationToken);
                else
                    await commandThrottlingPolicy.ExecuteAsync(async (cancellationToken) =>
                    {
                        await asyncCommandInfo.Callback(player, commandArguments, cancellationToken);
                    }, player.CancellationToken);
            }
        }
        catch (RateLimitRejectedException ex)
        {
            _chatBox.OutputTo(player, "Zbyt szybko wysyłasz komendy. Poczekaj chwilę.");
            _logger.LogError(ex, "Rate limit exception thrown while executing command {command} with arguments {commandArguments}", command, arguments);
        }
        catch (CommandArgumentException ex)
        {
            if (ex.Argument != null)
            {
                _chatBox.OutputTo(player, $"Wystąpił błąd podczas wykonywania komendy '{command}'");
                _chatBox.OutputTo(player, $"Argument {ex.Index} '{ex.Argument}' jest niepoprawny ponieważ: {ex.Message}");
            }
            else
            {
                _chatBox.OutputTo(player, $"Wystąpił błąd podczas wykonywania komendy '{command}'");
                if (string.IsNullOrWhiteSpace(ex.Argument))
                    _chatBox.OutputTo(player, $"Argument {ex.Index} jest niepoprawny ponieważ: {ex.Message}");
                else
                    if(ex.Message != null)
                    {
                        _chatBox.OutputTo(player, $"Argument {ex.Index} '{ex.Argument}' jest niepoprawny ponieważ: {ex.Message}");
                    }
                    else
                        _chatBox.OutputTo(player, $"Argument {ex.Index} '{ex.Argument}' jest niepoprawny.");
            }
            _logger.LogWarning("Command argument exception was thrown while executing command {command} with arguments {commandArguments}, argument index: {argumentIndex}", command, arguments, ex.Index);
        }
        catch (Exception ex)
        {
            _chatBox.OutputTo(player, "Wystąpił nieznany błąd podczas wykonywania komendy. Jeżeli się powtórzy zgłoś się do administracji", Color.Red);
            _logger.LogError(ex, "Exception thrown while executing command {command} with arguments {commandArguments}", command, arguments);
        }
        finally
        {
            _logger.LogInformation("Ended async command {command} execution with in {totalMilliseconds}milliseconds", command, (Stopwatch.GetTimestamp() - start) / (float)TimeSpan.TicksPerMillisecond);
            activity.Stop();
        }
    }
}
