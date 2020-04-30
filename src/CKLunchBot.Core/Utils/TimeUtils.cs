using System;

namespace CKLunchBot.Core.Utils
{
    public class TimeUtils
    {
        /// <summary>
        /// Get current Korea Standard Time as a formatted string.
        /// </summary>
        public static string FormattedKoreaNowTime
        {
            get
            {
                // Korea Standard Time (UTC +9:00)
                DateTime koreaTime = DateTime.UtcNow.AddHours(9);
                return koreaTime.ToString("D", new System.Globalization.CultureInfo("ko-KR"));
            }
        }
    }
}