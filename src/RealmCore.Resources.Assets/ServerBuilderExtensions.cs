﻿using Microsoft.Extensions.DependencyInjection;
using RealmCore.Resources.Assets.Interfaces;
using SlipeServer.Server.ServerBuilders;

namespace RealmCore.Resources.Assets;

public static class ServerBuilderExtensions
{
    public static void AddAssetsResource(this ServerBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IAssetsService, AssetsService>();
            services.AddSingleton<AssetsRegistry>();
            services.AddSingleton<IServerAssetsProvider>(x => x.GetRequiredService<AssetsRegistry>());
        });

        builder.AddBuildStep(x =>
        {
            x.InstantiatePersistent<AssetsLogic>();
        }, ServerBuildStepPriority.Low);
    }
}