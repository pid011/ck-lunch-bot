using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CKLunchBot.Core.ImageProcess;

namespace CKLunchBot.Core.Menu
{
    public enum MenuType
    {
        Breakfast,
        Lunch,
        Dinner
    }

    public record TodayMenu
    {
        public DateTime Date { get; }
        public IReadOnlyList<string>? Breakfast { get; init; }
        public IReadOnlyList<string>? Lunch { get; init; }
        public IReadOnlyList<string>? Dinner { get; init; }

        public TodayMenu(DateTime date)
        {
            Date = date;
        }

        /// <summary>
        /// Make image
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="MenuImageGenerateException"></exception>
        public async Task<byte[]> MakeImageAsync(MenuType type)
        {
            return await Task.Run(
                () => MenuImageGenerator.GenerateMenuImage(this, type)).ConfigureAwait(false);
        }

        public override string ToString()
        {
            var defaultList = new string[] { "No menu provides" };

            return new StringBuilder()
                .AppendLine($"<{Date.Year}-{Date.Month}-{Date.Day} {Date.DayOfWeek}>")
                .AppendLine($"{"[Breakfast]",-12}")
                .AppendJoin(',', Breakfast ?? defaultList)
                .AppendLine()
                .AppendLine($"{"[Lunch]",-12}")
                .AppendJoin(',', Lunch ?? defaultList)
                .AppendLine()
                .AppendLine($"{"[Dinner]",-12}")
                .AppendJoin(',', Dinner ?? defaultList)
                .ToString();
        }
    }
}
