﻿using Microsoft.Extensions.Logging;
using RealmCore.Resources.Base.Extensions;
using SlipeServer.Resources.Base;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using System.Drawing;

namespace RealmCore.Resources.ElementOutline;

internal class ElementOutlineLogic
{
    private struct OutlineInfo
    {
        public Color color;
    }

    private readonly IElementOutlineService _elementOutlineService;
    private readonly ILuaEventHub<IElementOutlineEventHub> _luaEventHub;
    private readonly ILogger<ElementOutlineLogic> _logger;
    private readonly ElementOutlineResource _resource;

    private readonly Dictionary<Element, OutlineInfo> _outlineInfoDictionary = [];

    public ElementOutlineLogic(MtaServer server, IElementOutlineService elementOutlineService, ILuaEventHub<IElementOutlineEventHub> luaEventHub, ILogger<ElementOutlineLogic> logger)
    {

        _resource = server.GetAdditionalResource<ElementOutlineResource>();
        _elementOutlineService = elementOutlineService;
        _luaEventHub = luaEventHub;
        _logger = logger;
        _elementOutlineService.OutlineForPlayerChanged += HandleForPlayerOutlineChanged;
        _elementOutlineService.OutlineForPlayerRemoved += HandleForPlayerOutlineRemoved;
        _elementOutlineService.OutlineChanged += HandleOutlineChanged;
        _elementOutlineService.OutlineRemoved += HandleOutlineRemoved;
        _elementOutlineService.RenderingEnabled += HandleSetRenderingEnabled;
        server.PlayerJoined += HandlePlayerJoin;
    }

    private async void HandlePlayerJoin(Player player)
    {
        try
        {
            await _resource.StartForAsync(player);
            if (_outlineInfoDictionary.Count != 0)
            {
                var elements = _outlineInfoDictionary.Keys.ToArray();
                var colors = _outlineInfoDictionary.Values.Select(x => x.color).ToArray();
                _luaEventHub.Invoke(player, x => x.SetOutlines(elements, colors));
            }
        }
        catch (Exception ex)
        {
            _logger.ResourceFailedToStart<ElementOutlineResource>(ex);
        }
    }

    private void HandleOutlineRemoved(Element element)
    {
        _outlineInfoDictionary.Remove(element);
        _luaEventHub.Broadcast(x => x.RemoveOutline(element));
    }

    private void HandleOutlineChanged(Element element, Color color)
    {
        _outlineInfoDictionary[element] = new OutlineInfo
        {
            color = color,
        };
        _luaEventHub.Broadcast(x => x.SetOutline(color), element);
    }

    private void HandleForPlayerOutlineChanged(Player player, Element element, Color color)
    {
        _luaEventHub.Invoke(player, x => x.SetOutlineForElement(element, color));
    }

    private void HandleForPlayerOutlineRemoved(Player player, Element element)
    {
        _luaEventHub.Invoke(player, x => x.RemoveOutline(element));
    }

    private void HandleSetRenderingEnabled(Player player, bool enabled)
    {
        _luaEventHub.Invoke(player, x => x.SetRenderingEnabled(enabled));
    }
}
