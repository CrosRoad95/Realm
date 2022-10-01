namespace Realm.Server.Resources;

public class ResourceBase : Resource
{
    public ResourceBase(MtaServer server, RootElement root, string name, string? path = null) : base(server, root, name, path)
    {
    }

    private static byte[] GetLuaFile(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(name);

        if (stream == null)
            throw new FileNotFoundException($"File \"{name}\" not found in embedded resources.");

        byte[] buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        return buffer;
    }

    public static Dictionary<string, byte[]> GetAdditionalFiles<TResource>() where TResource : ResourceBase, IResourceConstructor
    {
        const string basePath = "Realm.Server.Resources";
        var additionalFiles = new Dictionary<string, byte[]>();
        var resourceNameBase = $"{basePath}.{TResource.ResourceName}.Lua.";
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();
        foreach (var name in resourceNames)
        {
            if(name.Contains(resourceNameBase))
                additionalFiles[name.Substring(resourceNameBase.Length)] = GetLuaFile(name);
        }
        return additionalFiles;
    }
}
