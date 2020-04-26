using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CKLunchBot.Core.Requester
{
    public class BaseRequester
    {
        private static readonly HttpClient client = new HttpClient();

        protected async Task<string> GetResponseString(
            string url, string jsonContent = null, Dictionary<string, string> headers = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("요청받을 url이 null을 참조하고 있거나 비어있습니다", nameof(url));
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (jsonContent != null)
            {
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                // TODO: Exception 전달 시 Response받은 문자열도 같이 넣어서 반환
                throw;
            }
        }

        protected async Task<JObject> GetJsonFromUrl(
            string url, string jsonContent = null, Dictionary<string, string> headers = null)
        {
            try
            {
                return JObject.Parse(await GetResponseString(url, jsonContent, headers));
            }
            catch (Exception)
            {
                //Exception 전달 시 json 파싱 실패한 문자열도 같이 넣어서 반환
                throw;
            }
        }
    }
}