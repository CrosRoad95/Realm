﻿namespace RealmCore.Persistance.Data;

public sealed class BanData
{
    public int Id { get; set; }
    public DateTime End { get; set; }
    public string? Serial { get; set; }
    public int? UserId { get; set; }
    public string? Reason { get; set; }
    public string? Responsible { get; set; }
    public int Type { get; set; }

    public UserData? User { get; set; }
}