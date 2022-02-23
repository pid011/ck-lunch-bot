// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;

namespace CKLunchBot.Core.Requester
{
    internal static class WebClient
    {
        // https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient#remarks
        public static readonly HttpClient Client = new();
    }
}
