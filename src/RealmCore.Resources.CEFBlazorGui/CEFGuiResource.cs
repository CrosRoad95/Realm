using SlipeServer.Server.Elements;
using SlipeServer.Server;
using SlipeServer.Server.Resources;
using Microsoft.Extensions.Logging;

namespace RealmCore.Resources.CEFBlazorGui;

internal class CEFBlazorGuiResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["cefBlazorGui.lua"] = ResourceFiles.CEFBlazorGui,
        ["error.html"] = ResourceFiles.ErrorPage,
    };

    internal CEFBlazorGuiResource(MtaServer server, string directoryPath)
        : base(server, server.GetRequiredService<RootElement>(), "CEFBlazorGui")
    {

        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
        try
        {
            string searchPattern = "*.*";
            string[] files = Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);

            Dictionary<string, byte[]> dlls = new();
            foreach (var file in files)
            {
                var path = System.IO.Path.GetRelativePath(directoryPath, file);
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
            server.GetRequiredService<ILogger<CEFBlazorGuiResource>>().LogError(ex, "Failed to find production files, fallback to dev.");
            server.GetRequiredService<ICEFBlazorGuiService>().CEFGuiMode = CEFGuiBlazorMode.Dev;
        }
    }
}