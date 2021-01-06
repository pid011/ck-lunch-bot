// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKLunchBot.Core.Requester
{
    public class BaseRequester : IDisposable
    {
        private readonly HttpClient _client = new HttpClient();

        /// <summary>
        /// Return <see cref="JObject"/> from the URL requests result.
        /// </summary>
        /// <param name="url">request url</param>
        /// <param name="jsonContent">request json content</param>
        /// <param name="headers">request headers</param>
        /// <returns></returns>
        protected async Task<JObject> GetJsonFromUrl(
            string url, string jsonContent = null, Dictionary<string, string> headers = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
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

            using HttpResponseMessage response = await _client.SendAsync(request);
            using Stream s = response.Content.ReadAsStreamAsync().Result;
            using StreamReader sr = new StreamReader(s);
            using JsonReader reader = new JsonTextReader(sr);

            return JObject.Load(reader);
        }

        protected async Task<string> GetStringFromUrl(string url, string jsonContent = null, Dictionary<string, string> headers = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (jsonContent != null)
            {
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            using HttpResponseMessage response = await _client.SendAsync(request);

            return await response.Content.ReadAsStringAsync();
        }

        #region IDisposable Support

        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }
                _client.Dispose();
                _disposedValue = true;
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
