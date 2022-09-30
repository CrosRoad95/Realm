namespace Realm.Server.Resources.Interfaces;

public interface IResourceConstructor
{
    abstract static string ResourceName { get; }
    abstract static ResourceBase Create<TResource>(MtaServer server, Dictionary<string, byte[]> additionalFiles)
         where TResource : ResourceBase;
}
