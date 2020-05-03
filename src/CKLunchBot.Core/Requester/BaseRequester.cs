using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CKLunchBot.Core.Requester
{
    public abstract class BaseRequester : IDisposable
    {
        private readonly HttpClient client = new HttpClient();

        public abstract Task<JObject> RequestData();

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

            using HttpResponseMessage response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        protected async Task<JObject> GetJsonFromUrl(
            string url, string jsonContent = null, Dictionary<string, string> headers = null)
        {
            var jsonString = await GetResponseString(url, jsonContent, headers);
            return JObject.Parse(jsonString);
        }

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                client.Dispose();
                disposedValue = true;
            }
        }

        ~BaseRequester()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}