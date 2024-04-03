namespace RealmCore.Resources.Base;

public sealed class CommonResourceOptions
{
    public CommonResourceOptions()
    {
    }

    public void Configure(Resource resource)
    {
        resource.NoClientScripts["utilities/utilities.lua"] = ResourceFiles.Utilities;
    }
}