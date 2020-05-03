using System.Text;
using System.Text.RegularExpressions;

namespace CKLunchBot.Core.Menu
{
    public class MenuItem
    {
        public MenuItem(RawMenuItem raw)
        {
            RestaurantName = raw.RestaurantName;
            SundayMenu = SplitMenu(raw.SundayMenuRawText);
            MondayMenu = SplitMenu(raw.MondayMenuRawText);
            TuesdayMenu = SplitMenu(raw.TuesdayMenuRawText);
            WednesdayMenu = SplitMenu(raw.WednesdayMenuRawText);
            ThursdayMenu = SplitMenu(raw.ThursdayMenuRawText);
            FridayMenu = SplitMenu(raw.FridayMenuRawText);
            SaturdayMenu = SplitMenu(raw.SaturdayMenuRawText);
        }

        public string RestaurantName { get; }

        public string[] SundayMenu { get; }

        public string[] MondayMenu { get; }

        public string[] TuesdayMenu { get; }

        public string[] WednesdayMenu { get; }

        public string[] ThursdayMenu { get; }

        public string[] FridayMenu { get; }

        public string[] SaturdayMenu { get; }

        private string[] SplitMenu(string rawMenuText) =>
            rawMenuText != null ? Regex.Split(rawMenuText, "\r\n") : null;

        public override string ToString()
        {
            var menuText = new StringBuilder();
            menuText.AppendLine($"[{RestaurantName}]");
            menuText.Append($"Monday: ");
            menuText.AppendJoin(", ", MondayMenu);
            menuText.Append($"Tuesday: ");
            menuText.AppendJoin(", ", TuesdayMenu);
            menuText.Append($"Wednesday: ");
            menuText.AppendJoin(", ", WednesdayMenu);
            menuText.Append($"Thursday: ");
            menuText.AppendJoin(", ", ThursdayMenu);
            menuText.Append($"Friday: ");
            menuText.AppendJoin(", ", FridayMenu);
            menuText.Append($"Saturday: ");
            menuText.AppendJoin(", ", SaturdayMenu);
            menuText.Append($"Sunday: ");
            menuText.AppendJoin(", ", SundayMenu);

            return menuText.ToString();
        }
    }
}