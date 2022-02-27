using System;

namespace CKLunchBot.Core;

public static class DateTimeExtensions
{
    public static DateOnly ToDateOnly(this DateTime dateTime)
    {
        return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
    }

    public static TimeOnly ToTimeOnly(this DateTime dateTime)
    {
        return new TimeOnly(dateTime.Ticks);
    }
}
