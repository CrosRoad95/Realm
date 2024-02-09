namespace RealmCore.Server.Modules.Administration;

internal interface ILuaDebugDataProvider
{
    LuaValue GetLuaDebugData();
}
