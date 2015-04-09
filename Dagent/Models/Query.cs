using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections;
using Dagent.Options;
using Dagent.Library;
using System.Runtime.CompilerServices;
using Dagent;
using Dagent.Kernels;
using Dagent.Rows;

namespace Dagent.Models
{
    internal class Query<T> : IQuery<T> where T : class, new()
    {
        public Query(IDagentKernel dagentKernel, string selectSql, params Parameter[] parameters)
        {
            this.dagentKernel = dagentKernel;
            this.selectSql = selectSql;
            queryOption.Parameters = parameters == null ? new Parameter[0] : parameters;            
        }

        protected IDagentKernel dagentKernel;
        protected string selectSql;
        protected QueryOption<T> queryOption = new QueryOption<T>();        

        public virtual int Count()
        {
            using (ConnectionScope connectionScope = new ConnectionScope(dagentKernel.Connection))
            {
                DbCommand command = dagentKernel.CreateDbCommand(dagentKernel.GetSelectCountSql(selectSql, queryOption.UniqueColumnNames), ParameterConverter.GetKeyValuePairs(queryOption.Parameters));

                foreach (Parameter parameter in queryOption.Parameters)
                {
                    command.Parameters.Add(dagentKernel.CreateDbParameter(parameter.Name, parameter.Value));
                }

                object countValue = command.ExecuteScalar();                

                if (countValue == null)
                {
                    return 0;
                }
                else
                {
                    return int.Parse(countValue.ToString());
                }
            }
        }

        public virtual List<T> Page(int pageNo, int noPerPage, out int count)
        {
            using (ConnectionScope connectionScope = new ConnectionScope(dagentKernel.Connection))
            {
                 count = Count();
                return List((pageNo - 1) * noPerPage, noPerPage);
            }            
        }

        public virtual List<T> List()
        {            
            return List(0, 0);
        }

        public virtual T Single()
        {
            return List(0, 1).FirstOrDefault();
        }

        protected virtual List<T> List(int sliceIndex, int sliceCount)
        {
            using (ConnectionScope connectionScope = new ConnectionScope(dagentKernel.Connection))
            {
                DbCommand command = dagentKernel.CreateDbCommand(selectSql, ParameterConverter.GetKeyValuePairs(queryOption.Parameters));
                List<T> models = new List<T>();

                using (DbDataReader reader = command.ExecuteReader())
                {                    
                    int uniqueRowIndex = -1;

                    T model = default(T);
                    bool requestNewModel = true;

                    CurrentRow currentRow = null;                                        

                    bool firstRow = true;
                    
                    while (reader.Read())
                    {
                        if (firstRow)
                        {
                            currentRow = new CurrentRow(reader, queryOption.UniqueColumnNames);
                            firstRow = false;
                        }
                        else
                        {
                            if (currentRow.PrevRow == null)
                            {
                                currentRow.PrevRow = new CurrentRow(currentRow);
                            }
                            else
                            {   
                                currentRow.PrevRow.SetValue(currentRow.Values);
                            }

                            object[] nextValues = new object[reader.FieldCount];                                                        
                            reader.GetValues(nextValues);

                            currentRow.SetValue(nextValues);

                            requestNewModel = queryOption.UniqueColumnNames.Length == 0 ? true : !currentRow.Compare(currentRow.PrevRow, queryOption.UniqueColumnNames);
                        }                 

                        if (sliceCount != 0)
                        {
                            if (requestNewModel) uniqueRowIndex++;

                            if (sliceIndex > uniqueRowIndex)
                            {
                                continue;
                            }
                            else if (requestNewModel && sliceCount > 0 && models.Count >= sliceCount)
                            {
                                break;
                            }
                        }

                        if (requestNewModel)
                        {   
                            if (queryOption.AutoMapping)
                            {   
                                model = currentRow.Map<T>(queryOption.PrefixColumnName, queryOption.IgnorePropertyExpressions);                                
                            }
                            else
                            {
                                model = new T();
                            }

                            models.Add(model);                            
                        }                        

                        if (queryOption.MapAction != null)
                        {
                            queryOption.MapAction(model, currentRow);                               
                        }
                    }

                    return models;
                }
            }
        }

        public virtual V Scalar<V>()
        {
            using (ConnectionScope connectionScope = new ConnectionScope(dagentKernel.Connection))
            {
                DbCommand command = dagentKernel.CreateDbCommand(selectSql, ParameterConverter.GetKeyValuePairs(queryOption.Parameters));

                return (V)Convert.ChangeType(command.ExecuteScalar(), typeof(V));
            }
        }

        public virtual IQuery<T> Unique(params string[] columnNames)
        {
            queryOption.UniqueColumnNames = columnNames;
            return this;
        }

        public IQuery<T> Prefix(string prefixColumnName)
        {
            queryOption.PrefixColumnName = prefixColumnName;

            return this;
        }

        public virtual IQuery<T> Auto(bool autoMapping)
        {
            queryOption.AutoMapping = autoMapping;
            return this;
        }

        public virtual IQuery<T> Ignore(params Expression<Func<T, object>>[] ignorePropertyExpressions)
        {
            if (ignorePropertyExpressions == null)
            {
                queryOption.IgnorePropertyExpressions = new Expression<Func<T, object>>[0];
            }
            else
            {
                queryOption.IgnorePropertyExpressions = ignorePropertyExpressions;
            }

            return this;
        }

        public virtual IQuery<T> Parameters(params Parameter[] parameters)
        {
            if (parameters == null)
            {
                queryOption.Parameters = new Parameter[0];
            }
            else
            {
                queryOption.Parameters = parameters;
            }

            return this;
        }

        public virtual IQuery<T> Parameters(object parameters)
        {   
            return Parameters(ParameterConverter.GetParameters(parameters));
        }


        public virtual IQuery<T> Each(Action<T, ICurrentRow> mapAction)
        {
            if (mapAction == null)
            {
                queryOption.MapAction = (mdel, currentRow) => { };
            }
            else
            {
                queryOption.MapAction = mapAction;
            }
            return this;
        }
    }
}
