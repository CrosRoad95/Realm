namespace RealmCore.Server.Modules.Commands;

public class CommandArguments
{
    private readonly string[] _args;
    private readonly RealmPlayer _player;
    private readonly PlayerSearchService _searchService;
    private int _index;

    public int Index => _index;
    public int CurrentArgument => _index - 1;

    public CommandArguments(RealmPlayer player, PlayerSearchService searchService, string[] args)
    {
        _args = args;
        _player = player;
        _searchService = searchService;
    }

    public string ReadAllAsString()
    {
        return string.Join(' ', _args.Skip(_index));
    }

    public string ReadArgument()
    {
        if (_index >= 0 && _index < _args.Length)
        {
            var value = _args[_index++];
            if (string.IsNullOrWhiteSpace(value))
                throw new CommandArgumentException(CurrentArgument, "Argument jest pusty.", null);
            return value;
        }
        else
        {
            throw new CommandArgumentException(null, "Zbyt mało argumentów.", null);
        }
    }

    private bool TryReadArgument(out string? argument)
    {
        if (_index >= 0 && _index < _args.Length)
        {
            var value = _args[_index++];
            if (string.IsNullOrWhiteSpace(value))
            {
                argument = default;
                return false;
            }
            argument = value;
            return true;
        }
        else
        {
            argument = default;
            return false;
        }
    }

    public bool TryReadArgument<T>(out T? argument)
    {
        if (TryReadArgument(out string? arg) && arg != null)
        {
            try
            {
                argument = (T)Convert.ChangeType(arg, typeof(T));
                return true;
            }
            catch (Exception)
            {
                throw new CommandArgumentException(CurrentArgument, "Podany argument jest nie poprawny.", null);
            }
        }
        argument = default;
        return false;
    }

    public virtual RealmPlayer ReadPlayer(PlayerSearchOptions playerSearchOptions = default)
    {
        var name = ReadArgument();
        var users = _searchService.SearchPlayers(name, playerSearchOptions).ToList();
        if (users.Count == 1)
            return users[0];
        if (users.Count > 0)
            throw new CommandArgumentException(CurrentArgument, "Znaleziono więcej niż 1 gracza o takiej nazwie", name);
        throw new CommandArgumentException(CurrentArgument, "Gracz o takiej nazwie nie został znaleziony", name);
    }
}
