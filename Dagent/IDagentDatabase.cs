using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Dagent.Kernels;

namespace Dagent
{
    public interface IDagentDatabase
    {        
        DbConnection Connection { get; set; }

        int ExequteNonQuery(string sql, params Parameter[] parameters);
        object ExequteScalar(string sql, params Parameter[] parameters);
        int Fill(DataTable dataTable, string selectSql, params Parameter[] parameters);
        int Update(DataTable dataTable, string selectSql, params Parameter[] parameters);
        DbDataReader ExecuteReader(string selectSql, params Parameter[] parameters);

        IQuery<T> Query<T>(string tableNameOrSelectSql, params Parameter[] parameters) where T : class, new();
        IQuery<T> Query<T>(string tableNameOrSelectSql, object parameters) where T : class, new();

        ICommand<T> Command<T>(string tableName, params string[] primaryKeys) where T : class, new();

        IConnectionScope ConnectionScope();
        ITransactionScope TransactionScope();
        ITransactionScope TransactionScope(IsolationLevel isolationLevel);

        IConfig Config { get; }
    }

    public interface IConfig
    {
        int CommandTimeout { get; set; }
    }
}
