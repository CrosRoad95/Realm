namespace Realm.Server.Gui.Interfaces;

internal interface IGuiFilesProvider
{
    IEnumerable<(string, byte[])> GetFiles();
}
