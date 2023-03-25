using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.ServerBuilders;

namespace Realm.Resources.StatisticsCounter;

public static class ServerBuilderExtensions
{
    public static void AddStatisticsCounterResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new StatisticsCounterResource(server);

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IStatisticsCounterService, StatisticsCounterService>();
        });

        builder.AddLogic<StatisticCounterLogic>();
    }
}
