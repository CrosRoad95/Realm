using Microsoft.Extensions.Logging;

namespace RealmCore.Module.Web.AdminPanel;

internal class WebAdminPanelIntegration : IModule
{
    private readonly ILogger<WebAdminPanelIntegration> _logger;

    public WebAdminPanelIntegration(ILogger<WebAdminPanelIntegration> logger)
    {
        _logger = logger;

        _logger.LogInformation("{module} module loaded", "WebAdminPanel");
    }
}
