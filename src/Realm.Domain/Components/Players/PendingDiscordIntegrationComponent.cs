﻿namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class PendingDiscordIntegrationComponent : Component
{
    private string? _discordConnectionCode = null;
    private DateTime? _discordConnectionCodeValidUntil = null;

    public PendingDiscordIntegrationComponent()
    {
    }

    public bool Verify(string code)
    {
        ThrowIfDisposed();

        if (!HasPendingDiscordConnectionCode())
            return false;

        return _discordConnectionCode == code;
    }

    private bool HasPendingDiscordConnectionCode()
    {
        return _discordConnectionCodeValidUntil != null || _discordConnectionCodeValidUntil > DateTime.Now;
    }

    public string? GenerateAndGetDiscordConnectionCode(TimeSpan? validFor = null)
    {
        ThrowIfDisposed();

        _discordConnectionCode = Guid.NewGuid().ToString();
        _discordConnectionCodeValidUntil = DateTime.Now.Add(validFor ?? TimeSpan.FromMinutes(2));
        return _discordConnectionCode;
    }
}