namespace Realm.Server.Identity;

internal class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool VerifyHashedPassword(string hashedPassword, string password) => BCrypt.Net.BCrypt.Verify(password, hashedPassword);
}
