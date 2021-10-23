// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using HtmlAgilityPack;

namespace CKLunchBot.Core.Requester
{
    internal class BaseRequester
    {
        protected static readonly HtmlWeb Web = new();
        protected static readonly HttpClient Client = new();
    }
}
