using Newtonsoft.Json;

namespace CKLunchBot.Twitter
{
    public class TwitterTokens
    {
        public const string ConsumerApiKeyPropertyName = "consumer-api-key";
        public const string ConsumerSecretKeyPropertyName = "consumer-secret-key";
        public const string AccessTokenPropertyName = "access-token";
        public const string AccessTokenSecretPropertyName = "access-token-secret";

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