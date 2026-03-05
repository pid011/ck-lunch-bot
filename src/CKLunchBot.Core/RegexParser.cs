using System.Text.RegularExpressions;

namespace CKLunchBot;

public partial class RegexParser
{
    [GeneratedRegex(@"[0-9]{1,2}")]
    public static partial Regex DateTextRegex();

    [GeneratedRegex(@"\s{2,}")]
    public static partial Regex MenuTextRegex();

    [GeneratedRegex("{{(.*?)}}")]
    public static partial Regex ReplacementTextRegex();
}
