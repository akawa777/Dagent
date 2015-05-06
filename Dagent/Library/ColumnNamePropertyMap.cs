using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Dagent.Library
{
    internal class ColumnNamePropertyMap
    {        
        private Dictionary<string, string> propertyNameMap = new Dictionary<string, string>();
        private Dictionary<string, PropertyInfo> propertyNameIgnoreMap = new Dictionary<string, PropertyInfo>();        

        private string GetKey<T>(PropertyInfo property)
        {
            return typeof(T).FullName + ":" + property.Name;
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

        public void IgnoreProperty<T>(PropertyInfo property)
        {
            propertyNameIgnoreMap[GetKey<T>(property)] = property;
        }
    }
}
