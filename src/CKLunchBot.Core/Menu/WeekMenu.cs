using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CKLunchBot.Core.Parser;
using CKLunchBot.Core.Requester;

namespace CKLunchBot.Core.Menu
{
    public record WeekMenu : IReadOnlyList<TodayMenu>
    {
        private readonly IList<TodayMenu> _menus;

        public TodayMenu this[int index] => _menus[index];

        public int Count => _menus.Count;

        private WeekMenu(IList<TodayMenu> menus)
        {
            _menus = menus;
        }

        /// <summary>
        /// Load menu from web
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        /// <exception cref="MenuParseException"></exception>
        public async static Task<WeekMenu> LoadAsync(CancellationToken cancelToken = default)
        {
            var html = await MenuRequester.RequestMenuHtmlAsync(cancelToken);
            return new WeekMenu(MenuParser.ParseMenu(html));
        }

        /// <summary>
        /// Finds and returns a menu that matches the date.
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public TodayMenu FindMenu(int day)
        {
            var todayMenu = _menus.FirstOrDefault(menu => menu.Date.Day == day);
            if (todayMenu is null)
            {
                throw new NoProvidedMenuException($"{day}일에 제공하는 메뉴를 찾을 수 없습니다.");
            }
            return todayMenu;
        }

        public IEnumerator<TodayMenu> GetEnumerator()
        {
            return _menus.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _menus.GetEnumerator();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var menu in _menus)
            {
                sb.AppendLine(menu.ToString());
            }
            return sb.Remove(sb.Length - 1, 1).ToString();
        }
    }
}
