namespace Realm.Server.Scripting;

[NoDefaultScriptAccess]
public class InputScriptingFunctions
{
    private readonly RPGCommandService _commandService;

    public InputScriptingFunctions(RPGCommandService commandService)
    {
        _commandService = commandService;
    }

    [ScriptMember("addCommandHandler")]
    public bool AddCommandHandler(string commandName, ScriptObject callback, ScriptObject? requiredPoliciesObject = null)
    {
        string[]? requiredPolicies = requiredPoliciesObject.ConvertToArrayString();

        return _commandService.AddCommandHandler(commandName, async (player, args) =>
        {
            if (callback.IsAsync())
                await(callback.Invoke(false, player, args.ToScriptArray()) as dynamic);
            else
                callback.Invoke(false, player, args.ToScriptArray(callback.Engine));
        }, requiredPolicies);
    }
}
