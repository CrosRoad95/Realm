﻿using Microsoft.Extensions.DependencyInjection;
using RealmCore.Resources.Base;
using SlipeServer.Server.ServerBuilders;

namespace RealmCore.Resources.CEFBlazorGui;

public static class ServerBuilderExtensions
{
    public static void AddCEFBlazorGuiResource(this ServerBuilder builder, string filesLocation, CEFGuiBlazorMode defaultCEFGuiBlazorMode)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new CEFBlazorGuiResource(server, filesLocation);
            resource.AddLuaEventHub<ICEFBlazorGuiEventHub>();

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<HttpDebugServer>();
            services.AddSingleton<ICEFBlazorGuiService>(x => new CEFBlazorGuiService(defaultCEFGuiBlazorMode, x.GetRequiredService<HttpDebugServer>()));
        });

        builder.AddLuaEventHub<ICEFBlazorGuiEventHub, CEFBlazorGuiResource>();
        builder.AddLogic<CEFBlazorGuiLogic>();
    }
}