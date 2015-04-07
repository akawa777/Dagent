using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Dagent.Library;
using System.Data.Common;
using System.Data;

namespace Dagent.Rows
{

    public abstract class Row : IRow, IRowDiffer, IRowMapper
    {
        public Row(Row row)
        {
            columnCount = row.columnCount;
            columnTypes = row.columnTypes;
            columnNames = row.columnNames;
            values = new object[columnCount];
            Array.Copy(row.values, values, values.Length);
            valueMap = row.valueMap;
            uniqueKeyIndexes = row.uniqueKeyIndexes;
        }

        public Row(Type[] columnTypes, string[] columnNames, object[] values, params string[] uniqueKeys)
        {
            columnCount = columnNames.Length;
            this.columnTypes = columnTypes;
            this.columnNames = columnNames;
            this.values = values;
            valueMap = new Dictionary<string, int>();
            uniqueKeyIndexes = new int[uniqueKeys.Length];
            int uniqueKeyCount = 0;

            for (int i = 0; i < columnCount; i++)
            {
                valueMap[this.columnNames[i]] = i;

                if (uniqueKeys != null && uniqueKeys.Any(x => x == columnNames[i]))
                {
                    uniqueKeyIndexes[uniqueKeyCount] = i;
                    uniqueKeyCount++;
                }
            }
        }

        public Row(IDataReader dataReader, params string[] uniqueKeys)
        {
            columnCount = dataReader.FieldCount;
            columnTypes = new Type[columnCount];
            columnNames = new string[columnCount];

            values = new object[columnCount];
            dataReader.GetValues(values);

            valueMap = new Dictionary<string, int>();            
            List<int> uniqueKeyIndexList = new List<int>();            

            for (int i = 0; i < columnCount; i++)
            {
                columnTypes[i] = dataReader.GetFieldType(i);
                columnNames[i] = dataReader.GetName(i);

                valueMap[columnNames[i]] = i;

                if (uniqueKeys != null && uniqueKeys.Any(x => x == columnNames[i]))
                {
                    uniqueKeyIndexList.Add(i);                    
                }
            }

            uniqueKeyIndexes = uniqueKeyIndexList.ToArray();
        }

        private int columnCount;
        private Type[] columnTypes;
        private string[] columnNames;
        protected object[] values;
        private Dictionary<string, int> valueMap;
        private int[] uniqueKeyIndexes;

        public int ColumnCount
        {
            get { return columnCount; }
        }

        public Type GetColumnType(int i)
        {
            return columnTypes[i];
        }

        public string GetColumnName(int i)
        {
            return columnNames[i];
        }

        public int GetOrdinal(string columnName)
        {
            return valueMap[columnName];
        }

        public int[] GetUniqueKeyIndexes()
        {
            return uniqueKeyIndexes;
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

        public object this[string columnName]
        {
            get { return values[valueMap[columnName]]; }
            set 
            {   
                values[valueMap[columnName]] = value; 
            }
        }

        public object this[int i]
        {
            get { return values[i]; }
            set { values[i] = value; }
        }        

        public object[] Values
        {
            get { return values; }            
        }

        public string[] ColumnNames
        {
            get
            {
                return columnNames.ToArray();
            }
        }

        public bool Compare(IRow dagentRow, params string[] columnNames)
        {
            int[] indexes = new int[columnNames.Length];

            for (int i = 0; i < columnNames.Length; i++)
            {
                indexes[i] = dagentRow.GetOrdinal(columnNames[i]);
            }

            return Compare(dagentRow.Values, indexes);
        }

        public bool Compare(object[] values, params int[] indexes)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                if (this[indexes[i]] == null && values[indexes[i]] != null)
                {
                    return true;
                }
                else if (this[indexes[i]] != null && values[indexes[i]] == null)
                {
                    return true;
                }
                else if (this[indexes[i]].ToString() != values[indexes[i]].ToString())
                {
                    return true;
                }
            }

            return false;
        }

        public T Map<T>(string prefix, params Expression<Func<T, object>>[] ignorePropertyExpressions) where T : new()
        {
            T model = new T();

            ModelMapper<T>.Map(this, model, prefix, ignorePropertyExpressions);

            return model;
        }

        public T Map<T>(string prefix) where T : new()
        {
            return Map<T>(prefix, null);
        }

        public T Map<T>(params Expression<Func<T, object>>[] ignorePropertyExpressions) where T : new()
        {
            return Map<T>(null, ignorePropertyExpressions);
        }

        public T Map<T>() where T : new()
        {
            return Map<T>(null, new Expression<Func<T, object>>[0]);
        }

        public bool ContainsColumn(string columnName)
        {
            return valueMap.ContainsKey(columnName);
        }
    }
}
