using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using RealmCore.Module.Discord;
using RealmCore.Module.Grpc;

namespace RealmCore.Discord.Integration.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds integration from discord bot side
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddRealmDiscordBotIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GrpcOptions>(configuration.GetSection("Grpc"));
        services.Configure<DiscordBotOptions>(configuration.GetSection("Discord"));
        services.AddSingleton(new IntegrationHeader(1));
        services.AddSingleton<IRealmDiscordClient, RealmDiscordClient>();
        services.AddSingleton<CommandHandler>();
        services.AddSingleton<BotIdProvider>();

        services.AddSingleton<GrpcServer>();
        services.AddSingleton<MessagingServiceStub>();
        services.AddSingleton<TextBasedCommands>();
        services.AddHostedService<RealmDiscordHostedService>();

        services.AddSingleton(x =>
        {
            var options = x.GetRequiredService<IOptions<GrpcOptions>>();
            return GrpcChannel.ForAddress($"http://{options.Value.RemoteHost}:{options.Value.RemotePort}");
        });

        return services;
    }
    
    /// <summary>
    /// Adds integration from server side
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddRealmServerDiscordBotIntegration(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;
        services.Configure<GrpcOptions>(configuration.GetSection("Grpc"));
        services.AddGrpcModule();
        services.AddDiscordModule();

        return builder;
    }

    public static IServiceCollection AddDiscordChannel<T>(this IServiceCollection services)
        where T : class, IChannelBase
    {
        services.AddSingleton<IChannelBase, T>();
        return services;
    }
}
