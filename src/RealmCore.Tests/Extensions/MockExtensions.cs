namespace RealmCore.Tests.Extensions;

internal static class MockExtensions
{
    public static void SetupLogger<T>(this Mock<ILogger<T>> loggerMock)
    {
        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Verifiable();
        loggerMock.Setup(x => x.BeginScope(It.IsAny<Dictionary<string, object>>())).Returns((IDisposable)null);

    }
}
