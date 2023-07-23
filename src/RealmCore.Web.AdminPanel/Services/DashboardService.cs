using Grpc.Net.Client;
using WebAdminPanel;

namespace RealmCore.Web.AdminPanel.Services;

public class DashboardService
{
    private readonly Dashboard.DashboardClient _client;
    public DashboardService(GrpcChannel grpcChannel)
    {
        _client = new(grpcChannel);
    }

    public async Task<IEnumerable<DashboardElement>> GetAllDashboardElements()
    {
        return (await _client.GetDashboardElementsAsync(new GetDashboardElementsRequest
        {
            Id = 0
        })).Elements;
    }
    public async Task<DashboardElement> GetDashboardElement(int id)
    {
        return (await _client.GetDashboardElementsAsync(new GetDashboardElementsRequest
        {
            Id = id
        })).Elements.First();
    }
}
