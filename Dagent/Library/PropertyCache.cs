using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Dagent.Library
{
    internal static class PropertyCache<T>
    {
        static PropertyCache()
        {
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                map[property.Name] = property;
            }
        }

        private static readonly Dictionary<string, PropertyInfo> map = new Dictionary<string, PropertyInfo>();
        public static Dictionary<string, PropertyInfo> Map
        {
            get { return map; }
        }
    }
}
