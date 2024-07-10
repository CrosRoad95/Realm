namespace RealmCore.Resources.Assets;

internal class AssetsResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = [];
    public long ContentSize { get; private set; }

    private readonly string _decryptScriptProd = """
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

    private readonly string _decryptScriptDev = """
do
    function decryptAsset(fileName)
        local file = fileOpen(fileName);
        local content = fileRead(file, fileGetSize(file));
        fileClose(file)
	    return content;
    end
end
""";

    internal AssetsResource(MtaServer server)
        : base(server, server.RootElement, "Assets")
    {
        Exports.Add("requestAsset");
    }

    public void AddFiles(IAssetEncryptionProvider assetEncryptionProvider, AssetsCollection assetsCollection, IHostEnvironment hostEnvironment)
    {
        var basePath = assetsCollection.BasePath;
        foreach (var filePath in assetsCollection.GetAllFiles())
        {
            var fullFilePath = System.IO.Path.Combine(basePath, filePath);
            var extension = System.IO.Path.GetExtension(filePath);
            switch (extension)
            {
                case ".otf":
                case ".ttf":
                    {
                        var content = File.ReadAllBytes(fullFilePath);
                        Files.Add(ResourceFileFactory.FromBytes(content, filePath));
                        AdditionalFiles.Add(filePath, content);
                        ContentSize += content.Length;
                    }
                    break;
                default:
                    {
                        using var content = File.OpenRead(fullFilePath);
                        var checksum = content.CalculateChecksum();
                        var encryptedPath = assetEncryptionProvider.EncryptPath(filePath);
                        var contentBytes = content.ToArray();
                        var encrypted = assetEncryptionProvider.Encrypt(contentBytes);

                        Files.Add(ResourceFileFactory.FromBytes(encrypted, encryptedPath));
                        AdditionalFiles.Add(encryptedPath, encrypted);
                        ContentSize += content.Length;
                    }
                    break;
            }
        }

        string decryptScript;
        if (assetEncryptionProvider.IsEncryptionEnabled())
        {
            decryptScript = string.Format(_decryptScriptProd, assetEncryptionProvider.Key, "{", "}");
        }
        else
        {
            decryptScript = _decryptScriptDev;
        }

        NoClientScripts.Add("decrypt.lua", Encoding.UTF8.GetBytes(decryptScript));
    }
}
