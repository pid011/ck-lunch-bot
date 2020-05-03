using Newtonsoft.Json;

namespace CKLunchBot.Twitter
{
    public class ConfigItem
    {
        public const string TweetTimePropertyName = "tweet_time";
        public const string TwitterTokensName = "twitter_tokens";

        [JsonProperty(TweetTimePropertyName)]
        public Time TweetTime { get; set; }

        [JsonProperty(TwitterTokensName)]
        public TwitterToken TwitterTokens { get; set; }

        //[JsonObject(TweetTimePropertyName)]
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