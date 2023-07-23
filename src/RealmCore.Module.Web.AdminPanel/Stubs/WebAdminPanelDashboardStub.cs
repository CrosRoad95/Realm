using Grpc.Core;
using Microsoft.Extensions.Logging;
using WebAdminPanel;

namespace RealmCore.Module.Web.AdminPanel.Stubs;

internal class WebAdminPanelDashboardStub : Dashboard.DashboardBase
{
    private readonly IWebAdminPanelService _webAdminPanelService;
    private readonly ILogger<WebAdminPanelDashboardStub> _logger;

    public WebAdminPanelDashboardStub(IWebAdminPanelService webAdminPanelService, ILogger<WebAdminPanelDashboardStub> logger)
    {
        _webAdminPanelService = webAdminPanelService;
        _logger = logger;
    }

    public override async Task<GetDashboardElementsReply> GetDashboardElements(GetDashboardElementsRequest request, ServerCallContext context)
    {
        var reply = new GetDashboardElementsReply();
        reply.Elements.AddRange(_webAdminPanelService.Dashboard.DashboardElements.Select(x => new DashboardElement
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Type = (int)x.Type,
            Data = x.DataFactory(),
        }));
        return reply;
    }
}
