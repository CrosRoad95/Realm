using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Services;
using System.Drawing;

namespace Realm.Resources.AdminTools;

internal class AdminToolsLogic
{
    private readonly AdminToolsResource _resource;
    private readonly LuaEventService _luaEventService;

    public AdminToolsLogic(MtaServer mtaServer, LuaEventService luaEventService, AdminToolsService adminToolsService)
    {
        mtaServer.PlayerJoined += HandlePlayerJoin;
        _resource = mtaServer.GetAdditionalResource<AdminToolsResource>();
        _luaEventService = luaEventService;
        adminToolsService.AdminToolsDisabled += AdminToolsService_AdminToolsDisabled;
        adminToolsService.AdminToolsEnabled += AdminToolsService_AdminToolsEnabled;
    }

    private void AdminToolsService_AdminToolsEnabled(Player player)
    {
        _luaEventService.TriggerEventFor(player, "internalSetAdminToolsEnabled", player, true);
    }

    private void AdminToolsService_AdminToolsDisabled(Player player)
    {
        _luaEventService.TriggerEventFor(player, "internalSetAdminToolsEnabled", player, false);
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }
}