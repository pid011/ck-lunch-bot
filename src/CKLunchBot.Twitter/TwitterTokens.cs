using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CKLunchBot.Twitter
{
    public class TwitterTokens
    {
        public const string ConsumerApiKeyName = "consumer-api-key";
        public const string ConsumerSecretKeyName = "consumer-secret-key";
        public const string AccessTokenName = "access-token";
        public const string AccessTokenSecretName = "access-token-secret";

        [JsonProperty(ConsumerApiKeyName)]
        public string ConsumerApiKey { get; set; }

        [JsonProperty(ConsumerSecretKeyName)]
        public string ConsumerSecretKey { get; set; }

        [JsonProperty(AccessTokenName)]
        public string AccessToken { get; set; }

        [JsonProperty(AccessTokenSecretName)]
        public string AccessTokenSecret { get; set; }
    }
}
