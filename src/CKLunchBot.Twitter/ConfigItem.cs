using Newtonsoft.Json;

using System;

namespace CKLunchBot.Twitter
{
    public class ConfigItem
    {
        public const string LunchTweetTimePropertyName = "lunch_tweet_time";
        public const string BreakfastTweetTimePropertyName = "breakfast_tweet_time";
        public const string DinnerTweetTimePropertyName = "dinner_tweet_time";
        public const string TwitterTokensPropertyName = "twitter_tokens";

        [JsonProperty(BreakfastTweetTimePropertyName)]
        public Time BreakfastTweetTime { get; set; }
        
        [JsonProperty(LunchTweetTimePropertyName)]
        public Time LunchTweetTime { get; set; }

        [JsonProperty(DinnerTweetTimePropertyName)]
        public Time DinnerTweetTime { get; set; }

        [JsonProperty(TwitterTokensPropertyName)]
        public TwitterToken TwitterTokens { get; set; }

        public class Time : IEquatable<Time>
        {
            public const string HourPropertyName = "hour24";
            public const string MinutePropertyName = "minute";

            [JsonProperty(HourPropertyName)]
            public int Hour { get; set; }

            [JsonProperty(MinutePropertyName)]
            public int Minute { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is Time)
                {
                    return Equals((Time)obj);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public bool Equals(Time other)
            {
                return (Hour == other.Minute && Minute == other.Minute);
            }

            public static bool operator ==(Time a, Time b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(Time a, Time b)
            {
                return !a.Equals(b);
            }

            public static bool operator >(Time a, Time b)
            {
                var t1 = new TimeSpan(a.Hour, a.Minute, 0);
                var t2 = new TimeSpan(b.Hour, b.Minute, 0);

                return t1 > t2;
            }

            public static bool operator <(Time a, Time b)
            {
                return !(a > b);
            }
        }

        public class TwitterToken
        {
            public const string ConsumerApiKeyPropertyName = "consumer_api_key";
            public const string ConsumerSecretKeyPropertyName = "consumer_secret_key";
            public const string AccessTokenPropertyName = "access_token";
            public const string AccessTokenSecretPropertyName = "access_token_secret";

            [JsonProperty(ConsumerApiKeyPropertyName)]
            public string ConsumerApiKey { get; set; }

            [JsonProperty(ConsumerSecretKeyPropertyName)]
            public string ConsumerSecretKey { get; set; }

            [JsonProperty(AccessTokenPropertyName)]
            public string AccessToken { get; set; }

            [JsonProperty(AccessTokenSecretPropertyName)]
            public string AccessTokenSecret { get; set; }
        }
    }
}