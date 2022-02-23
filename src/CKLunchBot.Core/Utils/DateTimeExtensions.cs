using System;

namespace CKLunchBot.Core.Utils
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Get formatted time string as a korean.
        /// </summary>
        public static string GetFormattedKoreanTimeString(this DateTime time)
        {
            return time.ToString("D", new System.Globalization.CultureInfo("ko-KR"));
        }
    }
}
