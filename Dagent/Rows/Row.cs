﻿using System;
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

        public T Map<T>(string[] validColumnNames, string prefixColumnName, Expression<Func<T, object>>[] ignorePropertyExpressions) where T : class, new()
        {
            T model = new T();

            bool success = ModelMapper<T>.Map(model, this, validColumnNames, prefixColumnName, ignorePropertyExpressions, columnNamePropertyMap);

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

    internal class RowPropertyMapper<T, P> : IRowPropertyMapper<T, P>
        where T : class, new()
        where P : class, new()
    {
        public RowPropertyMapper(T model, Row row, Expression<Func<T, P>> targetPropertyExpression, string[] validColumnNames, ColumnNamePropertyMap columnNamePropertyMap)
        {
            this.model = model;
            this.row = row;
            this.targetPropertyExpression = targetPropertyExpression;
            this.validColumnNames = validColumnNames;
            this.columnNamePropertyMap = columnNamePropertyMap;
        }

        public RowPropertyMapper(T model, Row row, Expression<Func<T, List<P>>> targetListPropertyExpression, string[] validColumnNames, ColumnNamePropertyMap columnNamePropertyMap)
        {
            this.model = model;
            this.row = row;
            this.targetListPropertyExpression = targetListPropertyExpression;
            this.validColumnNames = validColumnNames;
            this.columnNamePropertyMap = columnNamePropertyMap;
        }

        private T model;
        private Row row;
        private Expression<Func<T, P>> targetPropertyExpression;
        private Expression<Func<T, List<P>>> targetListPropertyExpression;
        string[] validColumnNames;
        
        private string [] uniqueColumnNames = new string[0];
        private string prefixColumnName;
        private Expression<Func<P, object>>[] ignorePropertyExpressions = new Expression<Func<P,object>>[0];        
        private bool autoMapping = true;
        private Action<P> mapAction;

        private ColumnNamePropertyMap columnNamePropertyMap;

        public void Do()
        {
            if (targetPropertyExpression != null)
            {
                P value = new P();

                bool success = true;

                if (autoMapping)
                {
                    success = ModelMapper<P>.Map(value, row, validColumnNames, prefixColumnName, ignorePropertyExpressions, columnNamePropertyMap);
                }

                if (success)
                {
                    PropertyInfo property = PropertyCache<T>.GetProperty(ExpressionParser.GetMemberInfo(targetPropertyExpression).Name);
                    
                    DynamicMethodBuilder<T, P>.CreateSetMethod(property)(model, value);

                    if (mapAction != null)
                    {
                        mapAction(value);
                    }
                }
            }
            else
            {
                PropertyInfo property = PropertyCache<T>.GetProperty(ExpressionParser.GetMemberInfo(targetListPropertyExpression).Name);

                Func<T, List<P>> getMethod = DynamicMethodBuilder<T, List<P>>.CreateGetMethod(property);

                List<P> list = getMethod(model);                

                if (list == null)
                {
                    list = new List<P>();
                    DynamicMethodBuilder<T, List<P>>.CreateSetMethod(property)(model, list);
                }

                P value;

                if (list.Count != 0 && row.PrevRow != null && uniqueColumnNames.Length > 0 && row.Compare(row.PrevRow, uniqueColumnNames))
                {   
                    value = list[list.Count - 1];

                    if (mapAction != null)
                    {
                        mapAction(value);
                    }
                }
                else
                {
                    value = new P();

                    bool success = true;

                    if (autoMapping)
                    {
                        success = ModelMapper<P>.Map(value, row, validColumnNames, prefixColumnName, ignorePropertyExpressions, columnNamePropertyMap);
                    }

                    if (success)
                    {
                        list.Add(value);

                        if (mapAction != null)
                        {
                            mapAction(value);
                        }
                    }
                }
            }
        }

        public IRowPropertyMapper<T, P> Prefix(string prefixColumnName)
        {
            this.prefixColumnName = prefixColumnName;

            return this;
        }

        public IRowPropertyMapper<T, P> Ignore(params Expression<Func<P, object>>[] ignorePropertyExpressions)
        {
            this.ignorePropertyExpressions = ignorePropertyExpressions;

            return this;
        }


        public IRowPropertyMapper<T, P> Unique(params string[] uniqueColumnNames)
        {
            this.uniqueColumnNames = uniqueColumnNames;

            return this;
        }

        public IRowPropertyMapper<T, P> Auto(bool autoMapping)
        {
            this.autoMapping = autoMapping;

            return this;
        }


        public IRowPropertyMapper<T, P> Each(Action<P> mapAction)
        {
            this.mapAction = mapAction;

            return this;
        }
    }
}
