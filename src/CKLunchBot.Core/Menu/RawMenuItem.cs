// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace CKLunchBot.Core.Menu
{
    public class RawMenuItem
    {
        [JsonProperty("RAST_NAME")]
        public string RestaurantName { get; set; }

        [JsonProperty("SUN")]
        public string SundayMenuRawText { get; set; }

        [JsonProperty("MON")]
        public string MondayMenuRawText { get; set; }

        [JsonProperty("TUE")]
        public string TuesdayMenuRawText { get; set; }

        [JsonProperty("WED")]
        public string WednesdayMenuRawText { get; set; }

        [JsonProperty("THU")]
        public string ThursdayMenuRawText { get; set; }

        [JsonProperty("FRI")]
        public string FridayMenuRawText { get; set; }

        [JsonProperty("SAT")]
        public string SaturdayMenuRawText { get; set; }
    }
}
