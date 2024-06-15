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

    private bool IsNumericType(Type type) => Type.GetTypeCode(type) switch
    {
        TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.Decimal or TypeCode.Double or TypeCode.Single => true,
        _ => false,
    };

    public object ConvertToNumber(string input, TypeCode targetType) => targetType switch
    {
        TypeCode.Byte => Convert.ToByte(input),
        TypeCode.SByte => Convert.ToSByte(input),
        TypeCode.UInt16 => Convert.ToUInt16(input),
        TypeCode.UInt32 => Convert.ToUInt32(input),
        TypeCode.UInt64 => Convert.ToUInt64(input),
        TypeCode.Int16 => Convert.ToInt16(input),
        TypeCode.Int32 => Convert.ToInt32(input),
        TypeCode.Int64 => Convert.ToInt64(input),
        TypeCode.Decimal => Convert.ToDecimal(input),
        TypeCode.Double => Convert.ToDouble(input),
        TypeCode.Single => Convert.ToSingle(input),
        _ => throw new ArgumentException("Unsupported TypeCode", nameof(targetType)),
    };

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
                bool readAll = false;
                foreach (var attribute in parameterInfo.GetCustomAttributes())
                {
                    if (attribute is ReadRestAsStringAttribute)
                    {
                        value = arguments.ReadAllAsString();
                        break;
                    }
                }

                if(!readAll)
                    value = arguments.ReadArgument();
            }
            else if (IsNumericType(parameterInfo.ParameterType))
            {
                if (parameterInfo.HasDefaultValue)
                {
                    if(arguments.TryReadArgument(out string? str) && str != null)
                    {
                        value = ConvertToNumber(str, Type.GetTypeCode(parameterInfo.ParameterType));
                    }
                    else
                    {
                        value = parameterInfo.DefaultValue;
                    }
                }
                else
                {
                    var number = arguments.ReadArgument();
                    value = ConvertToNumber(number, Type.GetTypeCode(parameterInfo.ParameterType));
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