using Serilog.Core;
using Serilog.Events;

namespace Realm.Persistance.Logger.Enrichers;

internal class PlayerAccountEnricher : ILogEventEnricher
{
    private readonly PlayerAccount _playerAccount;

    public PlayerAccountEnricher(PlayerAccount playerAccount)
    {
        _playerAccount = playerAccount;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("accountId", _playerAccount.Id));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("userFriendlyName", _playerAccount.LongUserFriendlyName()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("padding", ": "));
    }
}
