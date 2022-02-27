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
        [FunctionName("today")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
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

            if (string.IsNullOrEmpty(reqType))
            {
                return new BadRequestObjectResult("Need request type.");
            }

            if (!Enum.TryParse<MenuType>(reqType.ToLower(), true, out var menuType))
            {
                return new BadRequestObjectResult("Wrong request type.");
            }

            return await ProcessAsync(log, menuType);
        }

        private static async Task<IActionResult> ProcessAsync(ILogger log, MenuType menuType)
        {
            await Task.Delay(1000);
            log.LogInformation($"type={menuType}");
            return new OkObjectResult($"type={menuType}");
        }
    }
}
