using Serilog.Context;

namespace Realm.Server.Scripting;

public class InputFunctions
{
    private readonly CommandService _commandService;
    private readonly ILogger _logger;

    public InputFunctions(CommandService commandService, ILogger logger)
    {
        _commandService = commandService;
        _logger = logger.ForContext<InputFunctions>();
    }

    private static object[]? ConvertArray(ScriptObject? arg)
    {
        if (arg == null)
            return null;

        dynamic scriptObject = arg;
        if (scriptObject.constructor.name == "Array")
        {
            int length = Convert.ToInt32(scriptObject.length);
            var array = new object[length];
            for (var index = 0; index < length; ++index)
            {
                array[index] = scriptObject[index];
            }
            return array;
        }
        return null;
    }

    public bool AddCommandHandler(string command, ScriptObject callback, ScriptObject? requiredPoliciesObject = null)
    {
        object[]? requiredPolicies = ConvertArray(requiredPoliciesObject);
        if (requiredPolicies != null)
            _logger.Verbose("Created command {commandName} with required policies: {requiredPolicies}", command, requiredPolicies);
        else
            _logger.Verbose("Created command {commandName}", command);
        _commandService.AddCommand(command).Triggered += async (source, args) =>
        {
            var player = (RPGPlayer)args.Player;
            using var playerProperty = LogContext.PushProperty("player", player);
            using var commandNameProperty = LogContext.PushProperty("commandName", command);
            using var commandArgumentProperty = LogContext.PushProperty("commandArguments", args.Arguments);
            if(requiredPolicies != null)
            {
                var account = player.Account;
                if (account == null)
                {
                    _logger.Verbose("{player} failed to execute command {commandName} player is not logged in", player, command);
                    return;
                }

                foreach (var policy in requiredPolicies)
                    if (!await account.AuthorizePolicy(policy.ToString()))
                    {
                        _logger.Verbose("{player} failed to execute command {commandName} because failed to authorize for policy {policy}", player, command, policy);
                        return;
                    }
            }
            if(args.Arguments.Any())
                _logger.Verbose("{player} executed command {commandName} with arguments {commandArguments}.", player);
            else
                _logger.Verbose("{player} executed command {commandName} with no arguments.", player);
            try
            {
                if (callback.IsAsync())
                    await (callback.Invoke(false, player, args.Arguments.ToScriptArray()) as dynamic);
                else
                    callback.Invoke(false, player, args.Arguments.ToScriptArray(callback.Engine));
            }
            catch (ScriptEngineException scriptEngineException)
            {
                var scriptException = scriptEngineException as IScriptEngineException;
                if (scriptException != null)
                {
                    using var errorDetails = LogContext.PushProperty("errorDetails", scriptException.ErrorDetails);
                    _logger.Error(scriptEngineException, "Exception thrown while executing command");
                }
                else
                    _logger.Error(scriptEngineException, "Exception thrown while executing command");
            }
        };
        return true;
    }
}
