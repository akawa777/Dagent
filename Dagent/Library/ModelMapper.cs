using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Common;
using System.Reflection.Emit;
using Dagent.Rows;
using System.ComponentModel;

namespace Dagent.Library
{
    internal static class ModelMapper<T>
    {
        static ModelMapper()
        {
            foreach (PropertyInfo property in PropertyCache<T>.Map.Values)
            {
                setterDelegateSetMap[property.Name] = new SetterDelegateSet { Property = property, Setter = DynamicMethodBuilder<T>.CreateSetMethod(property) };
            }
        }

        private static Dictionary<string, SetterDelegateSet> setterDelegateSetMap = new Dictionary<string, SetterDelegateSet>();

        private class SetterDelegateSet
        {
            public PropertyInfo Property { get; set; }            
            public Action<T, object> Setter { get; set; }
        }

        public static bool Map(T model, IRow dagentRow, string[] validColumnNames, string prefixColumnName, Expression<Func<T, object>>[] ignorePropertyExpressions)
        {
            Dictionary<string, MemberInfo> ignoreMemberMap = default(Dictionary<string, MemberInfo>);
            if (ignorePropertyExpressions != null && ignorePropertyExpressions.Length > 0)
            {
                ignoreMemberMap = ExpressionParser.GetMemberInfoMap<T>(ignorePropertyExpressions);
            }
            
            bool existData = false;

            if (validColumnNames == null) validColumnNames = new string[0];
            if (ignorePropertyExpressions == null) ignorePropertyExpressions = new Expression<Func<T, object>>[0];

            Dictionary<string, string> columnMap = new Dictionary<string, string>();
            Dictionary<string, bool> validColumnNameMap = new Dictionary<string, bool>();            

            foreach (string validColumnName in validColumnNames)
            {
                validColumnNameMap[validColumnName] = true;
            }

            for (int i = 0; i < dagentRow.ColumnCount; i++)
            {
                string originalColumnName = dagentRow.GetColumnName(i);
                string columnName = originalColumnName;

                if (columnMap.ContainsKey(columnName))
                {
                    continue;
                }
                else
                {
                    columnMap[columnName] = columnName;
                }

                if (!string.IsNullOrEmpty(prefixColumnName))
                {
                    if (columnName.Length > prefixColumnName.Length && columnName.Substring(0, prefixColumnName.Length) == prefixColumnName)
                    {
                        columnName = columnName.Substring(prefixColumnName.Length, columnName.Length - prefixColumnName.Length);
                    }
                    else
                    {
                        continue;
                    }
                }

                SetterDelegateSet setterDelegateSet;
                if (!setterDelegateSetMap.TryGetValue(columnName, out setterDelegateSet))
                {
                    continue;
                }

                object value = dagentRow[i];
                if (CanChangeType(value, setterDelegateSet.Property.PropertyType) && (ignoreMemberMap == null || !ignoreMemberMap.ContainsKey(setterDelegateSet.Property.Name)))
                {
                    if (value == DBNull.Value)
                    {   
                        if (validColumnNameMap.ContainsKey(originalColumnName))
                        {
                            validColumnNameMap[originalColumnName] = false;
                        }
                    }
                    else
                    {
                        existData = true;
                        if (Nullable.GetUnderlyingType(setterDelegateSet.Property.PropertyType) != null)
                        {
                            object baseValue = Convert.ChangeType(value, setterDelegateSet.Property.PropertyType.GetGenericArguments()[0]);
                            setterDelegateSet.Setter(model, baseValue);
                        }
                        else
                        {
                            setterDelegateSet.Setter(model, Convert.ChangeType(value, setterDelegateSet.Property.PropertyType));
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

            return true;
        }
    }   
}
