namespace RealmCore.Server.Modules.Players.Discord;

public sealed class DiscordRichPresenceFeature : IPlayerFeature
{
    private readonly object _lock = new();
    private readonly DiscordRichPresenceService _discordRichPresenceService;

    public bool IsReady { get;private set; }
    public ulong? UserId { get; private set; }
    public event Action<DiscordRichPresenceFeature>? Ready;
    public RealmPlayer Player { get; init; }

    public DiscordRichPresenceFeature(PlayerContext playerContext, DiscordRichPresenceService discordRichPresenceService)
    {
        Player = playerContext.Player;
        _discordRichPresenceService = discordRichPresenceService;
        _discordRichPresenceService.RichPresenceReady += HandleRichPresenceReady;
    }

    private void HandleRichPresenceReady(Player player, string? stringUserId)
    {
        if (player != Player)
            return;

        lock (_lock)
        {
            if (IsReady)
                return;

            IsReady = true;

            if (ulong.TryParse(stringUserId, out var userId))
            {
                UserId = userId;
            }
        }
        Ready?.Invoke(this);
    }

    public void SetState(string state)
    {
        if (!IsReady)
            return;

        _discordRichPresenceService.SetState(Player, state);
    }

    public void SetDetails(string details)
    {
        if (!IsReady)
            return;

        _discordRichPresenceService.SetDetails(Player, details);
    }

    public void SetAsset(string asset, string assetName)
    {
        if (!IsReady)
            return;

        _discordRichPresenceService.SetAsset(Player, asset, assetName);
    }

    public void SetSmallAsset(string asset, string assetName)
    {
        if (!IsReady)
            return;

        _discordRichPresenceService.SetSmallAsset(Player, asset, assetName);
    }

    public void SetButton(DiscordRichPresenceButton discordRichPresenceButton, string text, Uri uri)
    {
        if (!IsReady)
            return;

        _discordRichPresenceService.SetButton(Player, discordRichPresenceButton, text, uri);
    }

    public void SetPartySize(int size, int max)
    {
        if (!IsReady)
            return;

        _discordRichPresenceService.SetPartySize(Player, size, max);
    }

    public void SetStartTime(int seconds)
    {
        if (!IsReady)
            return;

        _discordRichPresenceService.SetStartTime(Player, seconds);
    }
}
