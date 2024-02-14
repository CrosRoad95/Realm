namespace RealmCore.Resources.Browser;

internal class BrowserResource : Resource
{
    private readonly MtaServer _server;
    private readonly string? _directoryPath;

    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["Browser.lua"] = ResourceFiles.Browser,
        ["error.html"] = ResourceFiles.ErrorPage,
    };

    internal BrowserResource(MtaServer server, string? directoryPath)
        : base(server, server.GetRequiredService<RootElement>(), "Browser")
    {
        var browserOptions = server.GetRequiredService<IOptions<BrowserOptions>>();
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
        _server = server;
        _directoryPath = directoryPath;

        if(browserOptions.Value.Mode == BrowserMode.Local)
            IncludeClientsideWebAssemblyFiles();
    }

    private void IncludeClientsideWebAssemblyFiles()
    {
        string currentDir = Directory.GetCurrentDirectory();
        if (_directoryPath != null)
        {
            var filesPath = System.IO.Path.Combine(currentDir, _directoryPath);
            try
            {
                string searchPattern = "*.*";
                string[] files = Directory.GetFiles(_directoryPath, searchPattern, SearchOption.AllDirectories);

                Dictionary<string, byte[]> dlls = [];
                foreach (var file in files)
                {
                    var path = System.IO.Path.GetRelativePath(_directoryPath, file);
                    if (path.EndsWith(".br") || path.EndsWith(".gz"))
                        continue;

                    var content = File.ReadAllBytes(file);
                    if (path.EndsWith(".dll"))
                    {
                        dlls[path] = content;
                    }
                    else
                    {
                        if (path == "index.html")
                        {
                            var indexHtmlString = System.Text.Encoding.UTF8.GetString(content);
                            indexHtmlString = indexHtmlString.Replace("<base href=\"/\" />", "");
                            content = System.Text.Encoding.UTF8.GetBytes(indexHtmlString);
                            // <base href="/" />
                        }
                        AdditionalFiles.Add(path, content);
                        Files.Add(ResourceFileFactory.FromBytes(content, path));
                    }
                }

                string prefixCode = @"function setAjaxHandlers(webBrowser)";
                string suffixCode = @"end";

                byte[] dllsLua = System.Text.Encoding.UTF8.GetBytes(string.Join('\n', dlls.Select(x => $"""
        setBrowserAjaxHandler(webBrowser, "{x.Key.Replace("\\", "/")}",
        	function(get, post)
            return base64Decode("{Convert.ToBase64String(x.Value)}");
        end)
        """
                )));

                byte[] prefixBytes = System.Text.Encoding.UTF8.GetBytes(prefixCode);
                byte[] suffixBytes = System.Text.Encoding.UTF8.GetBytes(suffixCode);

                int dllsLuaLength = dllsLua.Length;
                int prefixLength = prefixBytes.Length;
                int suffixLength = suffixBytes.Length;


                byte[] combinedBytes = new byte[prefixLength + dllsLuaLength + suffixLength];

                Array.Copy(prefixBytes, 0, combinedBytes, 0, prefixLength);
                Array.Copy(dllsLua, 0, combinedBytes, prefixLength, dllsLuaLength);
                Array.Copy(suffixBytes, 0, combinedBytes, prefixLength + dllsLuaLength, suffixLength);


                AdditionalFiles.Add("dlls.lua", combinedBytes);
                Files.Add(ResourceFileFactory.FromBytes(combinedBytes, "dlls.lua"));

            }
            catch (Exception ex)
            {
                _server.GetRequiredService<ILogger<BrowserResource>>().LogError(ex, "Failed to find production files.");
            }
        }
    }
}