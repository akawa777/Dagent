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

        public static PropertyInfo GetProperty(string propertyName)
        {
            PropertyInfo property;

            if (map.TryGetValue(propertyName, out property))
            {
                return property;
            }

            return null;
        }
    }

    internal class ColumnNamePropertyMap
    {
        private Dictionary<string, PropertyInfo> columnMap = new Dictionary<string, PropertyInfo>();
        private Dictionary<string, string> propertyNameMap = new Dictionary<string, string>();   
     
        private string GetKey<T>(string name)
        {
            return typeof(T).FullName + ":" + name;
        }
       
        public void Set<T>(string columnName, PropertyInfo property)
        {
            columnMap[GetKey<T>(columnName)] = property;
            propertyNameMap[GetKey<T>(property.Name)] = columnName;
        }

        public bool Remove<T>(string columnName)
        {
            PropertyInfo property;

            if (columnMap.TryGetValue(columnName, out property))
            {
                propertyNameMap.Remove(GetKey<T>(property.Name));
            }

            return columnMap.Remove(GetKey<T>(columnName));
        }

        public bool Remove<T>()
        {
            bool rtn = false;
            foreach (string key in columnMap.Keys)
            {
                if (key.Split(':')[0] == typeof(T).FullName)
                {
                    if (!rtn && Remove<T>(key))
                    {
                        rtn = true;
                    }
                }
            }

            return rtn;
        }

        public void Clear()
        {
            columnMap.Clear();
            propertyNameMap.Clear();
        }

        public PropertyInfo GetProperty<T>(string columnName)
        {
            PropertyInfo property;

            if (columnMap.TryGetValue(GetKey<T>(columnName), out property))
            {
                return property;
            }

            return PropertyCache<T>.GetProperty(columnName);
        }

        public string GetColumnName<T>(string propertyName)
        {
            string columnName;

            if (propertyNameMap.TryGetValue(GetKey<T>(propertyName), out columnName))
            {
                return columnName;
            }

            return PropertyCache<T>.GetProperty(propertyName).Name;
        }
    }
}
