using Newtonsoft.Json;

namespace CKLunchBot.Twitter
{
    public class ConfigItem
    {
        public const string LunchTweetTimePropertyName = "lunch_tweet_time";
        public const string BreakfastTweetTimePropertyName = "breackfast_tweet_time";
        public const string DinnerTweetTimePropertyName = "dinner_tweet_time";
        public const string TwitterTokensPropertyName = "twitter_tokens";

        [JsonProperty(LunchTweetTimePropertyName)]
        public Time LunchTweetTime { get; set; }

        [JsonProperty(BreakfastTweetTimePropertyName)]
        public Time BreakfastTweetTime { get; set; }

        [JsonProperty(DinnerTweetTimePropertyName)]
        public Time DinnerTweetTime { get; set; }

        [JsonProperty(TwitterTokensPropertyName)]
        public TwitterToken TwitterTokens { get; set; }

        public class Time
        {
            public const string HourPropertyName = "hour24";
            public const string MinutePropertyName = "minute";

            [JsonProperty(HourPropertyName)]
            public int Hour { get; set; }

            [JsonProperty(MinutePropertyName)]
            public int Minute { get; set; }
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