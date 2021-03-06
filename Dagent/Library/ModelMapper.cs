﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Common;
using System.Reflection.Emit;
using Dagent.Rows;
using System.ComponentModel;
using System.Globalization;

namespace Dagent.Library
{
    internal static class ModelMapper<T>
    {
        private static TextInfo textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;

        public static bool Map(T model, IRow row, string[] validColumnNames, string prefixColumnName, ColumnNamePropertyMap columnNamePropertyMap, bool ignoreCase)
        {   
            bool existData = false;

            Dictionary<string, string> columnMap = new Dictionary<string, string>();
            Dictionary<string, bool> validColumnNameMap = new Dictionary<string, bool>();

            if (validColumnNames != null)
            {
                foreach (string validColumnName in validColumnNames)
                {
                    validColumnNameMap[validColumnName] = true;
                }
            }
            
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                if (!property.CanWrite)
                {
                    continue;
                }

                string columnName;

                if (!columnNamePropertyMap.TryGetColumnName<T>(property, out columnName))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(prefixColumnName))
                {
                    columnName = prefixColumnName + columnName;                                        
                }                

                Action<T, object> setter = DynamicMethodBuilder<T>.CreateSetMethod(property);

                object value;

                if (!row.TryGetValue(columnName, out value))
                {
                    if (ignoreCase)
                    {
                        foreach (string name in row.ColumnNames)
                        {
                            if (String.Compare(name, property.Name, true) == 0)
                            {
                                value = row[name];
                                break;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                if (CanChangeType(value, property.PropertyType))
                {
                    if (value == DBNull.Value)
                    {
                        if (validColumnNameMap.ContainsKey(columnName))
                        {
                            validColumnNameMap[columnName] = false;
                        }
                    }
                    else
                    {
                        existData = true;
                        if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                        {
                            object baseValue = Convert.ChangeType(value, property.PropertyType.GetGenericArguments()[0]);
                            setter(model, baseValue);
                        }
                        else
                        {
                            setter(model, Convert.ChangeType(value, property.PropertyType));
                        }
                    }
                }
            }

            if (validColumnNameMap.Count > 0 && validColumnNameMap.All(x => x.Value == false))
            {                
                return false;
            }
            else if (!existData)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static bool CanChangeType(object value, Type conversionType)
        {
            if (conversionType == null
                || value == null
                || !(value is IConvertible))
            {
                return false;
            }

            if (conversionType.IsClass && conversionType != typeof(string))
            {
                return false;
            }

            return true;
        }
    }   
}
