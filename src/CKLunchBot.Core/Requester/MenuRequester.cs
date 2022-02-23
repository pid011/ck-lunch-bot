// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CKLunchBot.Core.Requester
{
    internal class MenuRequester
    {
        private const string MenuUrl = @"https://www.ck.ac.kr/univ-life/menu";

        public async static Task<HtmlDocument> RequestMenuHtmlAsync(CancellationToken cancelToken = default)
        {
            using var htmlStream = await WebClient.Client.GetStreamAsync(MenuUrl, cancelToken);

            var html = new HtmlDocument();
            html.Load(htmlStream);

            return html;
        }
    }
}
