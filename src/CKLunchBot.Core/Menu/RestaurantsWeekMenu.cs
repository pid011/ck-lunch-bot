// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace CKLunchBot.Core.Menu
{
    public class RestaurantsWeekMenu : IReadOnlyDictionary<Restaurants, MenuItem>
    {
        private readonly Dictionary<Restaurants, MenuItem> _menus;

        public RestaurantsWeekMenu(List<MenuItem> items)
        {
            _menus = new Dictionary<Restaurants, MenuItem>(new RestautrantsComparer());
            if (items is null)
            {
                return;
            }
            foreach (MenuItem item in items)
            {
                if (item is null)
                {
                    continue;
                }
                _menus.TryAdd(item.RestaurantName, item);
            }
        }

        public MenuItem this[Restaurants key]
        {
            get
            {
                try
                {
                    return _menus[key];
                }
                catch (KeyNotFoundException)
                {
                    throw new NoProvidedMenuException(key);
                }
            }
        }

        public IEnumerable<Restaurants> Keys => _menus.Keys;

        public IEnumerable<MenuItem> Values => _menus.Values;

        public int Count => _menus.Count;

        public bool ContainsKey(Restaurants key)
        {
            return _menus.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<Restaurants, MenuItem>> GetEnumerator()
        {
            return _menus.GetEnumerator();
        }

        public bool TryGetValue(Restaurants key, out MenuItem value)
        {
            return _menus.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _menus.GetEnumerator();
        }
    }
}
