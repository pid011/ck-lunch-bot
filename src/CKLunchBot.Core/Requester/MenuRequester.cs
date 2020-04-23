using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CKLunchBot.Core.Requester
{
    public class MenuRequester : BaseRequester, IRequester
    {
        private static readonly string ApiKey = "852f99ba1fd20a2b5f662467635f58d55dd18ae9ee16e635123dd187d8179738";

        private static readonly string ApiRequestUrl = @"https://api.ck.ac.kr/api/v2/router_main/";

        private static readonly string MenuRequestContent =
            JObject.FromObject(new
            {
                resource = new
                {
                    service = "CKINFO_MEALMENU",
                    CRUDF = "R"
                }
            }).ToString();

        public async Task<JObject> RequestData()
        {
            var headers = new Dictionary<string, string>()
            {
                ["X-Dreamfactory-API-Key"] = ApiKey
            };

            try
            {
                return await GetJsonFromUrl(ApiRequestUrl, MenuRequestContent, headers);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}