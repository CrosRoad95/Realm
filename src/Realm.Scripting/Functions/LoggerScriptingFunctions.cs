namespace Realm.Scripting.Functions;

[NoDefaultScriptAccess]
public class LoggerScriptingFunctions
{
    private readonly ILogger _logger;

    public LoggerScriptingFunctions(ILogger logger)
    {
        _logger = logger.ForContext("javascript", true);
    }

    [ScriptMember("information")]
    public void Information(string messageTemplate)
    {
        _logger.Information(messageTemplate);
    }

    [ScriptMember("information")]
    public void Information(string messageTemplate, params object?[]? propertyValues)
    {
        _logger.Information(messageTemplate, propertyValues);
    }

    [ScriptMember("toString")]
    public override string ToString() => "Logger";
}
