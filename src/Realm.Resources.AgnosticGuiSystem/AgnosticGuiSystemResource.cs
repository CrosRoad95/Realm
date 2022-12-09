using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server;

namespace Realm.Resources.AgnosticGuiSystem;

internal class AgnosticGuiSystemResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["controller.lua"] = ResourceFiles.Controller,
    };

    internal AgnosticGuiSystemResource(MtaServer server, AgnosticGuiSystemOptions agnosticGuiSystemOptions)
        : base(server, server.GetRequiredService<RootElement>(), "AgnosticGuiSystem")
    {
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
        
        foreach (var (path, content) in agnosticGuiSystemOptions._providers)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
        
        foreach (var (path, content) in agnosticGuiSystemOptions._guis)
            NoClientScripts[$"{Name}/{path}"] = content;

        foreach (var (path, content) in GetGuiFiles())
            NoClientScripts[$"{Name}/{path}"] = content;
    }

    private IEnumerable<(string, byte[])> GetGuiFiles()
    {
        var files = Directory.GetFiles("Gui");
        foreach (var item in files)
        {
            yield return (item, File.ReadAllBytes(item));
        }
    }
}