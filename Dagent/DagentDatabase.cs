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

namespace Dagent
{
    public class DagentDatabase : IDagentDatabase
    {
        public DagentDatabase()
        {            
            dagentKernel = dagentKernelFactory.CreateKernel();            
        }

        public DagentDatabase(string name)
        {         
            dagentKernel = dagentKernelFactory.CreateKernel(name);            
        }

        public DagentDatabase(string connectionString, string providerName)
        {         
            dagentKernel = dagentKernelFactory.CreateKernel(connectionString, providerName);            
        }

        public DagentDatabase(string name, string connectionString, string providerName)
        {         
            dagentKernel = dagentKernelFactory.CreateKernel(name, connectionString, providerName);            
        }

        private IDagentKernel dagentKernel;
        private DagentKernelFactory dagentKernelFactory = new DagentKernelFactory();        

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

        public virtual IQuery<T> Query<T>(string tableNameOrSelectSql) where T : new()
        {
            return Query<T>(tableNameOrSelectSql, null);
        }

        public virtual IQuery<T> Query<T>(string tableNameOrSelectSql, object parameters) where T : new()
        {   
            return Query<T>(tableNameOrSelectSql, ParameterConverter.GetParameters(parameters));
        }

        public virtual IQuery<T> Query<T>(string tableNameOrSelectSql, params Parameter[] parameters) where T : new()
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

        public virtual ICommand<T> Command<T>(string tableName, params string[] primaryKeys) where T : new()
        {
            Command<T> dagentCommand = new Command<T>(dagentKernel, tableName, primaryKeys);

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
    }
}
