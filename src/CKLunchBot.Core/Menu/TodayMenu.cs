using System;
using System.Text;
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
        public MenuItem? Breakfast { get; init; }
        public MenuItem? Lunch { get; init; }
        public MenuItem? Dinner { get; init; }

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
        public byte[] MakeImage(MenuType type)
        {
            return MenuImageGenerator.GenerateMenuImage(this, type);
        }

        public override string ToString()
        {
            var defaultList = new string[] { "No menu provides" };

            return new StringBuilder()
                .AppendLine($"<{Date.Year}-{Date.Month}-{Date.Day} {Date.DayOfWeek}>")
                .AppendLine($"{"[Breakfast]",-12}")
                .AppendJoin(',', Breakfast?.Menus ?? defaultList)
                .AppendLine()
                .AppendLine($"{"[Lunch]",-12}")
                .AppendJoin(',', Lunch?.Menus ?? defaultList)
                .AppendLine()
                .AppendLine($"{"[Dinner]",-12}")
                .AppendJoin(',', Dinner?.Menus ?? defaultList)
                .ToString();
        }
    }
}
