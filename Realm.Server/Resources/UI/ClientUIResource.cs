namespace Realm.Server.Resources.UI;

internal class ClientUIResource : ResourceBase, IResourceConstructor
{
    public static string ResourceName => "UI";

    private ClientUIResource(MtaServer server, Dictionary<string, byte[]> additionalFiles) : base(server, server.GetRequiredService<RootElement>(), ResourceName)
    {
        foreach (var (path, content) in additionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }

    public static ResourceBase Create<TResource>(MtaServer server, Dictionary<string, byte[]> additionalFiles) where TResource : ResourceBase
    {
        return new ClientUIResource(server, additionalFiles);
    }
}
