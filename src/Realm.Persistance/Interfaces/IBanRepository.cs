﻿namespace Realm.Persistance.Interfaces;

public interface IBanRepository : IDisposable
{
    Ban CreateBanForSerial(string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0);
    Ban CreateBanForUser(int userId, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0);
    Task<List<Ban>> GetBansByUserId(int userId);
    Task<List<Ban>> GetBansBySerial(string serial);
    void RemoveBan(Ban ban);
    Task Commit();
}