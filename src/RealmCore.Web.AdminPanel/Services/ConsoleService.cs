namespace RealmCore.Web.AdminPanel.Services;

public class ConsoleService
{
    public event Action<string?>? CommandExecuted;
    public event Action? LogAdded;

    public List<string> Logs { get; } = new List<string>();

    public ConsoleService()
    {
    }

    public void Submit(string inputCommand)
    {
    }
}
