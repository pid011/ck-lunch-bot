// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace CKLunchBot.Core.Utils
{
    public static class TimeUtils
    {
        /// <summary>
        /// Get Korea Standard Time.
        /// </summary>
        public static DateTime GetKoreaNowTime(DateTime utcNow) => utcNow.AddHours(9);

        /// <summary>
        /// Get Time as a formatted string.
        /// </summary>
        public static string GetFormattedKoreaTime(DateTime utcNow) =>
            GetKoreaNowTime(utcNow).ToString("D", new System.Globalization.CultureInfo("ko-KR"));
    }
}
