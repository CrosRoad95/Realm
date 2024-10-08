﻿namespace RealmCore.Resources.Assets;

public static class ResourceExtensions
{
    public static Resource InjectAssetsExportedFunctions(this Resource resource)
    {
        resource.NoClientScripts[$"{resource.Name}/assetsExports.lua"] =
            Encoding.UTF8.GetBytes("function requestAsset(...) return exports.assets:requestAsset(...) end; function requestRemoteImageAsset(...) return exports.assets:requestRemoteImageAsset(...) end;");
        return resource;
    }
}
