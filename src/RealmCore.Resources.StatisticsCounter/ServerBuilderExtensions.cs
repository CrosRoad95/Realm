using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Resources.Base;
using SlipeServer.Server.ServerBuilders;

namespace RealmCore.Resources.StatisticsCounter;

public static class ServerBuilderExtensions
{
    public static void AddStatisticsCounterResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new StatisticsCounterResource(server);
            var additionalFiles = resource.GetAndAddLuaFiles();
            server.AddAdditionalResource(resource, additionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddStatisticsCounterServices();
        });

        builder.AddLogic<StatisticCounterLogic>();
    }

    public static IServiceCollection AddStatisticsCounterServices(this IServiceCollection services)
    {
        services.AddSingleton<IStatisticsCounterService, StatisticsCounterService>();
        return services;
    }
}
