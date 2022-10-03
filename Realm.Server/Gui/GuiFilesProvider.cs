using System.IO;

namespace Realm.Server.Gui;

internal class GuiFilesProvider : IGuiFilesProvider
{
    private readonly string _path;

    public GuiFilesProvider(string path)
    {
        _path = path;
    }

    public IEnumerable<(string, byte[])> GetFiles()
    {
        var files = Directory.GetFiles(_path);
        foreach (var item in files)
        {
            yield return (item, File.ReadAllBytes(item));
        }
    }
}
