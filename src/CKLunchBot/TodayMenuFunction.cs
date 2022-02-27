using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CKLunchBot.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace CKLunchBot
{
    public class TodayMenuFunction
    {
        // [FunctionName("today")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string reqType = req.Query["type"];
            string reqBodyString = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(reqBodyString))
            {
                using var document = JsonDocument.Parse(reqBodyString);

                var root = document.RootElement;
                if (root.TryGetProperty("type", out var typeElement))
                {
                    var typeStr = typeElement.GetString();
                    if (typeStr is not null)
                    {
                        reqType ??= typeStr;
                    }
                }
            }

            return await ProcessAsync(reqType);
        }

        private static async Task<IActionResult> ProcessAsync(string? reqType)
        {
            var weekMenu = await WeekMenu.LoadAsync();
            var todayMenu = weekMenu.Find(KST.Now.ToDateOnly());
            if (todayMenu is null)
            {
                return new BadRequestObjectResult("Cannot found today menu.");
            }

            if (!string.IsNullOrEmpty(reqType) && Enum.TryParse<MenuType>(reqType.ToLower(), true, out var menuType))
            {
                var menu = todayMenu[menuType];
                if (menu!.IsEmpty())
                {
                    return new BadRequestObjectResult($"{menuType} menu is empty.");
                }

                return new OkObjectResult(menu!.ToString());
            }

            return new OkObjectResult(todayMenu.ToString());
        }
    }
}
