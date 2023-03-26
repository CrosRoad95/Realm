using SlipeServer.Server.Resources;
using System.Reflection;
using System.Text;

namespace Realm.Resources.Base;

public static class ResourcesExtensions
{
    private static string _hubBaseScript = """
local hub = {};
function hubBind(name, func)
    hub[name] = func
    iprint("hubBind", name, func);
end
""";
    public static void AddLuaEventHub<T>(this Resource resource)
    {
        var type = typeof(T);
        if(!type.IsInterface)
            throw new ArgumentException("T is not interface");

        var methods = type.GetMethods();

        var sb = new StringBuilder(_hubBaseScript);
        sb.Append(Environment.NewLine);
        sb.Append(Environment.NewLine);
        foreach (var methodInfo in methods)
            AddMethod(ref sb, resource.Name, methodInfo);
        var source = sb.ToString();
        resource.NoClientScripts[$"{resource.Name}/eventHub.lua"] = System.Text.UTF8Encoding.UTF8.GetBytes(source);
    }

    private static void AddMethod(ref StringBuilder sb, string baseName, MethodInfo methodInfo)
    {
        var parameters = string.Join(", ", methodInfo.GetParameters().Select(p => p.Name));
        var code = $"""
addEvent("internalHub{baseName}{methodInfo.Name}", true)
addEventHandler("internalHub{baseName}{methodInfo.Name}", localPlayer, function({parameters})
    local callbackName = "{methodInfo.Name}";
    local callback = hub[callbackName];
    if(type(callback) ~= "function")then
        error("Failed to invoke hub method: '{methodInfo.Name}' because no function is bound")
    end
    local success, result = pcall(callback, {parameters})
    if(not success)then
        error("Failed to invoke hub method: '{methodInfo.Name}' because: "..tostring(result))
    end
end)
""";
        sb.AppendLine(code);
    }
}
