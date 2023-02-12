using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server;
using Realm.Resources.Assets.Interfaces;
using Microsoft.Extensions.Logging;

namespace Realm.Resources.Assets;

internal class AssetsResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["assets.lua"] = ResourceFiles.Assets,
    };

    internal AssetsResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "Assets")
    {
        var logger = server.GetRequiredService<ILogger<AssetsResource>>();
        var assets = server.GetRequiredService<IEnumerable<IServerAssetsProvider>>().SelectMany(x => x.Provide()).ToList();
        long contentSize = 0;
        foreach (var (path, content) in assets)
        {
            Files.Add(ResourceFileFactory.FromBytes(content, path));
            AdditionalFiles.Add(path, content);
            contentSize += content.Length;
        }
        logger.LogInformation("Loaded {count} assets of total size: {sizeInMB:N2}MB", assets.Count, contentSize / 1024.0f / 1024.0f);

        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));

        Exports.Add("requestAsset");
    }
}
