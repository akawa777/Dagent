using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Linq.Expressions;
using Dagent.Library;
using Dagent.Rows;
using Dagent;
using Dagent.Kernels;
using Dagent.Exceptions;

namespace Dagent.Models
{
    internal class Command<T> : ICommand<T>
        where T : class
    {
        public Command(IDagentKernel dagentKernel, string tableName, string[] primaryKeys)
        {
            this.dagentKernel = dagentKernel;
            this.tableName = tableName;
            this.primaryKeys = primaryKeys;
        }

        private IDagentKernel dagentKernel;
        private string tableName;
        private ColumnNamePropertyMap columnNamePropertyMap = new ColumnNamePropertyMap();        
        private string[] primaryKeys;
        private bool autoMapping = true;
        private Action<IUpdateRow, T> mapAction = (row, model) => { };
        private string where = string.Empty;
        private Parameter[] parameters;

        protected virtual int Execute(DataRowState rowState, T entity)
        {
            using (ConnectionScope connectionScope = new ConnectionScope(dagentKernel))
            {
                Dictionary<string, object> columnValueMap = new Dictionary<string, object>();

                if (this.autoMapping)
                {
                    foreach (var property in typeof(T).GetProperties())
                    {                    
                        if (!property.CanRead)
                        {
                            continue;
                        }

                        DbType? dbType = dagentKernel.GetDbType(property.PropertyType);
                        string columnName;
                        if (dbType.HasValue && columnNamePropertyMap.TryGetColumnName<T>(property, out columnName))
                        {
                            columnValueMap[columnName] = DynamicMethodBuilder<T>.CreateGetMethod(property)(entity);
                        }
                    }
                }

                UpdateRow updateRow = new UpdateRow(columnValueMap);

                this.mapAction(updateRow, entity);

                List<KeyValuePair<string, object>> valueParameters = new List<KeyValuePair<string, object>>();

                foreach (string columnName in updateRow.ColumnNames)
                {
                    object value = updateRow[columnName];
                    if (value == null)
                    {
                        value = DBNull.Value;
                    }
                    valueParameters.Add(new KeyValuePair<string, object>(columnName, value));
                }

                if (parameters != null)
                {
                    foreach (Parameter parameter in parameters)
                    {
                        valueParameters.Add(new KeyValuePair<string, object>(parameter.Name, parameter.Value));
                    }
                }

                string sql = "";

                if (rowState == DataRowState.Added)
                {
                    sql = dagentKernel.GetInsertSql(tableName, this.primaryKeys, updateRow.ColumnNames);
                }
                else if (rowState == DataRowState.Modified)
                {
                    sql = dagentKernel.GetUpdateSql(tableName, this.primaryKeys, updateRow.ColumnNames) 
                        + (string.IsNullOrEmpty(this.where) ? string.Empty : string.Format(" and ({0}) ", where));
                    
                }
                else if (rowState == DataRowState.Deleted)
                {
                    sql = dagentKernel.GetDeleteSql(tableName, this.primaryKeys)
                        + (string.IsNullOrEmpty(this.where) ? string.Empty : string.Format(" and ({0}) ", where));
                }

                DbCommand command = dagentKernel.CreateDbCommand(sql, valueParameters.ToArray());
                command.Transaction = dagentKernel.Transaction;

                return command.ExecuteNonQuery();
            }
        }        

        public virtual int Insert(T entity)
        {
            return Execute(DataRowState.Added, entity);
        }

        public virtual int Update(T entity)
        {
            return Execute(DataRowState.Modified, entity);
        }

        public virtual int Delete(T entity)
        {
            return Execute(DataRowState.Deleted, entity);
        }

        public ICommand<T> Map(Action<IUpdateRow, T> mapAction)
        {
            if (mapAction == null)
            {
                this.mapAction = (t, row) => { };
            }
            else
            {
                this.mapAction = mapAction;
            }

            return this;
        }

        public virtual ICommand<T> AutoMapping(bool autoMapping)
        {
            this.autoMapping = autoMapping;
            return this;
        }

        public ICommand<T> Ignore(params Expression<Func<T, object>>[] ignoreProperties)
        {
            foreach (var property in ignoreProperties)
            {
                this.columnNamePropertyMap.IgnoreProperty<T>(ExpressionParser.GetPropertyInfo<T, object>(property));
            }

            return this;
        }


        public ICommand<T> Where(string where, params Parameter[] parameters)
        {
            this.where = where;

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

        public ICommand<T> Where(string where, object parameters)
        {
            this.where = where;

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
    }
}
