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
using Dagent.Library;
using System.Runtime.CompilerServices;
using Dagent;
using Dagent.Kernels;
using Dagent.Rows;

namespace Dagent.Models
{
    internal class Query<T> : IQuery, IQuery<T> where T : class
    {
        public Query(IDagentKernel dagentKernel, string selectSql, Parameter[] parameters)
        {
            this.dagentKernel = dagentKernel;
            this.selectSql = selectSql;
            this.parameters = parameters == null ? new Parameter[0] : parameters;
        }

        private IDagentKernel dagentKernel;
        private string selectSql;        
        private ColumnNamePropertyMap columnNamePropertyMap = new ColumnNamePropertyMap();

        private Parameter[] parameters;
        private string[] uniqueColumnNames = new string[0];
        private string prefixColumnName { get; set; }
        private bool autoMapping = true;
        private Action<T, ICurrentRow> mapAction = (model, row) => { };
        private bool ignoreCase = false;
        private Func<ICurrentRow, T> create = row => Activator.CreateInstance<T>();

        public IQuery<T> Create(Func<ICurrentRow, T> create)
        {
            this.create = create;

            return this;
        }

        public virtual List<T> Page(int pageNo, int noPerPage, out int count)
        {
            using (ConnectionScope connectionScope = new ConnectionScope(dagentKernel))
            {
                count = Count();
                return List((pageNo) * noPerPage, noPerPage).ToList();
            }            
        }

        public virtual List<T> List()
        {            
            return List(0, 0).ToList();
        }

        public virtual T Single()
        {
            return List(0, 1).FirstOrDefault();
        }
        
        protected virtual IEnumerable<T> List(int sliceIndex, int sliceNo)
        {
            using (ConnectionScope connectionScope = new ConnectionScope(dagentKernel))
            {
                DbCommand command = dagentKernel.CreateDbCommand(selectSql, ParameterConverter.GetKeyValuePairs(this.parameters));
                command.Transaction = dagentKernel.Transaction;

                List<T> models = new List<T>();

                using (DbDataReader reader = command.ExecuteReader())
                {                    
                    int uniqueRowIndex = -1;
                    int sliceCount = 0;                    
                    
                    bool requestNewModel = true;

                    CurrentRow currentRow = null;
                    CurrentRow prevRow = null;                                        

                    bool firstRow = true;
                    bool canYeld = sliceNo == 0 ? true : false;                    

                    List<CurrentRow> currentRows = new List<CurrentRow>();

                    T model = null;

                    string[] validColumnNames = new string[0];
                    
                    while (reader.Read())
                    {
                        if (firstRow)
                        {                            
                            currentRow = new CurrentRow(reader);
                            model = create(currentRow);
                            firstRow = false;
                            
                        }
                        else
                        {
                            prevRow = new CurrentRow(currentRow, true);

                            currentRow = new CurrentRow(currentRow, false);
                            reader.GetValues(currentRow.Values);
                            currentRow.PrevRow = prevRow;

                            requestNewModel = uniqueColumnNames.Length == 0 ? true : !currentRow.Compare(currentRow.PrevRow, uniqueColumnNames);
                            
                            if (canYeld && requestNewModel)
                            {
                                yield return GetModel(model, currentRows);
                                currentRows = new List<CurrentRow>();
                            }
                        }

                        if (sliceNo != 0)
                        {
                            if (requestNewModel) uniqueRowIndex++;

                            if (sliceIndex > uniqueRowIndex)
                            {
                                canYeld = false;
                                continue;
                            }
                            else if (requestNewModel)
                            {
                                canYeld = true;
                                sliceCount++;
                            }
                        }

                        currentRows.Add(currentRow);                        

                        if (requestNewModel)
                        {
                            model = create(currentRow);

                            if (autoMapping)
                            {
                                ModelMapper<T>.Map(model, currentRow, validColumnNames, prefixColumnName, columnNamePropertyMap, ignoreCase);
                            }
                        }

                        if (sliceNo != 0 && sliceNo == sliceCount)
                        {
                            break;
                        }
                    }

                    if (model != null)
                    {
                        yield return GetModel(model, currentRows);
                    }
                }
            }
        }

        private T GetModel (T model, List<CurrentRow> currentRows)
        {
            if (mapAction != null)
            {
                foreach (CurrentRow currentRow in currentRows)
                {
                    mapAction(model, currentRow);    
                }   
            }

            return model;
        }

        public virtual IQuery<T> Unique(params string[] columnNames)
        {
            this.uniqueColumnNames = columnNames;
            return this;
        }

        public IQuery<T> Prefix(string prefixColumnName)
        {
            this.prefixColumnName = prefixColumnName;

            return this;
        }

        public virtual IQuery<T> AutoMapping(bool autoMapping)
        {
            this.autoMapping = autoMapping;
            return this;
        }        

        public virtual IQuery<T> Parameters(params Parameter[] parameters)
        {
            if (parameters == null)
            {
                this.parameters = new Parameter[0];
            }
            else
            {
                this.parameters = parameters;
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
                this.mapAction = (mdel, currentRow) => { };
            }
            else
            {
                this.mapAction = mapAction;
            }

            return this;
        }

        public IQuery<T> Ignore(params Expression<Func<T, object>>[] ignoreProperties)
        {
            foreach (var property in ignoreProperties)
            {
                this.columnNamePropertyMap.IgnoreProperty<T>(ExpressionParser.GetPropertyInfo<T, object>(property));
            }
            
            return this;
        }

        public IQuery<T> IgnoreCase(bool ignore)
        {
            ignoreCase = ignore;

            return this;
        }

        public virtual int Count()
        {
            using (ConnectionScope connectionScope = new ConnectionScope(dagentKernel))
            {
                DbCommand command = dagentKernel.CreateDbCommand(dagentKernel.GetSelectCountSql(selectSql, this.uniqueColumnNames), ParameterConverter.GetKeyValuePairs(this.parameters));
                command.Transaction = dagentKernel.Transaction;

                foreach (Parameter parameter in this.parameters)
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

        public V Scalar<V>()
        {
            using (ConnectionScope connectionScope = new ConnectionScope(this.dagentKernel))
            {
                DbCommand command = dagentKernel.CreateDbCommand(selectSql, ParameterConverter.GetKeyValuePairs(parameters));
                command.Transaction = this.dagentKernel.Transaction;

                object val = command.ExecuteScalar();

                if (val == null)
                {
                    return default(V);
                }

                return (V)Convert.ChangeType(command.ExecuteScalar(), typeof(V));
            }
        }

        IQuery IQuery.Unique(params string[] columnNames)
        {
            this.uniqueColumnNames = columnNames;
            return this;
        }

        IQuery IQuery.Parameters(params Parameter[] parameters)
        {
            if (parameters == null)
            {
                this.parameters = new Parameter[0];
            }
            else
            {
                this.parameters = parameters;
            }

            return this;
        }

        IQuery IQuery.Parameters(object parameters)
        {
            Parameter[] parameterItems = ParameterConverter.GetParameters(parameters);

            if (parameterItems == null)
            {
                this.parameters = new Parameter[0];
            }
            else
            {
                this.parameters = parameterItems;
            }

            return this;
        }


        void IQuery.Execute()
        {
            List();
        }

        public IQuery Each(Action<ICurrentRow> mapAction)
        {
            Action<T, ICurrentRow> action = (obj, row) => mapAction(row);
            Each(action);

            return this;
        }
    }
}
