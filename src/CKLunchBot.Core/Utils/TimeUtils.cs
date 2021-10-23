// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace CKLunchBot.Core.Utils
{
    public static class TimeUtils
    {
        public static DateTime KSTNow => DateTime.UtcNow.AddHours(9);

        public static DateTime GetKoreaTime(in DateTime utcTime)
        {
            return utcTime.AddHours(9);
        }

        /// <summary>
        /// Get Time as a formatted string.
        /// </summary>
        public static string GetFormattedKoreaTime(in DateTime time)
        {
            return time.ToString("D", new System.Globalization.CultureInfo("ko-KR"));
        }
    }
}
