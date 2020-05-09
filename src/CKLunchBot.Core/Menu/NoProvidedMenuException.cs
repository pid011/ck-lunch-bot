using System;
using System.Runtime.Serialization;

namespace CKLunchBot.Core.Menu
{
    /// <summary>
    /// Represents an exception when target menu data is not exist.
    /// </summary>
    [Serializable]
    public class NoProvidedMenuException : Exception
    {
        public Restaurants[] RestaurantsName { get; }

        public NoProvidedMenuException(params Restaurants[] name)
        {
            RestaurantsName = name;
        }

        public NoProvidedMenuException(string message, params Restaurants[] name) : base(message)
        {
            RestaurantsName = name;
        }

        public NoProvidedMenuException(string message, Exception inner, params Restaurants[] name) : base(message, inner)
        {
            RestaurantsName = name;
        }

        protected NoProvidedMenuException(SerializationInfo info,
                                          StreamingContext context,
                                          params Restaurants[] name) : base(info, context)
        {
            RestaurantsName = name;
        }
    }
}