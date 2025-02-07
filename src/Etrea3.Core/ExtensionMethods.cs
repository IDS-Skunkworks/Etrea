using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Etrea3.Core
{
    public static class ExtensionMethods
    {
        private static readonly Random rnd = new Random();

        public static Tkey GetRandomElement<Tkey, Tvalue>(this ConcurrentDictionary<Tkey, Tvalue> dictionary)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                Game.LogMessage($"DEBUG: GetRandomElement was called on a null or empty dictionary", LogLevel.Debug);
                return default(Tkey);
            }
            var keys = dictionary.Keys.ToList();
            var itemID = rnd.Next(keys.Count);
            return keys[itemID];
        }

        public static T GetRandomElement<T>(this List<T> elements)
        {
            if (elements == null || elements.Count == 0)
            {
                Game.LogMessage($"DEBUG: GetRandomElement was called on a null or empty list", LogLevel.Debug);
                return default(T);
            }
            var elementID = rnd.Next(elements.Count);
            return elements[elementID];
        }

        public static void Add<Tk, Tv>(this ConcurrentDictionary<Tk, Tv> dictionary, Tk key, Tv value)
        {
            dictionary.TryAdd(key, value);
        }

        //public static Tobj Clone<Tobj>(this Tobj obj)
        //{
        //    if (!typeof(Tobj).IsSerializable)
        //    {
        //        Game.LogMessage($"ERROR: Cannot clone {typeof(Tobj)}, it is not serialisable", LogLevel.Error, true);
        //        return default(Tobj);
        //    }
        //    return JsonConvert.DeserializeObject<Tobj>(JsonConvert.SerializeObject(obj));
        //}
    }
}
