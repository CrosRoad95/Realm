namespace RealmCore.Console.Data.Validators;

internal class LoginDataValidator : AbstractValidator<LoginData>
{
    public LoginDataValidator()
    {
        RuleFor(x => x.Login).Length(3, 24);
    }
}
