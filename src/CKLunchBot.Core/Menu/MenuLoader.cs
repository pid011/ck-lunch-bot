using CKLunchBot.Core.Requester;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CKLunchBot.Core.Menu
{
    public class MenuLoader : IDisposable
    {
        private readonly MenuRequester requester = new MenuRequester();

        public async Task<List<MenuItem>> GetWeekMenuFromAPIAsync()
        {
            var jsonObj = await requester.RequestData();
            return MenuJsonParser(jsonObj);
        }

        public static List<MenuItem> GetWeekMenuFromJsonString(string json)
        {
            return MenuJsonParser(JObject.Parse(json));
        }

        private static List<MenuItem> MenuJsonParser(JObject jsonObject)
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

            return menuList;
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
                requester.Dispose();
                disposedValue = true;
            }
        }

        ~MenuLoader()
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