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
    internal class RowPropertyMapper<T, P> : IRowPropertyMapper<T, P>
        where T : class, new()
        where P : class, new()
    {
        public RowPropertyMapper(T model, Row row, Expression<Func<T, P>> targetPropertyExpression, string[] validColumnNames)
        {
            this.model = model;
            this.row = row;
            this.targetPropertyExpression = targetPropertyExpression;
            this.validColumnNames = validColumnNames;
            this.columnNamePropertyMap = new ColumnNamePropertyMap();
        }

        public RowPropertyMapper(T model, Row row, Expression<Func<T, List<P>>> targetListPropertyExpression, string[] validColumnNames)
        {
            this.model = model;
            this.row = row;
            this.targetListPropertyExpression = targetListPropertyExpression;
            this.validColumnNames = validColumnNames;
            this.columnNamePropertyMap = new ColumnNamePropertyMap();
        }

        private T model;
        private Row row;
        private Expression<Func<T, P>> targetPropertyExpression;
        private Expression<Func<T, List<P>>> targetListPropertyExpression;
        string[] validColumnNames;

        private string[] uniqueColumnNames = new string[0];
        private string prefixColumnName;
        private Expression<Func<P, object>>[] ignorePropertyExpressions = new Expression<Func<P, object>>[0];
        private bool autoMapping = true;
        private Action<P> mapAction;
        private bool ignoreCase = false;

        private ColumnNamePropertyMap columnNamePropertyMap;

        public void Do()
        {
            if (targetPropertyExpression != null)
            {
                PropertyInfo property = ExpressionParser.GetPropertyInfo(targetPropertyExpression);

                if (!property.CanWrite)
                {
                    return;
                }

                P value = new P();

                bool success = true;

                if (autoMapping)
                {
                    success = ModelMapper<P>.Map(value, row, validColumnNames, prefixColumnName, columnNamePropertyMap, ignoreCase);
                }

                if (success)
                {
                    DynamicMethodBuilder<T, P>.CreateSetMethod(property)(model, value);

                    if (mapAction != null)
                    {
                        mapAction(value);
                    }
                }
            }
            else if (targetListPropertyExpression != null)
            {
                PropertyInfo property = ExpressionParser.GetPropertyInfo(targetListPropertyExpression);

                if (!property.CanWrite || !property.CanRead)
                {
                    return;
                }

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
                        success = ModelMapper<P>.Map(value, row, validColumnNames, prefixColumnName, columnNamePropertyMap, ignoreCase);
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


        public IRowPropertyMapper<T, P> Ignore(params Expression<Func<P, object>>[] ignoreProperties)
        {
            foreach (var property in ignoreProperties)
            {
                this.columnNamePropertyMap.IgnoreProperty<P>(ExpressionParser.GetPropertyInfo<P, object>(property));
            }

            return this;
        }


        public IRowPropertyMapper<T, P> IgnoreCase(bool ignore)
        {
            ignoreCase = ignore;

            return this;
        }
    }
}
