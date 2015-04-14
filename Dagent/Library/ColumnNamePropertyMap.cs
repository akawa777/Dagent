using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Dagent.Library
{
    internal class ColumnNamePropertyMap
    {
        private Dictionary<string, PropertyInfo> columnMap = new Dictionary<string, PropertyInfo>();
        private Dictionary<string, string> propertyNameMap = new Dictionary<string, string>();
        private Dictionary<string, PropertyInfo> propertyNameIgnoreMap = new Dictionary<string, PropertyInfo>();   
     
        private string GetKey<T>(string columnName)
        {
            return typeof(T).FullName + ":" + columnName;
        }

        private string GetKey<T>(PropertyInfo property)
        {
            return GetKey<T>(property.Name);
        }
       
        public void Column<T>(string columnName, PropertyInfo property)
        {
            columnMap[GetKey<T>(columnName)] = property;
            propertyNameMap[GetKey<T>(property)] = columnName;
        }

        public bool RemoveColumn<T>(string columnName)
        {
            PropertyInfo property;

            if (columnMap.TryGetValue(columnName, out property))
            {
                propertyNameMap.Remove(GetKey<T>(property));
            }

            return columnMap.Remove(GetKey<T>(columnName));
        }

        public bool Clear<T>()
        {
            bool rtn = false;
            foreach (string key in columnMap.Keys)
            {
                if (key.Split(':')[0] == typeof(T).FullName)
                {
                    if (!rtn && RemoveColumn<T>(key))
                    {
                        rtn = true;
                    }
                }
            }

            foreach (var keyValue in propertyNameIgnoreMap)
            {
                if (keyValue.Key.Split(':')[0] == typeof(T).FullName)
                {
                    if (!rtn && RemoveIgnore<T>(keyValue.Value))
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
            propertyNameIgnoreMap.Clear();
        }

        public bool TryGetProperty<T>(string columnName, out PropertyInfo property)
        {
            if (!columnMap.TryGetValue(GetKey<T>(columnName), out property))
            {
                property = PropertyCache<T>.GetProperty(columnName);
            }            

            if (property == null)
            {
                return false;
            }

            if (propertyNameIgnoreMap.ContainsKey(GetKey<T>(property)))
            {
                property = null;
                return false;
            }

            return true;
        }

        public bool TryGetColumnName<T>(PropertyInfo property, out string columnName)
        {   
            if (propertyNameIgnoreMap.ContainsKey(GetKey<T>(property)))
            {
                columnName = null;
                return false;
            }

            if (propertyNameMap.TryGetValue(GetKey<T>(property), out columnName))
            {                
                return true;
            }

            columnName = property.Name;            

            return true;
        }

        public void Ignore<T>(PropertyInfo property)
        {
            propertyNameIgnoreMap[GetKey<T>(property)] = property;
        }

        public bool RemoveIgnore<T>(PropertyInfo property)
        {
            return propertyNameIgnoreMap.Remove(GetKey<T>(property));
        }
    }
}
