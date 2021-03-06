// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CKLunchBot.Core.Menu;

using Newtonsoft.Json.Linq;

namespace CKLunchBot.Core.Requester
{
    public class MenuRequester : BaseRequester
    {
        private const string ApiRequestUrl = @"https://api.ck.ac.kr/api/v2/router_main/";

        private readonly string _menuRequestContent =
            JObject.FromObject(new
            {
                resource = new
                {
                    service = "CKINFO_MEALMENU",
                    CRUDF = "R"
                }
            }).ToString();

        private readonly Dictionary<string, string> _headers;

        public MenuRequester(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("API key is wrong.", nameof(apiKey));
            }

            _headers = new Dictionary<string, string>() { ["X-Dreamfactory-API-Key"] = apiKey };
        }

        public async Task<RestaurantsWeekMenu> RequestWeekMenuAsync()
        {
            JObject jobj = await GetJsonFromUrl(ApiRequestUrl, _menuRequestContent, _headers);
            return ParseJson(jobj);
        }

        public async Task<string> RequestString()
        {
            return await GetStringFromUrl(ApiRequestUrl, _menuRequestContent, _headers);
        }

        private static RestaurantsWeekMenu ParseJson(JObject jsonObject)
        {
            var success = (bool)jsonObject["success"];
            if (!success)
            {
                var message = (string)jsonObject["result"];
                throw new Exception($"API request fail message: {message}");
            }

            List<MenuItem> menuList = jsonObject["result"]
                .Select(a => new MenuItem(a.ToObject<RawMenuItem>()))
                .ToList();

            return new RestaurantsWeekMenu(menuList);
        }
    }
}

/*
API response data examples

[Wrong service name]
{
  "success": false,
  "result": "Wrong Service Name.",
  "date": "20200420213128"
}

----------------------------------------------------------

[Response success but nothing recieved]
{
  "success": true,
  "result": [],
  "date": "20200504003540"
}

----------------------------------------------------------

[Response success]
{
  "success": true,
  "result": [
    {
      "RAST_NAME": "저녁",
      "SUN": null,
      "MON": null,
      "TUE": null,
      "WED": null,
      "THU": null,
      "FRI": null,
      "SAT": null
    },
    {
      "RAST_NAME": "아침",
      "SUN": null,
      "MON": null,
      "TUE": null,
      "WED": null,
      "THU": null,
      "FRI": null,
      "SAT": null
    },
    {
      "RAST_NAME": "점심",
      "SUN": null,
      "MON": null,
      "TUE": null,
      "WED": null,
      "THU": null,
      "FRI": null,
      "SAT": null
    },
    {
      "RAST_NAME": "다반",
      "SUN": null,
      "MON": "쌀밥\r\n배추된장국\r\n콩나물불고기\r\n김청포묵무침\r\n열무나물\r\n깍두기\r\n양상추샐러드\r\n숭늉",
      "TUE": "쌀밥\r\n초당순두부국\r\n청양풍닭살볶음\r\n맛살브로콜리볶음\r\n마늘종지무침\r\n배추김치\r\n양상추샐러드\r\n숭늉",
      "WED": "나물비빔밥\r\n&계란후라이\r\n열무된장국\r\n생선커틀릿\r\n배추김치\r\n고구마파이\r\n숭늉",
      "THU": "쌀밥\r\n소고기뭇국\r\n매콤두부조림\r\n햄감자채볶음\r\n오이지무침\r\n깍두기\r\n양상추샐러드\r\n숭늉",
      "FRI": "마파두부덮밥\r\n김치콩나물국\r\n미트볼데리조림\r\n깍두기\r\n양상추샐러드\r\n숭늉",
      "SAT": null
    },
    {
      "RAST_NAME": "난카츠난우동",
      "SUN": null,
      "MON": null,
      "TUE": null,
      "WED": null,
      "THU": null,
      "FRI": null,
      "SAT": null
    },
    {
      "RAST_NAME": "탕&찌개차림",
      "SUN": null,
      "MON": null,
      "TUE": null,
      "WED": null,
      "THU": null,
      "FRI": null,
      "SAT": null
    },
    {
      "RAST_NAME": "육해밥",
      "SUN": null,
      "MON": null,
      "TUE": null,
      "WED": null,
      "THU": null,
      "FRI": null,
      "SAT": null
    }
  ],
  "date": "20200420213345"
}
*/
