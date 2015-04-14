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
using Dagent.Exceptions;

namespace Dagent.Rows
{

    internal abstract class Row : IRow, IRowCompare, IRowModelMapper, IRowPropertyMapDefine
    {
        public Row(Row row, bool canSetValue)
        {
            columnCount = row.columnCount;
            columnTypes = row.columnTypes;
            columnNames = row.columnNames;
            values = new object[columnCount];
            if (canSetValue) Array.Copy(row.values, values, values.Length);
            valueMap = row.valueMap;

            columnNamePropertyMap = row.columnNamePropertyMap;
        }

        public Row(IDataReader dataReader, ColumnNamePropertyMap columnNamePropertyMap)
        {
            columnCount = dataReader.FieldCount;
            columnTypes = new Type[columnCount];
            columnNames = new string[columnCount];

            this.columnNamePropertyMap = columnNamePropertyMap;

            values = new object[columnCount];
            dataReader.GetValues(values);

            valueMap = new Dictionary<string, int>();            
            List<int> uniqueKeyIndexList = new List<int>();            

            for (int i = 0; i < columnCount; i++)
            {
                columnTypes[i] = dataReader.GetFieldType(i);
                columnNames[i] = dataReader.GetName(i);

                if (!valueMap.ContainsKey(columnNames[i]))
                {
                    valueMap[columnNames[i]] = i;
                } 
            }
        }

        private int columnCount;
        private Type[] columnTypes;
        private string[] columnNames;
        protected object[] values;
        private Dictionary<string, int> valueMap;        

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
            get 
            {
                int index;
                if (valueMap.TryGetValue(columnName, out index))
                {
                    return values[valueMap[columnName]]; 
                }

                throw new Exception(ExceptionMessges.NotExistColumnName(columnName));                
            }
            set 
            {   
                values[valueMap[columnName]] = value; 
            }
        }

        public bool TryGetValue(string columnName, out object value)
        {
            int index;
            if (valueMap.TryGetValue(columnName, out index))
            {
                value = values[valueMap[columnName]];
                return true;
            }

            value = null;
            return false;
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
                return columnNames;
            }
        }

        public IRow PrevRow { get; set; }

        public void SetValue(object[] values)
        {
            this.values = values;
        }

        public bool ContainsColumn(string columnName)
        {
            return valueMap.ContainsKey(columnName);
        }

        public bool Compare(IRow dagentRow, params string[] columnNames)
        {            
            foreach (string columnName in columnNames)
            {
                if (this[columnName] == DBNull.Value && dagentRow[columnName] != DBNull.Value)
                {
                    return false;
                }
                else if (this[columnName] != DBNull.Value && dagentRow[columnName] == DBNull.Value)
                {
                    return false;
                }
                else if (this[columnName].ToString() != dagentRow[columnName].ToString())
                {
                    return false;
                }
            }

            return true;
        }

        private ColumnNamePropertyMap columnNamePropertyMap;

        public T Map<T>(string[] validColumnNames, string prefixColumnName) where T : class, new()
        {
            T model = new T();

            bool success = ModelMapper<T>.Map(model, this, validColumnNames, prefixColumnName, columnNamePropertyMap);

            if (success)
            {
                return model;
            }
            else
            {
                return null;
            }
        }

        public IRowPropertyMapper<T, P> Map<T, P>(T model, Expression<Func<T, P>> targetPropertyExpression, params string[] validColumnNames)
            where T : class, new()
            where P : class, new()
        {
            return new RowPropertyMapper<T, P>(model, this, targetPropertyExpression, validColumnNames, columnNamePropertyMap);
        }

        public IRowPropertyMapper<T, P> Map<T, P>(T model, Expression<Func<T, List<P>>> targetListPropertyExpression, params string[] validColumnNames)
            where T : class, new()
            where P : class, new()
        {
            return new RowPropertyMapper<T, P>(model, this, targetListPropertyExpression, validColumnNames, columnNamePropertyMap);
        }        
    }

    
}
