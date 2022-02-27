using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CKLunchBot.Core.Parser;
using CKLunchBot.Core.Requester;

namespace CKLunchBot.Core;

public record class WeekMenu : IReadOnlyCollection<TodayMenu>
{
    private readonly IList<TodayMenu> _dayMenus;

    public int Count => _dayMenus.Count;

    private WeekMenu(IList<TodayMenu> dayMenus)
    {
        _dayMenus = dayMenus;
    }

    public static async Task<WeekMenu> LoadAsync(CancellationToken cancellation = default)
    {
        var menuHtml = await MenuRequester.RequestMenuHtmlAsync(cancellation);
        var menus = MenuParser.ParseAllDayMenu(menuHtml);

        return new WeekMenu(menus);
    }

    public TodayMenu? Find(DateOnly date)
    {
        return _dayMenus.FirstOrDefault(menu => menu.Date == date);
    }

    public IEnumerator<TodayMenu> GetEnumerator()
    {
        return _dayMenus.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendJoin("\n\n", _dayMenus)
            .ToString();
    }
}
