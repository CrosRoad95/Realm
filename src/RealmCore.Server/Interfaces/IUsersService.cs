﻿namespace RealmCore.Server.Interfaces;

public interface IUsersService
{
    Task<bool> AuthorizePolicy(UserComponent userComponent, string policy);
    Task<bool> CheckPasswordAsync(UserData user, string password);
    Task<UserData?> GetUserById(int id);
    Task<UserData?> GetUserByLogin(string login);
    Task<UserData?> GetUserByLoginCaseInsensitive(string login);
    Task<bool> IsSerialWhitelisted(int userId, string serial);
    Task<bool> IsUserNameInUse(string userName);
    Task<bool> SignIn(Entity entity, UserData user);
    Task<int> SignUp(string username, string password);
    Task<bool> TryAddWhitelistedSerial(int userId, string serial);
    Task<bool> TryRemoveWhitelistedSerial(int userId, string serial);
}