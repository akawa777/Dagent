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
        DbDataReader ExecuteReader(CommandBehavior commandBehavior, string selectSql, params Parameter[] parameters);

        IQuery<T> Query<T>(string tableNameOrSelectSql, params Parameter[] parameters) where T : class;
        IQuery<T> Query<T>(string tableNameOrSelectSql, object parameters) where T : class;

        IQuery Query(string tableNameOrSelectSql, params Parameter[] parameters);
        IQuery Query(string tableNameOrSelectSql, object parameters);        

        ICommand<T> Command<T>(string tableName, params string[] primaryKeys) where T : class;

        IConnectionScope ConnectionScope();
        ITransactionScope TransactionScope();
        ITransactionScope TransactionScope(IsolationLevel isolationLevel);

        IConfig Config { get; }
    }
}
