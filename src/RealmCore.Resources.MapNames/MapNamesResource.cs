namespace RealmCore.Resources.MapNames;

internal class MapNamesResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["render.lua"] = ResourceFiles.Render,
    };

    internal MapNamesResource(MtaServer server)
        : base(server, server.RootElement, "MapNames")
    {
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}
