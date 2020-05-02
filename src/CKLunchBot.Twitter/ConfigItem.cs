using Newtonsoft.Json;

namespace CKLunchBot.Twitter
{
    public class ConfigItem
    {
        public const string TweetTimePropertyName = "tweet-time";

        [JsonProperty(TweetTimePropertyName)]
        public Time TweetTime { get; set; }

        [JsonObject(TweetTimePropertyName)]
        public class Time
        {
            public const string HourPropertyName = "hour24";
            public const string MinutePropertyName = "minute";

            [JsonProperty(HourPropertyName)]
            public int Hour { get; set; }

            [JsonProperty(MinutePropertyName)]
            public int Minute { get; set; }
        }
    }
}