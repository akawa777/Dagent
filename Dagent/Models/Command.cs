using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Linq.Expressions;
using Dagent.Options;
using Dagent.Library;
using Dagent.Rows;
using Dagent;
using Dagent.Kernels;

namespace Dagent.Models
{
    internal class Command<T> : ICommand<T>
        where T : class, new()
    {
        public Command(IDagentKernel dagentKernel, string tableName, params string[] primaryKeys)
        {
            this.dagentKernel = dagentKernel;
            this.tableName = tableName;
            
            if (primaryKeys != null)
            {
                foreach (string key in primaryKeys)
                {
                    PropertyInfo property = null;
                    if (PropertyCache<T>.Map.TryGetValue(key, out property))
                    {
                        this.commandOption.PrimaryKeys[key] = DynamicMethodBuilder<T>.CreateGetMethod(property);
                    }                    
                }
            }
        }

        protected IDagentKernel dagentKernel;
        protected string tableName;        

        protected virtual int Execute(DataRowState rowState, T entity)
        {
            using (ConnectionScope connectionScope = new ConnectionScope(dagentKernel.Connection))
            {
                Dictionary<string, object> columnValueMap = new Dictionary<string, object>();

                if (commandOption.AutoMapping)
                {
                    foreach (var property in PropertyCache<T>.Map.Values)
                    {
                        DbType? dbType = dagentKernel.GetDbType(property.PropertyType);
                        if (dbType.HasValue)
                        {
                            columnValueMap[property.Name] = DynamicMethodBuilder<T>.CreateGetMethod(property)(entity);
                        }
                    }
                }

                UpdateRow updateRow = new UpdateRow(columnValueMap);

                commandOption.MapAction(updateRow, entity);

                List<KeyValuePair<string, object>> primaryKeyParameters = new List<KeyValuePair<string, object>>();

                foreach (string key in commandOption.PrimaryKeys.Keys)
                {
                    primaryKeyParameters.Add(new KeyValuePair<string, object>(key, commandOption.PrimaryKeys[key](entity)));
                }

                List<KeyValuePair<string, object>> valueParameters = new List<KeyValuePair<string, object>>();

                foreach (string columnName in updateRow.ColumnNames)
                {
                    valueParameters.Add(new KeyValuePair<string, object>(columnName, updateRow[columnName]));
                }

                string sql = "";

                if (rowState == DataRowState.Added)
                {
                    sql = dagentKernel.GetInsertSql(tableName, updateRow.ColumnNames);
                }
                else if (rowState == DataRowState.Modified)
                {
                    sql = dagentKernel.GetUpdateSql(tableName, commandOption.PrimaryKeys.Keys.ToArray(), updateRow.ColumnNames);
                }
                else if (rowState == DataRowState.Deleted)
                {
                    sql = dagentKernel.GetDeleteSql(tableName, commandOption.PrimaryKeys.Keys.ToArray());
                }

                DbCommand command = dagentKernel.CreateDbCommand(sql, valueParameters.ToArray());

                return command.ExecuteNonQuery();
            }
        }

        protected CommandOption<T> commandOption = new CommandOption<T>();

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
                commandOption.MapAction = (t, row) => { };
            }
            else
            {
                commandOption.MapAction = mapAction;
            }

            return this;
        }

        public virtual ICommand<T> AutoMapping(bool autoMapping)
        {
            commandOption.AutoMapping = autoMapping;
            return this;
        }

        public virtual ICommand<T> IgnoreProperties(params Expression<Func<T, object>>[] ignorePropertyExpressions)
        {
            if (ignorePropertyExpressions == null)
            {
                commandOption.IgnorePropertyExpressions = new Expression<Func<T, object>>[0];
            }
            else
            {
                commandOption.IgnorePropertyExpressions = ignorePropertyExpressions;
            }

            return this;
        }  
    }
}
