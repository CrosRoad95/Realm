namespace RealmCore.Interfaces.Server;

public interface IGuiFilesProvider
{
    IEnumerable<(string, byte[])> GetFiles();
}
