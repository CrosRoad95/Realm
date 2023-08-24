using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server;
using RealmCore.Interfaces.Providers;

namespace RealmCore.Resources.GuiSystem;

internal class GuiSystemResource : Resource
{
    private readonly IServerFilesProvider _serverFilesProvider;
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["controller.lua"] = ResourceFiles.Controller,
        ["utilities.lua"] = ResourceFiles.Utilities,
        ["ceguiProvider.lua"] = ResourceFiles.CeGuiProvider,
    };

    internal GuiSystemResource(MtaServer server, GuiSystemOptions GuiSystemOptions) : base(server, server.GetRequiredService<RootElement>(), "GuiSystem")
    {
        _serverFilesProvider = server.GetRequiredService<IServerFilesProvider>();
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));

        foreach (var (path, content) in GuiSystemOptions._providers)
            if (content != null)
                Files.Add(ResourceFileFactory.FromBytes(content, path));

        foreach (var (path, content) in GuiSystemOptions._guis)
            NoClientScripts[$"{Name}/{path}"] = content;

        NoClientScripts[$"{Name}/selectedGuiProvider.lua"] = GuiSystemOptions._selectedGuiProvider ?? throw new Exception("No gui provider selected");

        UpdateGuiFiles();
    }

    private IEnumerable<(string, byte[])> GetGuiFiles()
    {
        var files = _serverFilesProvider.GetFiles("Gui");
        foreach (var item in files)
        {
            yield return (item, File.ReadAllBytes(item));
        }
    }

    public void UpdateGuiFiles()
    {
        foreach (var (path, content) in GetGuiFiles())
            NoClientScripts[$"{Name}/{path}"] = content;
    }
}