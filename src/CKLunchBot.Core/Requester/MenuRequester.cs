// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CKLunchBot.Core.Requester
{
    internal class MenuRequester : BaseRequester
    {
        private const string MenuUrl = @"https://www.ck.ac.kr/univ-life/menu";

        public static async Task<HtmlDocument> RequestMenuHtmlAsync(CancellationToken cancelToken = default)
        {
            return await Web.LoadFromWebAsync(MenuUrl, cancelToken);
        }
    }
}
