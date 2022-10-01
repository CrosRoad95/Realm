namespace Realm.Server.Resources.ClientInterface;

internal class ClientInterfaceResource : ResourceBase, IResourceConstructor
{
    public static string ResourceName => "ClientInterface";

    private ClientInterfaceResource(MtaServer server, Dictionary<string, byte[]> additionalFiles) : base(server, server.GetRequiredService<RootElement>(), ResourceName)
    {
        foreach (var (path, content) in additionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }

    public static ResourceBase Create<TResource>(MtaServer server, Dictionary<string, byte[]> additionalFiles) where TResource : ResourceBase
    {
        return new ClientInterfaceResource(server, additionalFiles);
    }
}
