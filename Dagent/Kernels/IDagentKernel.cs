using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections.Specialized;

namespace Dagent.Kernels
{
    internal interface IDagentKernel
    {
        DbProviderFactory ProviderFactory { get; set; }
        DbConnection Connection { get; set; }
        DbTransaction Transaction { get; set; }

        string GetSelectSql(string tableName, string[] whereParameters);
        string GetSelectCountSql(string selectSql, string[] uniqueKeys);
        string GetInsertSql(string tableName, string[] valueParameters);
        string GetUpdateSql(string tableName, string[] whereParameters, string[] valueParameters);
        string GetDeleteSql(string tableName, string[] whereParameters);
        bool OnlyTableName(string selectSql);
        DbParameter CreateDbParameter(string name, object value);
        DbCommand CreateDbCommand(string sql, KeyValuePair<string, object>[] parameters);
        DbType? GetDbType(Type type);

        int CommandTimeout { get; set; }
    }
}
