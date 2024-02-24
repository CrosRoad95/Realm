namespace RealmCore.Server.Modules.Search;

public class NotSupportedSwitchException : Exception
{
    private readonly SearchSwitch _searchSwitch;

    public NotSupportedSwitchException(SearchSwitch searchSwitch)
    {
        _searchSwitch = searchSwitch;
    }
}
