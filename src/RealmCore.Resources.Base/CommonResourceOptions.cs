using SlipeServer.Server.Resources;

namespace RealmCore.Resources.Base;

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