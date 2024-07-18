namespace RealmCore.Resources.Assets;

internal class AssetsResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = [];
    public long ContentSize { get; private set; }

    private readonly string _decryptScriptProd = """
do
    local key = base64Decode("{0}");
    local excludedExtensions = {{ {1} }}
    function decryptAsset(fileName)
        local file = fileOpen(fileName);
        local content = fileRead(file, fileGetSize(file));
        fileClose(file)
        for i,v in ipairs(excludedExtensions)do
            if(string.find(fileName, v))then
                return content;
            end
        end

	    local decryptedContent = decodeString("aes128", content, {2}
		    key = key,
		    iv = key,
	    {3})
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
        var isEncryptionEnabled = assetEncryptionProvider.IsEncryptionEnabled();

        var basePath = assetsCollection.BasePath;
        foreach (var filePath in assetsCollection.GetAllFiles())
        {
            var fullFilePath = System.IO.Path.Combine(basePath, filePath);
            var extension = System.IO.Path.GetExtension(filePath);

            var path = filePath;
            var content = File.ReadAllBytes(fullFilePath);
            if (isEncryptionEnabled && assetEncryptionProvider.ShouldEncryptByExtension(extension))
            {
                path = assetEncryptionProvider.TryEncryptPath(path);
                content = assetEncryptionProvider.Encrypt(content);
            }

            Files.Add(ResourceFileFactory.FromBytes(content, path));
            AdditionalFiles.Add(path, content);
            ContentSize += content.Length;
        }

        string decryptScript;
        if (isEncryptionEnabled)
        {
            var excludedExtensions = string.Join(", ", assetEncryptionProvider.ExcludedExceptions.Select(x => $"\"{x}\""));
            decryptScript = string.Format(_decryptScriptProd, assetEncryptionProvider.Key, excludedExtensions, "{", "}");
        }
        else
        {
            decryptScript = _decryptScriptDev;
        }

        NoClientScripts.Add("decrypt.lua", Encoding.UTF8.GetBytes(decryptScript));
    }
}
