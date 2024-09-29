using System.Text;

namespace RealmCore.Resources.Base.Extensions;

public static class ResourcesExtensions
{
    public static void AddCommonCode(this Resource resource)
    {
        resource.NoClientScripts.Add("_positionContext.lua", Encoding.UTF8.GetBytes(PositionContext.LuaControllerCode));
    }
}
