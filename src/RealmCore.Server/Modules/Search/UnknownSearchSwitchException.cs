namespace RealmCore.Server.Modules.Search;

public class UnknownSearchSwitchException : Exception
{
    private readonly string _searchSwitch;

    public UnknownSearchSwitchException(string searchSwitch)
    {
        _searchSwitch = searchSwitch;
    }
}
