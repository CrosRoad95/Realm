﻿using Realm.Resources.AdminTools.Enums;
using Realm.Resources.AdminTools.Interfaces;

namespace Realm.Server.Elements;

[NoDefaultScriptAccess]
public class RPGSpawn : Element, IDisposable, IWorldDebugData
{
    private bool _disposed = false;
    private readonly AuthorizationPoliciesProvider _authorizationPoliciesProvider;
    private string? _id;
    private ILogger _logger;

    private readonly Guid _debugId = Guid.NewGuid();
    [ScriptMember("debugId")]
    public Guid DebugId => _debugId;
    public PreviewType PreviewType => PreviewType.BoxWireframe;
    public Color PreviewColor => Color.FromArgb(100, 0, 200, 0);

    private readonly bool _isPersistant = PersistantScope.IsPersistant;
    private readonly List<string> _requiredPolices = new();

    public RPGSpawn(AuthorizationPoliciesProvider authorizationPoliciesProvider, ILogger logger)
    {
        _authorizationPoliciesProvider = authorizationPoliciesProvider;
        Position = position;
        Rotation = rotation;
        Destroyed += e => Dispose();

        _logger = logger
            .ForContext<RPGSpawn>()
            .ForContext(new SpawnEnricher(this));
    }

    [ScriptMember("isPersistant")]
    public bool IsPersistant()
    {
        CheckIfDisposed();
        return _isPersistant;
    }

    [ScriptMember("addRequiredPolicy")]
    public bool AddRequiredPolicy(string policy)
    {
        CheckIfDisposed();
        _authorizationPoliciesProvider.ValidatePolicy(policy);

        if (_requiredPolices.Contains(policy))
            return false;
        _requiredPolices.Add(policy);
        return true;
    }

    [ScriptMember("removeRequiredPolicy")]
    public bool RemoveRequiredPolicy(string policy)
    {
        CheckIfDisposed();
        _authorizationPoliciesProvider.ValidatePolicy(policy);

        if (!_requiredPolices.Contains(policy))
            return false;
        _requiredPolices.Remove(policy);
        return true;
    }

    [ScriptMember("isAuthorized")]
    public async Task<bool> IsAuthorized(RPGPlayer player)
    {
        CheckIfDisposed();
        if(player.Account == null)
            return false;

        foreach (var policyName in _requiredPolices)
        {
            var result = await player.Account.AuthorizePolicy(policyName);
            if (!result)
                return false;
        }
        return true;
    }

    [ScriptMember("longUserFriendlyName")]
    public string LongUserFriendlyName() => Name;
    [ScriptMember("toString")]
    public override string ToString() => Name;

    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    public void Dispose()
    {
        _disposed = true;
    }
}