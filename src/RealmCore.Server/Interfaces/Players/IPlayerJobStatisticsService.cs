﻿namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerJobStatisticsService : IPlayerService, IEnumerable<UserJobStatisticsDTO>
{
    void AddPoints(short jobId, ulong points);
    void AddPointsAndTimePlayed(short jobId, ulong points, ulong timePlayed);
    void AddTimePlayed(short jobId, ulong timePlayed);
    (ulong, ulong) GetTotalPoints(short jobId);
}