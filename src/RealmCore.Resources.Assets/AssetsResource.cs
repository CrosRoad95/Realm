using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server;
using System.Text;
using RealmCore.Resources.Assets.Interfaces;

namespace RealmCore.Resources.Assets;

internal class AssetsResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["assets.lua"] = ResourceFiles.Assets,
    };

    private readonly long _contentSize = 0;
    public long ContentSize => _contentSize;

    private readonly string _decryptScript = """
do
    local key = base64Decode("{0}");
    function decryptAsset(fileName)
        local file = fileOpen(fileName);
        local content = fileRead(file, fileGetSize(file));
        fileClose(file)
	    local decryptedContent = decodeString("aes128", content, {1}
		    key = key,
		    iv = key,
	    {2})
        return decryptedContent
    end
end
""";

    internal AssetsResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "Assets")
    {
        var serverAssetsProviders = server.GetRequiredService<IEnumerable<IServerAssetsProvider>>();
        var encryptionProvider = server.GetRequiredService<IAssetEncryptionProvider>();
        var assetsCollection = server.GetRequiredService<AssetsCollection>();

        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));

        var assets = serverAssetsProviders.SelectMany(x => x.Provide()).ToList();
        foreach (var path in assets)
        {
            if(path.EndsWith(".otf") || path.EndsWith(".ttf"))
            {
                var content = File.ReadAllBytes(path);
                Files.Add(ResourceFileFactory.FromBytes(content, path));
                AdditionalFiles.Add(path, content);
                _contentSize += content.Length;
            }
            else
            {
            using var content = File.OpenRead(path);
                var md5 = Utilities.CreateMD5(content);
                var pathMd5 = Utilities.CreateMD5(path);
                var contentByte = Utilities.ReadFully(content);
                var encrypted = encryptionProvider.Encrypt(contentByte);

                Files.Add(ResourceFileFactory.FromBytes(encrypted, pathMd5));
                AdditionalFiles.Add(pathMd5, encrypted);
                _contentSize += content.Length;
            }
        }

        var keyBase64 = Convert.ToBase64String(encryptionProvider.Key);
        var decryptString = string.Format(_decryptScript, keyBase64, "{", "}");

        if(assetsCollection.ReplacedModels.Any())
        {
            var modelsToReplace = new StringBuilder();
            foreach (var item in assetsCollection.ReplacedModels)
            {
                var asset = assetsCollection.GetAsset<IModel>(item.Value);
                var col = Utilities.CreateMD5(asset.ColPath);
                var dff = Utilities.CreateMD5(asset.DffPath);
                modelsToReplace.AppendLine($"local col = engineLoadCOL(decryptAsset(\"{col}\"));engineReplaceCOL(col,{(int)item.Key});");
                modelsToReplace.AppendLine($"local dff = engineLoadDFF(decryptAsset(\"{dff}\"));engineReplaceModel(dff,{(int)item.Key});");
            }

            NoClientScripts.Add("decrypt.lua", Encoding.UTF8.GetBytes(decryptString));
            NoClientScripts.Add("modelsToReplace.lua", Encoding.UTF8.GetBytes(modelsToReplace.ToString()));
        }

        Exports.Add("requestAsset");
    }
}
