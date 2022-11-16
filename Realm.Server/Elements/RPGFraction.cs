using Realm.Server.Scripting.Sessions;

namespace Realm.Server.Elements;

[NoDefaultScriptAccess]
public class RPGFraction : IDisposable
{
    private bool _disposed;
    private readonly bool _isPersistant = PersistantScope.IsPersistant;

    public string Code { get; [NoScriptAccess] set; } = "";
    public string Name { get; [NoScriptAccess] set; } = "";
    public string MemberClaim { get; [NoScriptAccess] set; } = "";
    public Vector3 Position { get; [NoScriptAccess] set; }

    [NoScriptAccess]
    private readonly HashSet<PlayerAccount> _members = new();

    public RPGFraction()
    {
    }

    [ScriptMember("getMembers")]
    public object GetMembers() => _members.ToArray().ToScriptArray();

    [ScriptMember("isPersistant")]
    public bool IsPersistant()
    {
        CheckIfDisposed();
        return _isPersistant;
    }

    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    [ScriptMember("getMemberClaimName")]
    public string GetMemberClaimName() => $"fraction.{Code}.member";
    [ScriptMember("getLeaderClaimName")]
    public string GetLeaderClaimName() => $"fraction.{Code}.leader";

    [ScriptMember("isMember")]
    public bool IsMember(PlayerAccount playerAccount) => playerAccount.HasClaim(GetMemberClaimName());

    public async Task<bool> InternalAddMember(PlayerAccount playerAccount, object[]? permissions)
    {
        string value = "";
        if(permissions != null)
        {
            if (permissions.Any(x => x.ToString()?.Contains(",") ?? false))
                return false;
            value = string.Join(',', permissions.Select(x => x.ToString()));
        }
        await playerAccount.AddClaim(GetMemberClaimName(), value);

        return true;
    }

    [ScriptMember("addMember")]
    public async Task<bool> AddMember(PlayerAccount playerAccount, ScriptObject? permissions = null)
    {
        if (IsMember(playerAccount))
            return false;

        return await InternalAddMember(playerAccount, permissions.ConvertArray());
    }

    [ScriptMember("startSession")]
    public bool StartSession(RPGPlayer rpgPlayer)
    {
        if (rpgPlayer.IsDuringSession<FractionSession>())
            return false;

        var session = new FractionSession();
        rpgPlayer.StartSession(session);
        return true;
    }

    public void Dispose()
    {
        _disposed = true;
    }
}
