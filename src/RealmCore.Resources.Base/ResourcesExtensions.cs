using System.Reflection;
using System.Text;

namespace RealmCore.Resources.Base;

public static class ResourcesExtensions
{
    private static string _hubBaseScript = """
local hub = {};
function hubBind(name, func)
    if(type(name) ~= "string" or type(func) ~= "function")then
        error("Failed to bind event, expected name to be string, got "..type(name).." ("..tostring(name).."), expected callback to be function, got: "..type(func));
    end
    hub[name] = func;
end
""";
    public static void AddLuaEventHub<T>(this Resource resource)
    {
        var type = typeof(T);
        if (!type.IsInterface)
            throw new ArgumentException("T is not interface");

        var methods = type.GetMethods();

        var sb = new StringBuilder(_hubBaseScript);
        sb.Append(Environment.NewLine);
        sb.Append(Environment.NewLine);
        foreach (var methodInfo in methods)
            AddMethod(ref sb, resource.Name, methodInfo);
        var source = sb.ToString();
        resource.NoClientScripts[$"{resource.Name}/eventHub.lua"] = Encoding.UTF8.GetBytes(source);
    }

    private static void AddMethod(ref StringBuilder sb, string baseName, MethodInfo methodInfo)
    {
        var hasParameters = methodInfo.GetParameters().Any();
        var parameters = string.Join(", ", methodInfo.GetParameters().Select(p => p.Name));
        var call = hasParameters ? $"local success, result = pcall(callback, {parameters});" : "local success, result = pcall(callback);";
        var code = $"""
addEvent("internalHub{baseName}{methodInfo.Name}", true);
addEventHandler("internalHub{baseName}{methodInfo.Name}", localPlayer, function({parameters})
    local callbackName = "{methodInfo.Name}";
    local callback = hub[callbackName];
    if(type(callback) ~= "function")then
        error("Failed to invoke hub method: '"..callbackName.."' because no function is bound");
    end
    {call}
    if(not success)then
        error("Failed to invoke hub method: '"..callbackName.."' because: "..tostring(result));
    end
end)

addEvent("internalHubBroadcast{baseName}{methodInfo.Name}", true);
addEventHandler("internalHubBroadcast{baseName}{methodInfo.Name}", root, function({parameters})
    local callbackName = "{methodInfo.Name}";
    local callback = hub[callbackName];
    if(type(callback) ~= "function")then
        error("Failed to invoke hub method: '"..callbackName.."' because no function is bound");
    end
    {call}
    if(not success)then
        error("Failed to invoke hub method: '"..callbackName.."' because: "..tostring(result));
    end
end)

""";
        sb.AppendLine(code);
    }
}
