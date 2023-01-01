using Realm.Persistance.Interfaces;

namespace Realm.Persistance;

public class RepositoryFactory
{
    private readonly IServiceProvider _serviceProvider;

    public RepositoryFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IVehicleRepository GetVehicleRepository() => _serviceProvider.GetRequiredService<IVehicleRepository>();
}
