﻿using Realm.Domain.Sessions;
using Realm.Persistance.Scripting.Classes;
using Realm.Scripting.Extensions;
using SlipeServer.Server.Elements.Events;

namespace Realm.Domain.Elements;

[NoDefaultScriptAccess]
public class RPGFraction : Element, IDisposable, IWorldDebugData
{
    private bool _disposed;
    private readonly bool _isPersistant = PersistantScope.IsPersistant;

    private readonly Guid _debugId = Guid.NewGuid();
    [ScriptMember("debugId")]
    public Guid DebugId => _debugId;
    public PreviewType PreviewType => PreviewType.BoxWireframe;
    public Color PreviewColor => Color.FromArgb(100, 0, 200, 200);

    public string Code { get; [NoScriptAccess] set; } = "";
    public string MemberClaim { get; [NoScriptAccess] set; } = "";
    public new string Name
    {
        get => base.Name; [NoScriptAccess]
        set
        {
            base.Name = value;
        }
    }

    public new Vector3 Position
    {
        get => base.Position; [NoScriptAccess]
        set
        {
            base.Position = value;
        }
    }

    [NoScriptAccess]
    private readonly HashSet<PlayerAccount> _members = new();

    public event Action<Player, FractionSession>? SessionStarted;
    public event Action<Player, FractionSession>? SessionStopped;
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
    public string GetMemberClaimName()
    {
        CheckIfDisposed();
        return $"fraction.{Code}.member";
    }

    [ScriptMember("getLeaderClaimName")]
    public string GetLeaderClaimName()
    {
        CheckIfDisposed();
        return $"fraction.{Code}.leader";
    }

    [ScriptMember("isMember")]
    public bool IsMember(PlayerAccount playerAccount) => playerAccount.HasClaim(GetMemberClaimName());

    public async Task<bool> InternalAddMember(PlayerAccount playerAccount, object[]? permissions)
    {
        string value = "";
        if (permissions != null)
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
    public FractionSession? StartSession(RPGPlayer player)
    {
        if (player.IsDuringSession<FractionSession>())
            return null;

        var fractionSession = new FractionSession(Code, player);
        player.AddSession(fractionSession);
        fractionSession.Start();
        SessionStarted?.Invoke(player, fractionSession);
        player.Disconnected += Player_Disconnected;
        return fractionSession;
    }

    [ScriptMember("stopSession")]
    public bool StopSession(RPGPlayer player, FractionSession fractionSession)
    {
        if (!player.IsDuringSession<FractionSession>())
            return false;
        SessionStopped?.Invoke(player, fractionSession);
        fractionSession.Stop();
        player.RemoveSession(fractionSession);
        player.Disconnected -= Player_Disconnected;
        return true;
    }

    private void Player_Disconnected(Player player, PlayerQuitEventArgs e)
    {
        var fractionSession = ((RPGPlayer)player).GetRequiredSession<FractionSession>();
        StopSession((RPGPlayer)player, fractionSession);
    }

    [ScriptMember("toString")]
    public override string ToString() => Name;

    public void Dispose()
    {
        _disposed = true;
    }
}