using System;

namespace CKLunchBot.Core;

public static class DateOnlyExtensions
{
    /// <summary>
    /// Get formatted time string as a korean.
    /// </summary>
    public static string GetFormattedKoreanString(this DateOnly date)
    {
        return date.ToString("D", new System.Globalization.CultureInfo("ko-KR"));
    }
}
