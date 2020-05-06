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
            string nullStr = "null";
            string[] nullStrArr = new string[1] { nullStr };
            var menuText = new StringBuilder();
            menuText.AppendLine($"[{RestaurantName ?? nullStr}]");
            menuText.AppendLine();

            menuText.Append($"{"Monday", -12}: ");
            menuText.AppendJoin(", ", MondayMenu ?? nullStrArr);
            menuText.AppendLine();

            menuText.Append($"{"Tuesday", -12}: ");
            menuText.AppendJoin(", ", TuesdayMenu ?? nullStrArr);
            menuText.AppendLine();

            menuText.Append($"{"Wednesday", -12}: ");
            menuText.AppendJoin(", ", WednesdayMenu ?? nullStrArr);
            menuText.AppendLine();

            menuText.Append($"{"Thursday", -12}: ");
            menuText.AppendJoin(", ", ThursdayMenu ?? nullStrArr);
            menuText.AppendLine();

            menuText.Append($"{"Friday", -12}: ");
            menuText.AppendJoin(", ", FridayMenu ?? nullStrArr);
            menuText.AppendLine();

            menuText.Append($"{"Saturday", -12}: ");
            menuText.AppendJoin(", ", SaturdayMenu ?? nullStrArr);
            menuText.AppendLine();

            menuText.Append($"{"Sunday", -12}: ");
            menuText.AppendJoin(", ", SundayMenu ?? nullStrArr);
            menuText.AppendLine();

            return menuText.ToString();
        }
    }
}