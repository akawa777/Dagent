using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Dagent.Models;
using Dagent.Kernels;
using Dagent.Library;
using System.Linq.Expressions;

namespace Dagent
{
    public class DagentDatabase : IDagentDatabase
    {
        public DagentDatabase()
        {            
            dagentKernel = dagentKernelFactory.CreateKernel();
            config = new Config(columnNamePropertyMap);
        }

        public DagentDatabase(string connectionStringName)
        {
            dagentKernel = dagentKernelFactory.CreateKernel(connectionStringName);
            config = new Config(columnNamePropertyMap);
        }

        public DagentDatabase(string connectionString, string providerName)
        {         
            dagentKernel = dagentKernelFactory.CreateKernel(connectionString, providerName);
            config = new Config(columnNamePropertyMap);
        }

        public DagentDatabase(string connectionStringName, string connectionString, string providerName)
        {
            dagentKernel = dagentKernelFactory.CreateKernel(connectionStringName, connectionString, providerName);
            config = new Config(columnNamePropertyMap);
        }

        private IDagentKernel dagentKernel;
        private DagentKernelFactory dagentKernelFactory = new DagentKernelFactory();

        private ColumnNamePropertyMap columnNamePropertyMap = new ColumnNamePropertyMap();

        public virtual DbConnection Connection 
        {
            get
            {
                return dagentKernel.Connection;
            }
            set
            {                
                dagentKernel.Connection = value;
            }
        }

        public virtual int ExequteNonQuery(string sql, params Parameter[] parameters)
        {
            using (ConnectionScope connectionScope = new ConnectionScope(Connection))
            {
                DbCommand command = dagentKernel.CreateDbCommand(sql, ParameterConverter.GetKeyValuePairs(parameters));
                return command.ExecuteNonQuery();
            }
        }

        public virtual object ExequteScalar(string sql, params Parameter[] parameters)
        {
            using (ConnectionScope connectionScope = new ConnectionScope(Connection))
            {
                DbCommand command = dagentKernel.CreateDbCommand(sql, ParameterConverter.GetKeyValuePairs(parameters));
                return command.ExecuteScalar();
            }
        }

        public virtual int Fill(DataTable dataTable, string selectSql, params Parameter[] parameters)
        {
            using (ConnectionScope connectionScope = new ConnectionScope(Connection))
            {
                DbCommand command = dagentKernel.CreateDbCommand(selectSql, ParameterConverter.GetKeyValuePairs(parameters));

                DbDataAdapter dataAdapter = dagentKernel.ProviderFactory.CreateDataAdapter();
                dataAdapter.SelectCommand = command;

                return dataAdapter.Fill(dataTable);
            }
        }

        public virtual int Update(DataTable dataTable, string selectSql, params Parameter[] parameters)
        {
            using (ConnectionScope connectionScope = new ConnectionScope(Connection))
            {
                DbCommand command = dagentKernel.CreateDbCommand(selectSql, ParameterConverter.GetKeyValuePairs(parameters));

                DbDataAdapter dataAdapter = dagentKernel.ProviderFactory.CreateDataAdapter();
                dataAdapter.SelectCommand = command;

                DbCommandBuilder commandBuilder = dagentKernel.ProviderFactory.CreateCommandBuilder();
                commandBuilder.DataAdapter = dataAdapter;

                return dataAdapter.Update(dataTable);
            }
        }

        public virtual IQuery<T> Query<T>(string tableNameOrSelectSql) where T : class, new()
        {
            return Query<T>(tableNameOrSelectSql, null);
        }

        public virtual IQuery<T> Query<T>(string tableNameOrSelectSql, object parameters) where T : class, new()
        {   
            return Query<T>(tableNameOrSelectSql, ParameterConverter.GetParameters(parameters));
        }

        public virtual IQuery<T> Query<T>(string tableNameOrSelectSql, params Parameter[] parameters) where T : class, new()
        {
            string selectSql = null;
            
            if (dagentKernel.OnlyTableName(tableNameOrSelectSql))
            {

                selectSql = dagentKernel.GetSelectSql(tableNameOrSelectSql, parameters == null || parameters.Length == 0 ? null : parameters.Select(x => x.Name).ToArray());
            }
            else
            {
                selectSql = tableNameOrSelectSql;
            }

            Query<T> dagentQuery = new Query<T>(dagentKernel, selectSql, parameters, columnNamePropertyMap);            

            return dagentQuery;
        }

        public virtual ICommand<T> Command<T>(string tableName, params string[] primaryKeys) where T : class, new()
        {
            Command<T> dagentCommand = new Command<T>(dagentKernel, tableName, primaryKeys,columnNamePropertyMap);

            return dagentCommand;
        }

        public IConnectionScope ConnectionScope()
        {
            return new ConnectionScope(Connection);
        }

        public ITransactionScope TransactionScope()
        {
            return new TransactionScope(Connection);
        }

        public ITransactionScope TransactionScope(IsolationLevel isolationLevel)
        {
            return new TransactionScope(Connection, isolationLevel);
        }

        private Config config;
       
        //public IConfig Config()
        //{
        //    return config;
        //}
    }

    public interface IConfig
    {
        IMap Map();
    }

    internal class Config : IConfig
    {
        public Config(ColumnNamePropertyMap columnNamePropertyMap)
        {            
            map = new Map(columnNamePropertyMap);
        }

        private Map map;
        public IMap Map()
        {
            return map;
        }
    }

    public interface IMap
    {
        IMap SetColumn<T, P>(Expression<Func<T, P>> propertyExpression, string columnName);
        IMap RemoveColumn<T>(string columnName);
        IMap RemoveColumn<T>();
        IMap ClearColumn();        
    }

    internal class Map : IMap
    {
        public Map(ColumnNamePropertyMap columnNamePropertyMap)
        {
            this.columnNamePropertyMap = columnNamePropertyMap;
        }

        private ColumnNamePropertyMap columnNamePropertyMap = new ColumnNamePropertyMap();

        public IMap SetColumn<T, P>(Expression<Func<T, P>> propertyExpression, string columnName)
        {
            columnNamePropertyMap.Set<T>(columnName, PropertyCache<T>.GetProperty(propertyExpression.Body.ToString().Split('.')[1]));

            return this;
        }

        public IMap RemoveColumn<T>(string columnName)
        {
            columnNamePropertyMap.Remove<T>(columnName);
            return this;
        }

        public IMap RemoveColumn<T>()
        {
            columnNamePropertyMap.Remove<T>();
            return this;
        }

        public IMap ClearColumn()
        {
            columnNamePropertyMap.Clear();
            return this;
        }
    }
}
