using System;

namespace CKLunchBot.Core;

public static class DateOnlyExtensions
{
    /// <summary>
    /// Get formatted time string as a korean.
    /// </summary>
    public static string GetFormattedKoreanString(this DateOnly date)
    {
        var dayOfWeek = date.DayOfWeek switch
        {
            DayOfWeek.Sunday => "일",
            DayOfWeek.Monday => "월",
            DayOfWeek.Tuesday => "화",
            DayOfWeek.Wednesday => "수",
            DayOfWeek.Thursday => "목",
            DayOfWeek.Friday => "금",
            DayOfWeek.Saturday => "토",
            _ => throw new NotImplementedException()
        };

        return date.ToString($"yyyy.M.d ({dayOfWeek})");
    }
}
