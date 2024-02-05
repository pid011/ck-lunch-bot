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
            DayOfWeek.Sunday => "일요일",
            DayOfWeek.Monday => "월요일",
            DayOfWeek.Tuesday => "화요일",
            DayOfWeek.Wednesday => "수요일",
            DayOfWeek.Thursday => "목요일",
            DayOfWeek.Friday => "금요일",
            DayOfWeek.Saturday => "토요일",
            _ => throw new NotImplementedException()
        };

        return date.ToString($"yyyy년 M월 d일 {dayOfWeek}");
    }
}
