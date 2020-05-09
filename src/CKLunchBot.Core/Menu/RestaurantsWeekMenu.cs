using System.Collections;
using System.Collections.Generic;

namespace CKLunchBot.Core.Menu
{
    public class RestaurantsWeekMenu : IReadOnlyDictionary<Restaurants, MenuItem>
    {
        private readonly Dictionary<Restaurants, MenuItem> menus;

        public RestaurantsWeekMenu(List<MenuItem> items)
        {
            menus = new Dictionary<Restaurants, MenuItem>(new RestautrantsComparer());
            if (items is null)
            {
                return;
            }
            foreach (var item in items)
            {
                if (item is null)
                {
                    continue;
                }
                menus.TryAdd(item.RestaurantName, item);
            }
        }

        public MenuItem this[Restaurants key]
        {
            get
            {
                try
                {
                    return menus[key];
                }
                catch (KeyNotFoundException)
                {
                    throw new NoProvidedMenuException(key);
                }
            }
        }

        public IEnumerable<Restaurants> Keys => menus.Keys;

        public IEnumerable<MenuItem> Values => menus.Values;

        public int Count => menus.Count;

        public bool ContainsKey(Restaurants key)
        {
            return menus.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<Restaurants, MenuItem>> GetEnumerator()
        {
            return menus.GetEnumerator();
        }

        public bool TryGetValue(Restaurants key, out MenuItem value)
        {
            return menus.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return menus.GetEnumerator();
        }
    }
}