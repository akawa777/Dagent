using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Dagent.Library;
using System.Data.Common;
using Dagent;

namespace Dagent.Rows
{
    internal class UpdateRow : IUpdateRow
    {
        public UpdateRow(Dictionary<string, object> columnValueMap)
        {
            this.columnValueMap = columnValueMap;
        }

        public Dictionary<string, object> columnValueMap;

        public object this[string columnName]
        {
            get
            {
                return columnValueMap[columnName];
            }
            set
            {
                columnValueMap[columnName] = value;
            }
        }

        public bool ContainsColumn(string columnName)
        {
            return columnValueMap.ContainsKey(columnName);
        }

        public string[] ColumnNames
        {
            get
            {
                return columnValueMap.Keys.ToArray();
            }
        }

        public T Get<T>(string columnName)
        {
            object value = this[columnName];

            if (value == null || value.GetType() == typeof(DBNull))
            {
                if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return default(T);
                }
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public object[] Values
        {
            get { return columnValueMap.Values.ToArray(); }
        }
    }
}
