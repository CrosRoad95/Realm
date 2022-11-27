namespace Realm.Persistance.Scripting.Classes;

[NoDefaultScriptAccess]
public class PlayerRole : IDisposable
{
    private readonly Role _role;
    private readonly RoleManager<Role> _roleManager;
    private bool _disposed;

    [ScriptMember("name")]
    public string? Name => _role.Name;

    public PlayerRole(Role role, RoleManager<Role> roleManager)
    {
        _role = role;
        _roleManager = roleManager;
    }

    [ScriptMember("delete")]
    public async Task<bool> Delete()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        var result = await _roleManager.DeleteAsync(_role);
        if (result.Succeeded)
        {
            Dispose();
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        _disposed = true;
    }
}
