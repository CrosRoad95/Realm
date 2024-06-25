using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SlipeServer.Resources.Base;
using SlipeServer.Server.ServerBuilders;

[assembly: InternalsVisibleTo("RealmCore.TestingTools")]
[assembly: InternalsVisibleTo("RealmCore.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace RealmCore.Resources.Assets;

public static class ServerBuilderExtensions
{
    public static void AddAssetsResource(this ServerBuilder builder)
    {
        builder.AddLogic<AssetsLogic>();
    }

    public static IServiceCollection AddAssetsServices(this IServiceCollection services)
    {
        services.AddHostedService<AddAssetsResourceHostedService>();
        services.AddSingleton<IAssetsService, AssetsService>();
        services.AddSingleton<AssetsCollection>();
        return services;
    }
}

internal sealed class AddAssetsResourceHostedService : IHostedLifecycleService
{
    private readonly MtaServer _mtaServer;
    private readonly IAssetEncryptionProvider _assetEncryptionProvider;
    private readonly AssetsCollection _assetsCollection;

    public AddAssetsResourceHostedService(MtaServer mtaServer, IAssetEncryptionProvider assetEncryptionProvider, AssetsCollection assetsCollection)
    {
        _mtaServer = mtaServer;
        _assetEncryptionProvider = assetEncryptionProvider;
        _assetsCollection = assetsCollection;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        var resource = new AssetsResource(_mtaServer);
        resource.AddFiles(_assetEncryptionProvider, _assetsCollection);

        var additionalFiles = resource.GetAndAddLuaFiles();
        foreach (var item in resource.AdditionalFiles)
        {
            additionalFiles.Add(item.Key, item.Value);
        }
        _mtaServer.AddAdditionalResource(resource, additionalFiles);

        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
