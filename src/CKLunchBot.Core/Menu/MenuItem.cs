using System.Text;
using System.Text.RegularExpressions;

namespace CKLunchBot.Core.Menu
{
    public class MenuItem
    {
        public MenuItem(RawMenuItem raw)
        {
            RestaurantName = StringToRestaurants(raw.RestaurantName);
            SundayMenu     = SplitMenu(raw.SundayMenuRawText);
            MondayMenu     = SplitMenu(raw.MondayMenuRawText);
            TuesdayMenu    = SplitMenu(raw.TuesdayMenuRawText);
            WednesdayMenu  = SplitMenu(raw.WednesdayMenuRawText);
            ThursdayMenu   = SplitMenu(raw.ThursdayMenuRawText);
            FridayMenu     = SplitMenu(raw.FridayMenuRawText);
            SaturdayMenu   = SplitMenu(raw.SaturdayMenuRawText);

            RawMenu = raw;
        }

        public RawMenuItem RawMenu { get; }

        public Restaurants RestaurantName { get; }

        public string[] SundayMenu { get; }

        public string[] MondayMenu { get; }

        public string[] TuesdayMenu { get; }

        public string[] WednesdayMenu { get; }

        public string[] ThursdayMenu { get; }

        public string[] FridayMenu { get; }

        public string[] SaturdayMenu { get; }

        /// <summary>
        /// Convert raw menu text to string array. If raw menu text is null, return empty string array.
        /// </summary>
        /// <param name="rawMenuText">raw menu text. example: "rise\r\nkimchi"</param>
        /// <returns>splited menus</returns>
        private string[] SplitMenu(string rawMenuText)
        {
            return Regex.Split(rawMenuText ?? string.Empty, "\r\n");
        }

        private Restaurants StringToRestaurants(string name)
        {
            const string daban            = "다반";
            const string nankatsuNanUdong = "난카츠난우동";
            const string tangAndJjigae    = "탕&찌개차림";
            const string yukHaeBab        = "육해밥";
            const string dormBreakfast    = "아침";
            const string dormLunch        = "점심";
            const string dormDinner       = "저녁";

            Restaurants result = name switch
            {
                daban            => Restaurants.Daban,
                nankatsuNanUdong => Restaurants.NankatsuNanUdong,
                tangAndJjigae    => Restaurants.TangAndJjigae,
                yukHaeBab        => Restaurants.YukHaeBab,
                dormBreakfast    => Restaurants.DormBreakfast,
                dormLunch        => Restaurants.DormLunch,
                dormDinner       => Restaurants.DormDinner,
                _                => Restaurants.Unknown
            };

            return result;
        }

        public override string ToString()
        {
            string nullStr = "null";
            string[] nullStrArr = new string[1] { nullStr };

            var menuText = new StringBuilder();
            menuText.AppendLine($"[{RestaurantName}]");
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