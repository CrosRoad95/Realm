using System.Text.RegularExpressions;

namespace RealmCore.Server.Modules.Core;

public static partial class Regexes
{
    [GeneratedRegex(@"@(?<Search>\w)(?:\((?<Argument>\w+)\))?")]
    public static partial Regex SpecialSearchSwitch();

    [GeneratedRegex("^[A-F0-9]{32}$")]
    public static partial Regex ValidSerial();
}
