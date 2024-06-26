﻿namespace RealmCore.TestingTools;

public class TestDateTimeProvider : IDateTimeProvider
{
    private DateTime _now;
    public TestDateTimeProvider(DateTime? now = null)
    {
        _now = now ?? DateTime.Now;
    }

    public void Add(TimeSpan timeSpan)
    {
        _now += timeSpan;
    }

    public DateTime Now => _now;
}
