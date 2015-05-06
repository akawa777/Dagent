using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace Dagent.Library
{
    internal static class PropertyCache<T>
    {
        static PropertyCache()
        {
            properties = typeof(T).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                map[property.Name] = property;
            }
        }

        private static Dictionary<string, PropertyInfo> map = new Dictionary<string, PropertyInfo>();
        private static PropertyInfo[] properties;

        public static PropertyInfo GetProperty(string propertyName)
        {
            PropertyInfo property;

            if (map.TryGetValue(propertyName, out property))
            {
                return property;
            }

            return null;
        }

        public static PropertyInfo[] GetProperties()
        {
            return properties;
        }
    }
}
