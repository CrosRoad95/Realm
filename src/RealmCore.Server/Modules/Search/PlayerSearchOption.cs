namespace RealmCore.Server.Modules.Search;

[Flags]
public enum PlayerSearchOption
{
    None = 0,
    CaseInsensitive = 1,
    LoggedIn = 2,
    AllowEmpty = 4,
    All = CaseInsensitive | LoggedIn
}
