﻿[assembly: InternalsVisibleTo("RealmCore.TestingTools")]
[assembly: InternalsVisibleTo("RealmCore.Tests")]

namespace RealmCore.Resources.ElementOutline;

public static class ServerBuilderExtensions
{
    public static void AddElementOutlineResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new ElementOutlineResource(server);

            resource.AddLuaEventHub<IElementOutlineEventHub>();
            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IElementOutlineService, ElementOutlineService>();
        });

        builder.AddLuaEventHub<IElementOutlineEventHub, ElementOutlineResource>();
        builder.AddLogic<ElementOutlineLogic>();
    }
}

