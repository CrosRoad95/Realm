﻿namespace RealmCore.Server.Extensions.Resources;

public static class AssetsServiceExtensions
{
    public static void ReplaceModelFor(this IAssetsService assetsService, Entity entity, Stream dff, Stream col, ushort model)
    {
        assetsService.ReplaceModelFor(entity.Player, dff, col, model);
    }

    public static void RestoreModelFor(this IAssetsService assetsService, Entity entity, ushort model)
    {
        assetsService.RestoreModelFor(entity.Player, model);
    }
}