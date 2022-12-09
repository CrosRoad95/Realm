using SlipeServer.Server.Resources;

namespace Realm.Resources.Addons.Common;

public class CommonResourceOptions
{
    public CommonResourceOptions()
    {
    }

    public void Configure(Resource resource)
    {
        resource.NoClientScripts["utilities/utilities.lua"] = ResourceFiles.Utilities;
    }
}