namespace Realm.Persistance.Scripting.Classes;

public class PlayerRole : IDisposable
{
    private readonly Role _role;
    private readonly RoleManager<Role> _roleManager;
    private bool _disposed;

    public string Name => _role.Name;

    public PlayerRole(Role role, RoleManager<Role> roleManager)
    {
        _role = role;
        _roleManager = roleManager;
    }

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


    [NoScriptAccess]
    public void Dispose()
    {
        _disposed = true;
    }
}
