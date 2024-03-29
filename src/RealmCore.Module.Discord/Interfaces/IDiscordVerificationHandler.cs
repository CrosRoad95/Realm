﻿namespace RealmCore.Module.Discord.Interfaces;

public interface IDiscordVerificationHandler
{
    Task<bool> HandleVerifyCode(string code, ulong discordUserId);
}
