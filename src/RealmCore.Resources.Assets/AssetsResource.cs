namespace RealmCore.Resources.Assets;

internal class AssetsResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = [];
    public long ContentSize { get; private set; }

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
        : base(server, server.RootElement, "Assets")
    {
        Exports.Add("requestAsset");
    }

    public void AddFiles(IAssetEncryptionProvider assetEncryptionProvider, AssetsCollection assetsCollection)
    {
        foreach (var path in assetsCollection.GetAllFiles())
        {
            var extension = System.IO.Path.GetExtension(path);
            switch (extension)
            {
                case ".otf":
                case ".ttf":
                    {
                        var content = File.ReadAllBytes(path);
                        Files.Add(ResourceFileFactory.FromBytes(content, path));
                        AdditionalFiles.Add(path, content);
                        ContentSize += content.Length;
                    }
                    break;
                default:
                    {
                        using var content = File.OpenRead(path);
                        var checksum = content.CalculateChecksum();
                        var pathMd5 = path.CalculateChecksum();
                        var contentBytes = content.ToArray();
                        var encrypted = assetEncryptionProvider.Encrypt(contentBytes);

                        Files.Add(ResourceFileFactory.FromBytes(encrypted, pathMd5));
                        AdditionalFiles.Add(pathMd5, encrypted);
                        ContentSize += content.Length;
                    }
                    break;
            }
        }

        var keyBase64 = Convert.ToBase64String(assetEncryptionProvider.Key);
        var decryptString = string.Format(_decryptScript, keyBase64, "{", "}");

        if (assetsCollection.ReplacedModels.Any())
        {
            //var modelsToReplace = new StringBuilder();
            //foreach (var item in assetsCollection.ReplacedModels)
            //{
            //    var asset = assetsCollection.GetAsset<IModel>(item.Value);
            //    var col = Utilities.CreateMD5(asset.ColPath);
            //    var dff = Utilities.CreateMD5(asset.DffPath);
            //    modelsToReplace.AppendLine($"local col = engineLoadCOL(decryptAsset(\"{col}\"));engineReplaceCOL(col,{(int)item.Key});");
            //    modelsToReplace.AppendLine($"local dff = engineLoadDFF(decryptAsset(\"{dff}\"));engineReplaceModel(dff,{(int)item.Key});");
            //}

            NoClientScripts.Add("decrypt.lua", Encoding.UTF8.GetBytes(decryptString));
            //NoClientScripts.Add("modelsToReplace.lua", Encoding.UTF8.GetBytes(modelsToReplace.ToString()));
        }

    }
}
