using Newtonsoft.Json.Linq;
using Realm.Server.Elements;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Mappers;
using System;
using System.Security.Principal;
using System.Threading;

namespace Realm.Server.Logic;

internal class RPGPlayerLogic
{
    private readonly EventScriptingFunctions _eventFunctions;
    private readonly DebugLog _debugLog;
    private readonly LuaValueMapper _luaValueMapper;
    private readonly LuaEventService _luaEventService;
    private readonly ChatBox _chatBox;
    private readonly AgnosticGuiSystemService _agnosticGuiSystemService;
    private readonly ILogger _logger;

    public RPGPlayerLogic(MtaServer mtaServer, EventScriptingFunctions eventFunctions, DebugLog debugLog, ILogger logger,
            LuaValueMapper luaValueMapper, LuaEventService luaEventService, ChatBox chatBox, AgnosticGuiSystemService agnosticGuiSystemService)
    {
        mtaServer.PlayerJoined += MtaServer_PlayerJoined;
        _eventFunctions = eventFunctions;
        _debugLog = debugLog;
        _luaValueMapper = luaValueMapper;
        _luaEventService = luaEventService;
        _chatBox = chatBox;
        _agnosticGuiSystemService = agnosticGuiSystemService;
        _logger = logger.ForContext<RPGPlayerLogic>();
    }

    private void MtaServer_PlayerJoined(Player player)
    {
        var rpgPlayer = (RPGPlayer)player;
        rpgPlayer.LoggedIn += RpgPlayer_LoggedIn;
        rpgPlayer.LoggedOut += RpgPlayer_LoggedOut;
        rpgPlayer.Spawned += RpgPlayer_Spawned;
        rpgPlayer.SpawnedAtPosition += RpgPlayer_SpawnedAtPosition;
        rpgPlayer.DebugViewStateChanged += RpgPlayer_DebugViewStateChanged;
        rpgPlayer.EventTriggered += RpgPlayer_EventTriggered;
        rpgPlayer.Disconnected += RpgPlayer_Disconnected;
        rpgPlayer.ChatMessageSend += RpgPlayer_ChatMessageSend;
        rpgPlayer.ClearChatRequested += RpgPlayer_ClearChatRequested;
        rpgPlayer.GuiOpened += RpgPlayer_GuiOpened;
        rpgPlayer.GuiClosed += RpgPlayer_GuiClosed;
        rpgPlayer.AllGuiClosed += RpgPlayer_AllGuiClosed;
    }

    private void RpgPlayer_AllGuiClosed(RPGPlayer rpgPlayer)
    {
        _agnosticGuiSystemService.CloseAllGuis(rpgPlayer);
        _logger.Verbose("Closed all guis");
    }

    private void RpgPlayer_GuiClosed(RPGPlayer rpgPlayer, string guiName)
    {
        var success = _agnosticGuiSystemService.CloseGui(rpgPlayer, guiName);
        if (success)
            _logger.Verbose("Closed gui {gui}", guiName);
        else
            _logger.Verbose("Failed to close gui {gui}", guiName);
    }

    private void RpgPlayer_GuiOpened(RPGPlayer rpgPlayer, string guiName)
    {
        var success = _agnosticGuiSystemService.OpenGui(rpgPlayer, guiName);
        if (success)
            _logger.Verbose("Opened gui {gui}", guiName);
        else
            _logger.Verbose("Failed to open gui {gui}", guiName);
    }

    private void RpgPlayer_ChatMessageSend(RPGPlayer player, string message, Color? color, bool? colorCoded)
    {
        _chatBox.OutputTo(player, message, color ?? Color.White, colorCoded ?? false);
    }

    private void RpgPlayer_ClearChatRequested(RPGPlayer player)
    {
        _chatBox.ClearFor(player);
    }

    private async void RpgPlayer_Disconnected(Player player, PlayerQuitEventArgs e)
    {
        var rpgPlayer = (RPGPlayer)player;
        using var _ = LogContext.Push(new RPGPlayerEnricher(rpgPlayer));
        if (await rpgPlayer.LogOut())
        {
            _logger.Verbose("Logged out from account: {account}", rpgPlayer.Account);
        }
        _logger.Verbose("Disconnected");
    }


    private void RpgPlayer_DebugViewStateChanged(RPGPlayer rpgPlayer, bool enabled)
    {
        using var _ = LogContext.Push(new RPGPlayerEnricher(rpgPlayer));
        if (enabled)
            _logger.Verbose("Enabled debug view");
        else
            _logger.Verbose("Disabled debug view");
        _debugLog.SetVisibleTo(rpgPlayer, enabled);
    }

    private async void RpgPlayer_Spawned(RPGPlayer rpgPlayer, RPGSpawn spawn)
    {
        using var _ = LogContext.Push(new RPGPlayerEnricher(rpgPlayer));
        using var playerSpawnedEvent = new PlayerSpawnedEvent(rpgPlayer, spawn);
        await _eventFunctions.InvokeEvent(playerSpawnedEvent);
        _logger.Verbose("Spawned at {spawn}", spawn);
    }
    
    private async void RpgPlayer_SpawnedAtPosition(RPGPlayer rpgPlayer, Vector3 position)
    {
        using var _ = LogContext.Push(new RPGPlayerEnricher(rpgPlayer));
        using var playerSpawnedEvent = new PlayerSpawnedEvent(rpgPlayer, position);
        await _eventFunctions.InvokeEvent(playerSpawnedEvent);
        _logger.Verbose("Spawned at {position}", position);
    }

    private async void RpgPlayer_LoggedOut(RPGPlayer rpgPlayer, string id)
    {
        using var _ = LogContext.Push(new RPGPlayerEnricher(rpgPlayer));
        using var playerLoggedOutEvent = new PlayerLoggedOutEvent(rpgPlayer);
        await _eventFunctions.InvokeEvent(playerLoggedOutEvent);
    }

    private async void RpgPlayer_LoggedIn(RPGPlayer rpgPlayer, PlayerAccount account)
    {
        using var _ = LogContext.Push(new RPGPlayerEnricher(rpgPlayer));
        rpgPlayer.TriggerClientEvent(ClientEventsNames.ON_LOGGED_IN);
        using var playerLoggedInEvent = new PlayerLoggedInEvent(rpgPlayer, account);
        await _eventFunctions.InvokeEvent(playerLoggedInEvent);
        _logger.Verbose("Logged in to the account: {account}", account);
    }

    // TODO: improve
    public void RpgPlayer_EventTriggered(RPGPlayer rpgPlayer, string name, object[] values)
    {
        using var _ = LogContext.Push(new RPGPlayerEnricher(rpgPlayer));
        LuaValue luaValue;
        if (values.Length == 1 && values[0].GetType() == typeof(object[]))
        {
            luaValue = ((object[])values[0]).Select(_luaValueMapper.Map).ToArray();
        }
        else
        {
            luaValue = values.Select(_luaValueMapper.Map).ToArray();
        }

        if (values.Any())
            _logger.Verbose("Triggered client event {eventName} with arguments: {luaValue}.", name, luaValue);
        else
            _logger.Verbose("Triggered client event {eventName} with no arguments.", name);
        _luaEventService.TriggerEventFor(rpgPlayer, name, rpgPlayer, luaValue);
    }
}
