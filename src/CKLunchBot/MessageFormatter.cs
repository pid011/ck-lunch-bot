using System.Text.RegularExpressions;

namespace CKLunchBot;

internal interface IMessageFormatter
{
    string Format(string message, Func<string, string> replace);
}

internal class MessageFormatter : IMessageFormatter
{
    public string Format(string message, Func<string, string> replace)
    {
        return RegexParser.ReplacementTextRegex().Replace(message, match => replace(match.Groups[1].Value));
    }
}

internal partial class RegexParser
{
    [GeneratedRegex("{{(.*?)}}")]
    public static partial Regex ReplacementTextRegex();
}
