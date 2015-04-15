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
using Dagent.Exceptions;

namespace Dagent.Models
{
    internal class Command<T> : ICommand<T>
        where T : class, new()
    {
        public Command(IDagentKernel dagentKernel, string tableName, string[] primaryKeys)
        {
            this.dagentKernel = dagentKernel;
            this.tableName = tableName;
            this.primaryKeys = primaryKeys;

            config = new Config(columnNamePropertyMap);
        }

        protected IDagentKernel dagentKernel;
        protected string tableName;
        private ColumnNamePropertyMap columnNamePropertyMap = new ColumnNamePropertyMap();
        private Config config;
        private string[] primaryKeys;

        protected virtual int Execute(DataRowState rowState, T entity)
        {
            if (primaryKeys != null)
            {
                foreach (string key in primaryKeys)
                {
                    PropertyInfo property;
                    if (!columnNamePropertyMap.TryGetProperty<T>(key, out property))
                    {
                        throw new Exception(ExceptionMessges.NotExistProperty(typeof(T), key));
                    }

                    this.commandOption.PrimaryKeys[key] = DynamicMethodBuilder<T>.CreateGetMethod(property);
                }
            }

            using (ConnectionScope connectionScope = new ConnectionScope(dagentKernel))
            {
                Dictionary<string, object> columnValueMap = new Dictionary<string, object>();

                if (commandOption.AutoMapping)
                {
                    foreach (var property in typeof(T).GetProperties())
                    {                        
                        DbType? dbType = dagentKernel.GetDbType(property.PropertyType);
                        string columnName;
                        if (dbType.HasValue && columnNamePropertyMap.TryGetColumnName<T>(property, out columnName))
                        {
                            columnValueMap[columnName] = DynamicMethodBuilder<T>.CreateGetMethod(property)(entity);
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
                    object value = updateRow[columnName];
                    if (value == null)
                    {
                        value = DBNull.Value;
                    }
                    valueParameters.Add(new KeyValuePair<string, object>(columnName, value));
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
                command.Transaction = dagentKernel.Transaction;

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

        public virtual ICommand<T> Auto(bool autoMapping)
        {
            commandOption.AutoMapping = autoMapping;
            return this;
        }

        public ICommand<T> Config(Action<IConfig> setConfigAction)
        {
            if (setConfigAction != null)
            {
                setConfigAction(config);
            }

            return this;
        }
    }
}
