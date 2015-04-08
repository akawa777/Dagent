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

    public abstract class Row : IRow, IRowCompare, IRowModelMapper, IRowPropertyMapDefine
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
                if (!valueMap.ContainsKey(columnNames[i]))
                {
                    valueMap[columnNames[i]] = i;
                }

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

                if (!valueMap.ContainsKey(columnNames[i]))
                {
                    valueMap[columnNames[i]] = i;
                }                

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
                return columnNames;
            }
        }

        public bool ContainsColumn(string columnName)
        {
            return valueMap.ContainsKey(columnName);
        }

        public bool Compare(IRow dagentRow, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (this[columnName] == null && dagentRow[columnName] != null)
                {
                    return false;
                }
                else if (this[columnName] != null && dagentRow[columnName] == null)
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

        //public bool Compare(object[] values, params int[] indexes)
        //{
        //    for (int i = 0; i < indexes.Length; i++)
        //    {
        //        if (this[indexes[i]] == null && values[indexes[i]] != null)
        //        {
        //            return true;
        //        }
        //        else if (this[indexes[i]] != null && values[indexes[i]] == null)
        //        {
        //            return true;
        //        }
        //        else if (this[indexes[i]].ToString() != values[indexes[i]].ToString())
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        public T Map<T>(string prefix, params Expression<Func<T, object>>[] ignorePropertyExpressions) where T : class, new()
        {
            T model = new T();

            bool success = ModelMapper<T>.Map(model, this, string.Empty, prefix, ignorePropertyExpressions);

            if (success)
            {
                return model;
            }
            else
            {
                return null;
            }
        }

        public T Map<T>(string prefix) where T : class, new()
        {
            return Map<T>(prefix, null);
        }

        public T Map<T>(params Expression<Func<T, object>>[] ignorePropertyExpressions) where T : class, new()
        {
            return Map<T>(string.Empty, ignorePropertyExpressions);
        }

        public T Map<T>() where T : class, new()
        {
            return Map<T>(string.Empty, new Expression<Func<T, object>>[0]);
        }
        

        public IRowPropertyMapper<T, P> Map<T, P>(T model, Expression<Func<T, P>> targetPropertyExpression)
            where T : class, new()
            where P : class, new()
        {               
            return new RowPropertyMapper<T, P>(model, this, targetPropertyExpression);
        }        

        public IRowPropertyMapper<T, P> MapList<T, P>(T model, Expression<Func<T, List<P>>> targetPropertyExpression, params string[] uniqueKeys)
            where T : class, new()
            where P : class, new()
        {
            return new RowPropertyMapper<T, P>(model, this, targetPropertyExpression, uniqueKeys);
        }

        public IRow PrevRow { get; set; }

        public void SetValue(object[] values)
        {
            this.values = values;            
        }
    }

    public class RowPropertyMapper<T, P> : IRowPropertyMapper<T, P>
        where T : class, new()
        where P : class, new()
    {
        public RowPropertyMapper(T model, Row row, Expression<Func<T, P>> targetPropertyExpression)        
        {
            this.model = model;
            this.row = row;
            this.targetPropertyExpression = targetPropertyExpression;
        }

        public RowPropertyMapper(T model, Row row, Expression<Func<T, List<P>>> targetListPropertyExpression, string[] uniqueColumnNames)
        {
            this.model = model;
            this.row = row;
            this.targetListPropertyExpression = targetListPropertyExpression;
            this.uniqueColumnNames = uniqueColumnNames;     
        }

        private T model;
        private Row row;
        private Expression<Func<T, P>> targetPropertyExpression;
        private Expression<Func<T, List<P>>> targetListPropertyExpression;
        private string [] uniqueColumnNames;     

        public void Do(string validColumnName, string prefix, params Expression<Func<P, object>>[] ignorePropertyExpressions)
        {
            this.To(validColumnName, prefix, ignorePropertyExpressions);
        }

        public void Do(string validColumnName, string prefix)
        {
            this.To(validColumnName, prefix, new Expression<Func<P, object>>[0]);
        }

        public void Do(string validColumnName, params Expression<Func<P, object>>[] ignorePropertyExpressions)
        {
            this.To(validColumnName, string.Empty, ignorePropertyExpressions);
        }

        public void Do(string validColumnName)
        {
            this.To(validColumnName, string.Empty, new Expression<Func<P, object>>[0]);
        }

        public void Do()
        {
            this.To(string.Empty, string.Empty, new Expression<Func<P, object>>[0]);
        }

        public IRowPropertyMapperCallback<T, P> To(string validColumnName, string prefix, params Expression<Func<P, object>>[] ignorePropertyExpressions)
        {
            if (targetPropertyExpression != null)
            {
                P value = new P();

                bool success = ModelMapper<P>.Map(value, row, validColumnName, prefix, ignorePropertyExpressions);

                if (success)
                {
                    if (value == null) return new RowPropertyMapperNullCallback<T, P>();

                    PropertyInfo property = PropertyCache<T>.Map[ExpressionParser.GetMemberInfo(targetPropertyExpression).Name];

                    DynamicMethodBuilder<T>.CreateSetMethod(property)(model, value);

                    return new RowPropertyMapperCallback<T, P>(value);
                }
                else
                {
                    return new RowPropertyMapperNullCallback<T, P>();
                }
            }
            else
            {
                PropertyInfo property = PropertyCache<T>.Map[ExpressionParser.GetMemberInfo(targetListPropertyExpression).Name];

                Func<T, object> getMethod = DynamicMethodBuilder<T>.CreateGetMethod(property);

                if (getMethod(model) == null)
                {
                    DynamicMethodBuilder<T>.CreateSetMethod(property)(model, Activator.CreateInstance(property.PropertyType));
                }

                P value = null;

                if (row.PrevRow != null && uniqueColumnNames.Length > 0 && row.Compare(row.PrevRow, uniqueColumnNames))
                {
                    List<P> list = getMethod(model) as List<P>;
                    value = list[list.Count - 1];
                    return new RowPropertyMapperCallback<T, P>(value);
                }

                value = new P();

                bool success = ModelMapper<P>.Map(value, row, validColumnName, prefix, ignorePropertyExpressions);

                if (success)
                {
                    (getMethod(model) as List<P>).Add(value);
                }
                else
                {
                    return new RowPropertyMapperNullCallback<T, P>();
                }

                return new RowPropertyMapperCallback<T, P>(value);
            }
        }

        public IRowPropertyMapperCallback<T, P> To(string validColumnName, string prefix)
        {
            return this.To(validColumnName, prefix, new Expression<Func<P, object>>[0]);
        }

        public IRowPropertyMapperCallback<T, P> To(string validColumnName, params Expression<Func<P, object>>[] ignorePropertyExpressions)
        {
            return this.To(validColumnName, string.Empty, ignorePropertyExpressions);
        }

        public IRowPropertyMapperCallback<T, P> To(string validColumnName)
        {
            return this.To(validColumnName, string.Empty, new Expression<Func<P, object>>[0]);
        }

        public IRowPropertyMapperCallback<T, P> To()
        {
            return this.To(string.Empty, string.Empty, new Expression<Func<P, object>>[0]);
        }
    }

    public class RowPropertyMapperCallback<T, P> : IRowPropertyMapperCallback<T, P>
    {
        public RowPropertyMapperCallback(P value)
        {
            this.value = value;
        }

        private P value;

        public void Callback(Action<P> callback)
        {
            callback(value);
        }
    }

    public class RowPropertyMapperNullCallback<T, P> : IRowPropertyMapperCallback<T, P>
    {
        public void Callback(Action<P> callback)
        {

        }
    }
}
