using Realm.Resources.Assets;
using SlipeServer.Server.ServerBuilders;

namespace Realm.Assets;

public static class ServerBuilderExtensions
{
    public static void AddAssets(this ServerBuilder builder)
    {
        builder.AddAssetsResource();
    }
}
