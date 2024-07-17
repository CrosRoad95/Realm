namespace RealmCore.Server.Modules.Commands;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class CommandAttribute : Attribute { }
public class CallingPlayerAttribute : CommandAttribute { }
public class ReadRestAsStringAttribute : CommandAttribute { }
public class PlayerSearchOptionsAttribute : CommandAttribute
{
    public PlayerSearchOption PlayerSearchOption { get; }
    public PlayerSearchOptionsAttribute(PlayerSearchOption playerSearchOption = PlayerSearchOption.All)
    {
        PlayerSearchOption = playerSearchOption;
    }
}

public sealed class RealmCommandService : PlayerLifecycle
{   
    private readonly ChatBox _chatBox;
    private readonly IOptions<CommandsOptions> _options;
    private readonly ILogger<RealmCommandService> _logger;

    private readonly Dictionary<string, CommandInfo> _commands = [];

    public CommandInfo[] Commands => [.. _commands.Select(x => x.Value)];
    public string[] CommandNames => [.. _commands.Keys.Concat(_commands.Keys)];
    public int Count => _commands.Count;

    public RealmCommandService(ILogger<RealmCommandService> logger, ChatBox chatBox, PlayersEventManager playersEventManager, IEnumerable<IInGameCommand> inGameCommands, IOptions<CommandsOptions> options) : base(playersEventManager)
    {
        _logger = logger;
        _chatBox = chatBox;
        _options = options;
        foreach (var inGameCommand in inGameCommands)
        {
            var type = inGameCommand.GetType();
            var commandNameAttribute = type.GetCustomAttribute<CommandNameAttribute>();
            if (commandNameAttribute == null)
            {
                logger.LogWarning("Command class {commandClass} has no CommandName attribute", type.Name);
                continue;
            }

            var commandName = commandNameAttribute.Name.ToLower();
            AddAsyncCommandHandler(commandName, async (player, args, token) =>
            {
                if (player.GetRequiredService(type) is not IInGameCommand inGameCommand)
                    throw new InvalidOperationException();

                if (inGameCommand.RequiredPolicies.Length > 0)
                {
                    var authorized = player.User.HasAuthorizedPolicies(inGameCommand.RequiredPolicies);
                    if (!authorized)
                    {
                        _logger.LogInformation("Failed to execute command {command} because one of authorized policy failed", commandName);
                        return;
                    }
                }

                await inGameCommand.Handle(player, args, token);
            });
        }
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.CommandEntered += HandleCommandEntered;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        player.CommandEntered -= HandleCommandEntered;
    }

    private async void HandleCommandEntered(Player player, PlayerCommandEventArgs eventArgs)
    {
        try
        {
            await InternalHandleAsyncTriggered((RealmPlayer)eventArgs.Source, eventArgs.Command, eventArgs.Arguments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception was thrown while executing async command {command} with arguments {commandArguments}", eventArgs.Command, eventArgs.Arguments);
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

    private void AddCore(CommandInfo commandInfo)
    {
        CheckIfCommandExists(commandInfo.CommandName);

        _commands.Add(commandInfo.CommandName, commandInfo);

        if (!_options.Value.LogCreatedCommands)
            return;

        if (commandInfo.IsAsync)
        {
            if (commandInfo.RequiredPolicies != null)
                _logger.LogInformation("Created async command {commandName} with required policies: {requiredPolicies}", commandInfo.CommandName, commandInfo.RequiredPolicies);
            else
                _logger.LogInformation("Created async command {commandName}", commandInfo.CommandName);
        }
        else
        {
            if (commandInfo.RequiredPolicies != null)
                _logger.LogInformation("Created sync command {commandName} with required policies: {requiredPolicies}", commandInfo.CommandName, commandInfo.RequiredPolicies);
            else
                _logger.LogInformation("Created sync command {commandName}", commandInfo.CommandName);
        }
    }

    public void Add(string commandName, Delegate callback, string[]? requiredPolicies = null, string? description = null, string? category = null)
    {
        var isAsync = callback.Method.ReturnType == typeof(Task);
        if (isAsync)
        {
            AddCore(new DelegateAsyncCommandInfo(commandName, callback)
            {
                RequiredPolicies = requiredPolicies,
                Description = description,
                Category = category
            });
        }
        else
        {
            AddCore(new DelegateSyncCommandInfo(commandName, callback)
            {
                RequiredPolicies = requiredPolicies,
                Description = description,
                Category = category
            });
        }
    }

    [Obsolete("Use variant with delegate")]
    public void AddAsyncCommandHandler(string commandName, Func<RealmPlayer, CommandArguments, CancellationToken, Task> callback, string[]? requiredPolicies = null, string? description = null, string? usage = null, string? category = null)
    {
        AddCore(new AsyncCommandInfo(commandName, callback)
        {
            RequiredPolicies = requiredPolicies,
            Description = description,
            Usage = usage,
            Category = category
        });
    }

    [Obsolete("Use variant with delegate")]
    public void AddCommandHandler(string commandName, Action<RealmPlayer, CommandArguments> callback, string[]? requiredPolicies = null, string? description = null, string? usage = null, string? category = null)
    {
        AddCore(new SyncCommandInfo(commandName, callback)
        {
            RequiredPolicies = requiredPolicies,
            Description = description,
            Usage = usage,
            Category = category
        });
    }

    internal async Task InternalHandleAsyncTriggered(RealmPlayer player, string command, string[] arguments)
    {
        if (!_commands.TryGetValue(command, out var commandInfo))
            return;

        if (!player.User.IsLoggedIn)
            return;

        using var activity = Activity.StartActivity("CommandHandler");
        if(activity != null)
        {
            activity.SetTag("Command", command);
            activity.SetTag("Arguments", arguments);
        }

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
        _logger.LogInformation("{playerName} executed command {command} with arguments {commandArguments}.", player.Name, command, arguments);
        try
        {
            var commandArguments = new CommandArguments(player, player.ServiceProvider.GetRequiredService<IElementSearchService>(), arguments);

            if (commandInfo is SyncCommandInfo syncCommandInfo)
            {
                await player.Invoke(() =>
                {
                    if (player.User.HasClaim("commandsNoLimit"))
                        syncCommandInfo.Callback(player, commandArguments);
                    else
                    {
                        var commandThrottlingPolicy = player.GetRequiredService<ICommandThrottlingPolicy>();
                        commandThrottlingPolicy.Execute(() =>
                        {
                            syncCommandInfo.Callback(player, commandArguments);
                        });
                    }
                    return Task.CompletedTask;
                });
            }
            else if (commandInfo is AsyncCommandInfo asyncCommandInfo)
            {
                var token = player.CreateCancellationToken();

                await player.Invoke(async () =>
                {
                    if (player.User.HasClaim("commandsNoLimit"))
                        await asyncCommandInfo.Callback(player, commandArguments, token);
                    else
                    {
                        var commandThrottlingPolicy = player.GetRequiredService<ICommandThrottlingPolicy>();
                        await commandThrottlingPolicy.ExecuteAsync(async (cancellationToken) =>
                        {
                            await asyncCommandInfo.Callback(player, commandArguments, cancellationToken);
                        }, token);
                    }
                }, token);
            }
            else if (commandInfo is DelegateAsyncCommandInfo delegateAsyncCommandInfo)
            {
                var token = player.CreateCancellationToken();
                if (player.User.HasClaim("commandsNoLimit"))
                    await delegateAsyncCommandInfo.Invoke(player, commandArguments, token);
                else
                {
                    var commandThrottlingPolicy = player.GetRequiredService<ICommandThrottlingPolicy>();
                    await commandThrottlingPolicy.ExecuteAsync(async (cancellationToken) =>
                    {
                        await delegateAsyncCommandInfo.Invoke(player, commandArguments, cancellationToken);
                    }, token);
                }
            }
            else if (commandInfo is DelegateSyncCommandInfo delegateSyncCommandInfo)
            {
                var token = player.CreateCancellationToken();
                if (player.User.HasClaim("commandsNoLimit"))
                    delegateSyncCommandInfo.Invoke(player, commandArguments, token);
                else
                {
                    var commandThrottlingPolicy = player.GetRequiredService<ICommandThrottlingPolicy>();
                    commandThrottlingPolicy.Execute(() =>
                    {
                        delegateSyncCommandInfo.Invoke(player, commandArguments, token);
                    });
                }
            }
        }
        catch (RateLimitRejectedException ex)
        {
            _chatBox.OutputTo(player, "Zbyt szybko wysyłasz komendy. Poczekaj chwilę.");
            //_logger.LogError(ex, "Rate limit exception thrown while executing command {command} with arguments {commandArguments}", command, arguments);
        }
        catch (CommandArgumentException ex)
        {
            if (ex.Argument != null)
            {
                if(ex.Index != null)
                {
                    _chatBox.OutputTo(player, $"Wystąpił błąd podczas wykonywania komendy '{command}'");
                    _chatBox.OutputTo(player, $"Argument {ex.Index} '{ex.Argument}' jest niepoprawny ponieważ: {ex.Message}");
                }
                else
                {
                    _chatBox.OutputTo(player, $"Wystąpił błąd podczas wykonywania komendy '{command}'. {ex.Message}");
                }
            }
            else
            {
                if (ex.Index != null)
                {
                    _chatBox.OutputTo(player, $"Wystąpił błąd podczas wykonywania komendy '{command}'");
                    if (string.IsNullOrWhiteSpace(ex.Argument))
                        _chatBox.OutputTo(player, $"Argument {ex.Index} jest niepoprawny ponieważ: {ex.Message}");
                    else
                        if (ex.Message != null)
                    {
                        _chatBox.OutputTo(player, $"Argument {ex.Index} '{ex.Argument}' jest niepoprawny ponieważ: {ex.Message}");
                    }
                    else
                        _chatBox.OutputTo(player, $"Argument {ex.Index} '{ex.Argument}' jest niepoprawny.");
                }
                else
                {
                    _chatBox.OutputTo(player, $"Wystąpił błąd podczas wykonywania komendy '{command}'. {ex.Message}");
                }
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
            _logger.LogInformation("{playerName} Ended async command {command} execution with in {totalMilliseconds}milliseconds", player.Name, command, (Stopwatch.GetTimestamp() - start) / (float)TimeSpan.TicksPerMillisecond);
        }
    }

    public static readonly ActivitySource Activity = new("RealmCore.Commands", "1.0.0");
}
