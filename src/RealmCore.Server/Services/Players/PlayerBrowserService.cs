﻿using RealmCore.Resources.Browser;

namespace RealmCore.Server.Services.Players;

internal sealed class PlayerBrowserService : IPlayerBrowserService, IDisposable
{
    public event Action<string, bool>? PathChanged;
    public event Action<bool>? DevToolsStateChanged;
    public event Action<bool>? VisibleChanged;

    private readonly IBrowserGuiService _browserGuiService;
    private readonly IBrowserService _browserService;

    private string _path = "/";
    public string Path
    {
        get => _path; set
        {
            PathChanged?.Invoke(value, false);
            _path = value;
        }
    }

    private bool _devTools;
    public bool DevTools
    {
        get => _devTools; set
        {
            if (_devTools != value)
            {
                _devTools = value;
                DevToolsStateChanged?.Invoke(value);
                _browserService.ToggleDevTools(Player, value);
            }
        }
    }

    private bool _visible;

    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible != value)
            {
                _visible = value;
                VisibleChanged?.Invoke(value);
                _browserService.SetVisible(Player, value);
            }
        }
    }

    public RealmPlayer Player { get; private set; }

    public PlayerBrowserService(IBrowserGuiService browserGuiService, IBrowserService browserService, PlayerContext playerContext)
    {
        var key = browserGuiService.GenerateKey();
        browserGuiService.AuthorizePlayer(key, playerContext.Player);
        _browserGuiService = browserGuiService;
        _browserService = browserService;
        Player = playerContext.Player;
        browserService.BrowserStarted += HandleBrowserStarted;
    }

    private void HandleBrowserStarted(Player player)
    {
        if (player != Player)
            return;

        var key = _browserGuiService.GenerateKey();
        if (_browserGuiService.AuthorizePlayer(key, Player))
        {
            var url = $"/realmGuiInitialize?{_browserGuiService.KeyName}={key}";
            SetPath(url, true);
        }
    }

    public void SetPath(string path, bool clientSide = false)
    {
        PathChanged?.Invoke(path, clientSide);
        _path = path;
        if (clientSide)
            _browserService.SetPath(Player, _path);
    }

    /// <summary>
    /// Server side only
    /// </summary>
    /// <param name="path"></param>
    public void Open(string path)
    {
        Path = path;
        Visible = true;
    }

    public void Close()
    {
        Visible = false;
    }

    public void Dispose()
    {
        _browserGuiService.UnauthorizePlayer(Player);
    }
}