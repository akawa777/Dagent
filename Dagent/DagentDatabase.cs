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
            _config = new Config(dagentKernel);
        }

        public DagentDatabase(string connectionStringName)
        {
            dagentKernel = dagentKernelFactory.CreateKernel(connectionStringName);
            _config = new Config(dagentKernel);
        }

        public DagentDatabase(string connectionString, string providerName)
        {         
            dagentKernel = dagentKernelFactory.CreateKernel(connectionString, providerName);
            _config = new Config(dagentKernel);
        }

        private IDagentKernel dagentKernel;
        private DagentKernelFactory dagentKernelFactory = new DagentKernelFactory();

        private ColumnNamePropertyMap columnNamePropertyMap = new ColumnNamePropertyMap();

        private IConfig _config;        

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
            using (ConnectionScope connectionScope = new ConnectionScope(this.dagentKernel))
            {
                DbCommand command = dagentKernel.CreateDbCommand(sql, ParameterConverter.GetKeyValuePairs(parameters));     
                command.Transaction = this.dagentKernel.Transaction;
                return command.ExecuteNonQuery();
            }
        }

        public virtual object ExequteScalar(string sql, params Parameter[] parameters)
        {
            using (ConnectionScope connectionScope = new ConnectionScope(this.dagentKernel))
            {                
                DbCommand command = dagentKernel.CreateDbCommand(sql, ParameterConverter.GetKeyValuePairs(parameters));
                command.Transaction = this.dagentKernel.Transaction;
                return command.ExecuteScalar();
            }
        }

        public virtual int Fill(DataTable dataTable, string selectSql, params Parameter[] parameters)
        {
            using (ConnectionScope connectionScope = new ConnectionScope(this.dagentKernel))
            {
                DbCommand command = dagentKernel.CreateDbCommand(selectSql, ParameterConverter.GetKeyValuePairs(parameters));
                command.Transaction = this.dagentKernel.Transaction;

                DbDataAdapter dataAdapter = dagentKernel.ProviderFactory.CreateDataAdapter();
                dataAdapter.SelectCommand = command;

                return dataAdapter.Fill(dataTable);
            }
        }

        public virtual int Update(DataTable dataTable, string selectSql, params Parameter[] parameters)
        {
            using (ConnectionScope connectionScope = new ConnectionScope(this.dagentKernel))
            {
                DbCommand command = dagentKernel.CreateDbCommand(selectSql, ParameterConverter.GetKeyValuePairs(parameters));
                command.Transaction = this.dagentKernel.Transaction;                

                DbDataAdapter dataAdapter = dagentKernel.ProviderFactory.CreateDataAdapter();
                dataAdapter.SelectCommand = command;

                DbCommandBuilder commandBuilder = dagentKernel.ProviderFactory.CreateCommandBuilder();
                commandBuilder.DataAdapter = dataAdapter;

                return dataAdapter.Update(dataTable);
            }
        }

        public virtual DbDataReader ExecuteReader(string selectSql, params Parameter[] parameters)
        {
            return ExecuteReader(CommandBehavior.Default, selectSql, parameters);
        }

        public virtual DbDataReader ExecuteReader(CommandBehavior commandBehavior, string selectSql, params Parameter[] parameters)
        {
            DbCommand command = dagentKernel.CreateDbCommand(selectSql, ParameterConverter.GetKeyValuePairs(parameters));
            command.Transaction = this.dagentKernel.Transaction;

            return command.ExecuteReader(commandBehavior);
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

            Query<T> dagentQuery = new Query<T>(dagentKernel, selectSql, parameters);            

            return dagentQuery;
        }

        public virtual ICommand<T> Command<T>(string tableName, params string[] primaryKeys) where T : class, new()
        {
            Command<T> dagentCommand = new Command<T>(dagentKernel, tableName, primaryKeys);

            return dagentCommand;
        }

        public IConnectionScope ConnectionScope()
        {
            return new ConnectionScope(this.dagentKernel);
        }

        public ITransactionScope TransactionScope()
        {
            return new TransactionScope(this.dagentKernel);
        }

        public ITransactionScope TransactionScope(IsolationLevel isolationLevel)
        {
            return new TransactionScope(this.dagentKernel, isolationLevel);
        }

        public IConfig Config
        {
            get
            {
                return _config;
            }
        }

        public IQuery Query(string tableNameOrSelectSql, params Parameter[] parameters)
        {
            return Query<object>(tableNameOrSelectSql, parameters) as IQuery;
        }

        public IQuery Query(string tableNameOrSelectSql, object parameters)
        {
            return Query<object>(tableNameOrSelectSql, parameters) as IQuery;
        }
    }
}
