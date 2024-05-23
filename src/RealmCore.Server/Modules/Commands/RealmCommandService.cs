using System.ComponentModel.DataAnnotations;

namespace RealmCore.Server.Modules.Commands;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class CallingPlayerAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class PlayerSearchOptionsAttribute : Attribute
{
    public PlayerSearchOption PlayerSearchOption { get; }
    public PlayerSearchOptionsAttribute(PlayerSearchOption playerSearchOption = PlayerSearchOption.All)
    {
        PlayerSearchOption = playerSearchOption;
    }
}

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

        public CommandInfo(string commandName)
        {
            CommandName = commandName;
        }
    }

    internal sealed class SyncCommandInfo : CommandInfo
    {
        public override bool IsAsync => false;

        internal Action<RealmPlayer, CommandArguments> Callback { get; }

        public SyncCommandInfo(string commandName, Action<RealmPlayer, CommandArguments> callback) : base(commandName)
        {
            Callback = callback;
        }
    }

    internal sealed class AsyncCommandInfo : CommandInfo
    {
        public override bool IsAsync => true;
        internal Func<RealmPlayer, CommandArguments, CancellationToken, Task> Callback { get; }

        public AsyncCommandInfo(string commandName, Func<RealmPlayer, CommandArguments, CancellationToken, Task> callback) : base(commandName)
        {
            Callback = callback;
        }
    }
    
    internal sealed class DelegateAsyncCommandInfo : CommandInfo
    {
        public override bool IsAsync => true;
        private readonly Delegate _callback;
        private readonly ParameterInfo[] _parameters;

        public DelegateAsyncCommandInfo(string commandName, Delegate callback) : base(commandName)
        {
            var method = callback.GetMethodInfo();
            _parameters = method.GetParameters();
            var parameterExpressions = new ParameterExpression[_parameters.Length];
            int i = 0;
            foreach (var item in method.GetParameters())
            {
                parameterExpressions[i++] = Expression.Parameter(item.ParameterType, item.Name);
            }
            var body = Expression.Call(Expression.Constant(callback.Target), method, parameterExpressions);
            var func = Expression.Lambda(body, parameterExpressions);
            _callback = func.Compile();
        }

        public async Task Invoke(RealmPlayer player, CommandArguments arguments, CancellationToken cancellationToken)
        {
            int i = 0;
            var args = new object[_parameters.Length];
            foreach (var parameterInfo in _parameters)
            {
                if(parameterInfo.ParameterType.IsSubclassOf(typeof(Player)))
                {
                    Player? plr = null;
                    foreach (var attribute in parameterInfo.GetCustomAttributes())
                    {
                        if (attribute is CallingPlayerAttribute)
                        {
                            plr = player;
                            break;
                        }
                    }

                    if(plr == null)
                    {
                        var playerSearchOption = PlayerSearchOption.All;
                        var playerSearchOptionsAttribute = parameterInfo.GetCustomAttribute<PlayerSearchOptionsAttribute>();
                        if(playerSearchOptionsAttribute != null)
                        {
                            playerSearchOption = playerSearchOptionsAttribute.PlayerSearchOption;
                        }
                        plr = arguments.ReadPlayer(new(playerSearchOption));
                    }
                    args[i++] = plr;
                }
                else if(parameterInfo.ParameterType == typeof(CancellationToken))
                {
                    args[i++] = cancellationToken;
                }
                else if(parameterInfo.ParameterType == typeof(string))
                {
                    var value = arguments.ReadArgument();
                    args[i++] = value;
                }
                else if(parameterInfo.ParameterType == typeof(short))
                {
                    var value = arguments.ReadShort();
                    foreach (var attribute in parameterInfo.GetCustomAttributes())
                    {
                        if(attribute is RangeAttribute range)
                        {
                            var valid = range.IsValid(value);
                            if(!valid)
                                throw new CommandArgumentException(arguments.CurrentArgument, $"liczba powinna być w zakresie od {range.Minimum} do {range.Maximum}", value.ToString());
                        }
                    }
                    args[i++] = value;
                }
                else if(parameterInfo.ParameterType == typeof(ushort))
                {
                    var value = arguments.ReadUShort();
                    foreach (var attribute in parameterInfo.GetCustomAttributes())
                    {
                        if(attribute is RangeAttribute range)
                        {
                            var valid = range.IsValid(value);
                            if(!valid)
                                throw new CommandArgumentException(arguments.CurrentArgument, $"liczba powinna być w zakresie od {range.Minimum} do {range.Maximum}", value.ToString());
                        }
                    }
                    args[i++] = value;
                }
                else if(parameterInfo.ParameterType == typeof(byte))
                {
                    var value = arguments.ReadByte();
                    foreach (var attribute in parameterInfo.GetCustomAttributes())
                    {
                        if(attribute is RangeAttribute range)
                        {
                            var valid = range.IsValid(value);
                            if(!valid)
                                throw new CommandArgumentException(arguments.CurrentArgument, $"liczba powinna być w zakresie od {range.Minimum} do {range.Maximum}", value.ToString());
                        }
                    }
                    args[i++] = value;
                }
                else if(parameterInfo.ParameterType == typeof(uint))
                {
                    var value = arguments.ReadUInt();
                    foreach (var attribute in parameterInfo.GetCustomAttributes())
                    {
                        if(attribute is RangeAttribute range)
                        {
                            var valid = range.IsValid(value);
                            if(!valid)
                                throw new CommandArgumentException(arguments.CurrentArgument, $"liczba powinna być w zakresie od {range.Minimum} do {range.Maximum}", value.ToString());
                        }
                    }
                    args[i++] = value;
                }
                else if(parameterInfo.ParameterType == typeof(decimal))
                {
                    var value = arguments.ReadDecimal();
                    foreach (var attribute in parameterInfo.GetCustomAttributes())
                    {
                        if(attribute is RangeAttribute range)
                        {
                            var valid = range.IsValid(value);
                            if(!valid)
                                throw new CommandArgumentException(arguments.CurrentArgument, $"liczba powinna być w zakresie od {range.Minimum} do {range.Maximum}", value.ToString());
                        }
                    }
                    args[i++] = value;
                }
                else if(parameterInfo.ParameterType == typeof(float))
                {
                    var value = arguments.ReadFloat();
                    foreach (var attribute in parameterInfo.GetCustomAttributes())
                    {
                        if(attribute is RangeAttribute range)
                        {
                            var valid = range.IsValid(value);
                            if(!valid)
                                throw new CommandArgumentException(arguments.CurrentArgument, $"liczba powinna być w zakresie od {range.Minimum} do {range.Maximum}", value.ToString());
                        }
                    }
                    args[i++] = value;
                }
                else if(parameterInfo.ParameterType == typeof(int))
                {
                    var value = arguments.ReadInt();
                    foreach (var attribute in parameterInfo.GetCustomAttributes())
                    {
                        if(attribute is RangeAttribute range)
                        {
                            var valid = range.IsValid(value);
                            if(!valid)
                                throw new CommandArgumentException(arguments.CurrentArgument, $"liczba powinna być w zakresie od {range.Minimum} do {range.Maximum}", value.ToString());
                        }
                    }
                    args[i++] = value;
                }
            }
            var result = _callback.DynamicInvoke(args);
            if(result is Task task)
            {
                await task;
            }
        }
    }

    private readonly ChatBox _chatBox;
    private readonly ILogger<RealmCommandService> _logger;

    private readonly Dictionary<string, CommandInfo> _commands = [];

    public CommandInfo[] Commands => [.. _commands.Select(x => x.Value)];
    public string[] CommandNames => [.. _commands.Keys.Concat(_commands.Keys)];
    public int Count => _commands.Count;

    public RealmCommandService(ILogger<RealmCommandService> logger, ChatBox chatBox, PlayersEventManager playersEventManager, IEnumerable<IInGameCommand> inGameCommands)
    {
        _logger = logger;
        _chatBox = chatBox;
        playersEventManager.Joined += HandlePlayerJoined;

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
        if (element is RealmPlayer player)
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

    public void AddAsyncCommandHandler(string commandName, Delegate callback, string[]? requiredPolicies = null, string? description = null, string? category = null)
    {
        CheckIfCommandExists(commandName);

        _commands.Add(commandName, new DelegateAsyncCommandInfo(commandName, callback)
        {
            RequiredPolicies = requiredPolicies,
            Description = description,
            Category = category
        });

        if (requiredPolicies != null)
            _logger.LogInformation("Created async command {commandName} with required policies: {requiredPolicies}", commandName, requiredPolicies);
        else
            _logger.LogInformation("Created async command {commandName}", commandName);

    }

    public void AddAsyncCommandHandler(string commandName, Func<RealmPlayer, CommandArguments, CancellationToken, Task> callback, string[]? requiredPolicies = null, string? description = null, string? usage = null, string? category = null)
    {
        CheckIfCommandExists(commandName);

        _commands.Add(commandName, new AsyncCommandInfo(commandName, callback)
        {
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

        _commands.Add(commandName, new SyncCommandInfo(commandName, callback)
        {
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
        _logger.LogInformation("{player} executed command {command} with arguments {commandArguments}.", player, command, arguments);
        try
        {
            var commandArguments = new CommandArguments(player, player.ServiceProvider.GetRequiredService<IElementSearchService>(), arguments);

            if (commandInfo is SyncCommandInfo syncCommandInfo)
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
            }
            else if (commandInfo is AsyncCommandInfo asyncCommandInfo)
            {
                var token = player.CreateCancellationToken();
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
            _logger.LogInformation("Ended async command {command} execution with in {totalMilliseconds}milliseconds", command, (Stopwatch.GetTimestamp() - start) / (float)TimeSpan.TicksPerMillisecond);
        }
    }

    public static readonly ActivitySource Activity = new("RealmCore.Commands", "1.0.0");
}
