namespace RealmCore.Server.Modules.Commands;

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

internal abstract class DelegateCommandInfoBase : CommandInfo
{
    public override bool IsAsync => true;
    protected readonly Delegate _callback;
    private readonly ParameterInfo[] _parameters;

    public DelegateCommandInfoBase(string commandName, Delegate callback) : base(commandName)
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

    private Player GetPlayer(ParameterInfo parameterInfo, CommandArguments arguments, RealmPlayer callingPlayer)
    {
        Player? plr = null;
        foreach (var attribute in parameterInfo.GetCustomAttributes())
        {
            if (attribute is CallingPlayerAttribute)
            {
                plr = callingPlayer;
                break;
            }
        }

        if (plr == null)
        {
            var playerSearchOption = PlayerSearchOption.All;
            var playerSearchOptionsAttribute = parameterInfo.GetCustomAttribute<PlayerSearchOptionsAttribute>();
            if (playerSearchOptionsAttribute != null)
            {
                playerSearchOption = playerSearchOptionsAttribute.PlayerSearchOption;
            }
            plr = arguments.ReadPlayer(new(playerSearchOption));
        }
        return plr;

    }
    protected object[] GetArgs(RealmPlayer player, CommandArguments arguments, CancellationToken cancellationToken)
    {
        int i = 0;
        var args = new object[_parameters.Length];
        object? value = null;

        foreach (var parameterInfo in _parameters)
        {
            if (parameterInfo.ParameterType.IsSubclassOf(typeof(RealmPlayer)) || parameterInfo.ParameterType == typeof(RealmPlayer))
            {
                value = GetPlayer(parameterInfo, arguments, player);
            }
            else if (parameterInfo.ParameterType == typeof(CancellationToken))
            {
                value = cancellationToken;
            }
            else if (parameterInfo.ParameterType == typeof(string))
            {
                value = arguments.ReadArgument();
            }
            else if (parameterInfo.ParameterType == typeof(short))
            {
                value = arguments.ReadShort();
            }
            else if (parameterInfo.ParameterType == typeof(ushort))
            {
                value = arguments.ReadUShort();
            }
            else if (parameterInfo.ParameterType == typeof(byte))
            {
                value = arguments.ReadByte();
            }
            else if (parameterInfo.ParameterType == typeof(uint))
            {
                value = arguments.ReadUInt();
            }
            else if (parameterInfo.ParameterType == typeof(decimal))
            {
                value = arguments.ReadDecimal();
            }
            else if (parameterInfo.ParameterType == typeof(float))
            {
                value = arguments.ReadFloat();
            }
            else if (parameterInfo.ParameterType == typeof(int))
            {
                if (parameterInfo.HasDefaultValue)
                {
                    if(arguments.TryReadInt(out int intValue))
                    {
                        value = intValue;
                    }
                    else
                    {
                        value = (int)parameterInfo.DefaultValue;
                    }
                }
                else
                {
                    value = arguments.ReadInt();
                }
            }
            else
            {
                throw new CommandArgumentException(arguments.CurrentArgument, $"Podany typ argumentu nie jest wspierany (typ: {parameterInfo.ParameterType.Name}).", null);
            }

            foreach (var attribute in parameterInfo.GetCustomAttributes())
            {
                if (attribute is RangeAttribute range)
                {
                    var valid = range.IsValid(value);
                    if (!valid)
                        throw new CommandArgumentException(arguments.CurrentArgument, $"liczba powinna być w zakresie od {range.Minimum} do {range.Maximum}", value?.ToString());
                }
            }

            args[i++] = value;
        }

        return args;
    }

}

internal class DelegateAsyncCommandInfo : DelegateCommandInfoBase
{
    public override bool IsAsync { get; } = true;
    public DelegateAsyncCommandInfo(string commandName, Delegate callback) : base(commandName, callback)
    {
    }

    public async Task Invoke(RealmPlayer player, CommandArguments arguments, CancellationToken cancellationToken)
    {
        var args = GetArgs(player, arguments, cancellationToken);
        var result = _callback.DynamicInvoke(args);
        if (result is Task task)
        {
            await task;
        }
    }
}

internal class DelegateSyncCommandInfo : DelegateCommandInfoBase
{
    public DelegateSyncCommandInfo(string commandName, Delegate callback) : base(commandName, callback)
    {
    }

    public void Invoke(RealmPlayer player, CommandArguments arguments, CancellationToken cancellationToken)
    {
        var args = GetArgs(player, arguments, cancellationToken);
        _callback.DynamicInvoke(args);
    }
}