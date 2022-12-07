namespace Realm.GRpc;

public class GrpcModule : IModule
{
    private readonly Server _grpcServer;

    public GrpcModule()
    {
        var greeterServiceStub = new GreeterServiceStub();
        _grpcServer = new Server
        {
            Services =
            {
                Greet.Greeter.BindService(greeterServiceStub)
            },
            Ports =
            {
                new ServerPort("0.0.0.0", 22010, ServerCredentials.Insecure)
            },
        };
    }

    public string Name => "Grpc";
    private IGrpcModuleInterface? _interface;

    public void Configure(IServiceCollection services)
    {
    }

    public void Init(IServiceProvider serviceProvider)
    {
        Start();
        _interface = serviceProvider.GetRequiredService<IGrpcModuleInterface>();
    }

    public void PostInit(IServiceProvider serviceProvider)
    {
        if (_interface == null)
            throw new InvalidOperationException();

        var logger = serviceProvider.GetRequiredService<ILogger>().ForContext<GrpcModule>();
    }

    public T GetInterface<T>() where T : class
    {
        if (_interface == null)
            throw new InvalidOperationException();

        if (typeof(T) == typeof(IGrpcModuleInterface))
        {
            var @interface = _interface as T;
            if (@interface == null)
                throw new InvalidOperationException();
            return @interface;
        }
        throw new ArgumentException();
    }

    private void Start()
    {
        _grpcServer.Start();
    }

    private void Shutdown()
    {
        _grpcServer.ShutdownAsync().Wait();
    }

    public void Reload()
    {
        Shutdown();
        Start();
    }

    public int GetPriority() => -100;
}
