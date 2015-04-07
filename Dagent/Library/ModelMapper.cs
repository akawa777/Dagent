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

        private struct SetterDelegateSet
        {
            public PropertyInfo Property { get; set; }            
            public Action<T, object> Setter { get; set; }
        }   

        public static void Map(IRow dagentRow, T model, string prefix, params Expression<Func<T, object>>[] ignorePropertyExpressions)
        {
            Dictionary<string, MemberInfo> ignoreMemberMap = default(Dictionary<string, MemberInfo>);
            if (ignorePropertyExpressions != null && ignorePropertyExpressions.Length > 0)
            {
                ignoreMemberMap = ExpressionParser.GetMemberInfoMap<T>(ignorePropertyExpressions);
            }

            for (int i = 0; i < dagentRow.ColumnCount; i++)
            {
                string columnName = dagentRow.GetColumnName(i);

                if (!string.IsNullOrEmpty(prefix))
                {
                    if (columnName.Length > prefix.Length && columnName.Substring(0, prefix.Length) == prefix)
                    {
                        columnName = columnName.Substring(prefix.Length, columnName.Length - prefix.Length);
                    }
                    else
                    {
                        continue;
                    }
                }

                SetterDelegateSet setterDelegateSet = default(SetterDelegateSet);
                if (!setterDelegateSetMap.TryGetValue(columnName, out setterDelegateSet))
                {
                    continue;
                }

                object value = dagentRow[i];
                if (CanChangeType(value, setterDelegateSet.Property.PropertyType) && (ignoreMemberMap == null || !ignoreMemberMap.ContainsKey(setterDelegateSet.Property.Name)))
                {
                    if (value.GetType() == typeof(DBNull) || value == null)
                    {                        
                        setterDelegateSet.Setter(model, null);
                    }
                    else
                    {
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
