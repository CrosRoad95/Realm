namespace RealmCore.Server.Modules.Groups;

public class GroupShortcutInUseException : Exception
{
    public string Shortcut { get; }

    public GroupShortcutInUseException(string shortcut) : base($"Shortcut '{shortcut}' is already in use")
    {
        Shortcut = shortcut;
    }
}
