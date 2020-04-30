using System;

namespace CKLunchBot.Core.Utils
{
    public class TimeUtils
    {
        public static TimeUtils Time => new TimeUtils();

        /// <summary>
        /// Get current Korea Standard Time.
        /// </summary>
        public DateTime KoreaNowTime => DateTime.UtcNow.AddHours(9);

        /// <summary>
        /// Get current Korea Standard Time as a formatted string.
        /// </summary>
        public string FormattedKoreaNowTime =>
            KoreaNowTime.ToString("D", new System.Globalization.CultureInfo("ko-KR"));

        private TimeUtils()
        {
        }
    }
}