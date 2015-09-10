using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Dagent.Kernels
{
    internal abstract class DagentKernel : IDagentKernel
    {
        public DagentKernel(DbProviderFactory providerFactory, DbConnection connection)
        {
            ProviderFactory = providerFactory;
            Connection = connection;

            SetTypeMap();

            DbCommand command = ProviderFactory.CreateCommand();
            CommandTimeout = command.CommandTimeout;
        }

        public virtual DbProviderFactory ProviderFactory { get; set; }
        public virtual DbConnection Connection { get; set; }

        private Regex rxColumns = new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        private Regex rxOrderBy = new Regex(@"\bORDER\s+BY\s+(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\.\[\] ""`])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\.\[\] ""`])+(?:\s+(?:ASC|DESC))?)*(?!.*FROM)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        protected virtual string ParameterPrefix
        {
            get
            {
                return "@";
            }
        }

        protected virtual DbType? GetDbType(object value)
        {
            if (value == null) return null;

            Type type = value.GetType();

            return GetDbType(type);
        }

        public DbType? GetDbType(Type type)
        {
            if (type == null) return null;

            DbType dbType;

            if (typeMap.TryGetValue(type, out dbType))
            {
                return dbType;
            }
            else
            {
                return null;
            }
        }

        protected Dictionary<Type, DbType> typeMap;

        protected virtual void SetTypeMap()
        {
            typeMap = new Dictionary<Type, DbType>();
            typeMap[typeof(byte)] = DbType.Byte;
            typeMap[typeof(sbyte)] = DbType.SByte;
            typeMap[typeof(short)] = DbType.Int16;
            typeMap[typeof(ushort)] = DbType.UInt16;
            typeMap[typeof(int)] = DbType.Int32;
            typeMap[typeof(uint)] = DbType.UInt32;
            typeMap[typeof(long)] = DbType.Int64;
            typeMap[typeof(ulong)] = DbType.UInt64;
            typeMap[typeof(float)] = DbType.Single;
            typeMap[typeof(double)] = DbType.Double;
            typeMap[typeof(decimal)] = DbType.Decimal;
            typeMap[typeof(bool)] = DbType.Boolean;
            typeMap[typeof(string)] = DbType.String;
            typeMap[typeof(char)] = DbType.StringFixedLength;
            typeMap[typeof(Guid)] = DbType.Guid;
            typeMap[typeof(DateTime)] = DbType.DateTime;
            typeMap[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            typeMap[typeof(TimeSpan)] = DbType.Time;
            typeMap[typeof(byte[])] = DbType.Binary;
            typeMap[typeof(byte?)] = DbType.Byte;
            typeMap[typeof(sbyte?)] = DbType.SByte;
            typeMap[typeof(short?)] = DbType.Int16;
            typeMap[typeof(ushort?)] = DbType.UInt16;
            typeMap[typeof(int?)] = DbType.Int32;
            typeMap[typeof(uint?)] = DbType.UInt32;
            typeMap[typeof(long?)] = DbType.Int64;
            typeMap[typeof(ulong?)] = DbType.UInt64;
            typeMap[typeof(float?)] = DbType.Single;
            typeMap[typeof(double?)] = DbType.Double;
            typeMap[typeof(decimal?)] = DbType.Decimal;
            typeMap[typeof(bool?)] = DbType.Boolean;
            typeMap[typeof(char?)] = DbType.StringFixedLength;
            typeMap[typeof(Guid?)] = DbType.Guid;
            typeMap[typeof(DateTime?)] = DbType.DateTime;
            typeMap[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
            typeMap[typeof(TimeSpan?)] = DbType.Time;
            typeMap[typeof(Object)] = DbType.Object;
        }

        public virtual string GetSelectSql(string tableName, string[] whereParameters)
        {
            StringBuilder sql = new StringBuilder(string.Format("select * from {0} ", tableName));

            if (whereParameters != null && whereParameters.Length > 0)
            {
                sql.Append("where ");

                for (int i = 0; i < whereParameters.Length; i++)
                {
                    if (i == 0) sql.Append(whereParameters[i] + " = @" + whereParameters[i]);
                    else sql.Append(" and " + whereParameters[i] + " = @" + whereParameters[i]);
                }
            }

            return sql.ToString();
        }

        public virtual string GetSelectCountSql(string selectSql, string[] uniqueKeys)
        {
            string selectSqlUnordered = rxOrderBy.Replace(selectSql, string.Empty);
            if (uniqueKeys == null || uniqueKeys.Length == 0)
            {
                return string.Format("select count(*) from ( {0} ) countTable ", selectSqlUnordered);
            }
            else
            {
                string columnSql = string.Join(", ", uniqueKeys);

                return string.Format("select count(*) from ( select {0} from ( {1} ) groupTable group by {2} ) countTable", columnSql, selectSqlUnordered, columnSql);
            }
        }

        public virtual string GetInsertSql(string tableName, string[] whereParameters, params string[] valueParameters)
        {
            StringBuilder names = new StringBuilder();
            StringBuilder values = new StringBuilder();
            StringBuilder where = new StringBuilder();

            if (valueParameters != null && valueParameters.Length > 0)
            {
                for (int i = 0; i < valueParameters.Length; i++)
                {
                    if (i == 0)
                    {
                        names.Append(valueParameters[i]);
                        values.Append("@" + valueParameters[i]);
                    }
                    else
                    {
                        names.Append(", " + valueParameters[i]);
                        values.Append(", @" + valueParameters[i]);                        
                    }
                }
            }

            if (whereParameters != null && whereParameters.Length > 0)
            {
                for (int i = 0; i < whereParameters.Length; i++)
                {
                    if (i == 0)
                    {
                        where.Append(whereParameters[i] + " = @" + whereParameters[i]);
                    }
                    else
                    {
                        where.Append(" and " + whereParameters[i] + " = @" + whereParameters[i]);
                    }
                }
            }

            //StringBuilder sql = new StringBuilder(string.Format("insert into {0} ({1}) values({2})", tableName, names.ToString(), values.ToString()));
            StringBuilder sql = new StringBuilder(string.Format("insert into {0} ({1}) select {2} where not exists (select {3} from {0} where {4})", tableName, names.ToString(), values.ToString(), whereParameters[0], where.ToString()));

            return sql.ToString();
        }

        public virtual string GetUpdateSql(string tableName, string[] whereParameters, params string[] valueParameters)
        {
            StringBuilder values = new StringBuilder();
            StringBuilder where = new StringBuilder();

            if (valueParameters != null && valueParameters.Length > 0)
            {
                for (int i = 0; i < valueParameters.Length; i++)
                {
                    if (i == 0)
                    {
                        values.Append(valueParameters[i] + " = @" + valueParameters[i]);                        
                    }
                    else
                    {
                        values.Append(", " + valueParameters[i] + " = @" + valueParameters[i]);                        
                    }
                }
            }

            if (whereParameters != null && whereParameters.Length > 0)
            {
                for (int i = 0; i < whereParameters.Length; i++)
                {
                    if (i == 0)
                    {
                        where.Append(whereParameters[i] + " = @" + whereParameters[i]);
                    }
                    else
                    {
                        where.Append(" and " + whereParameters[i] + " = @" + whereParameters[i]);
                    }
                }
            }

            StringBuilder sql = new StringBuilder(string.Format("update {0} set {1} where {2}", tableName, values.ToString(), where.ToString()));

            return sql.ToString();
        }

        public virtual string GetDeleteSql(string tableName, string[] whereParameters)
        {            
            StringBuilder where = new StringBuilder();

            if (whereParameters != null && whereParameters.Length > 0)
            {
                for (int i = 0; i < whereParameters.Length; i++)
                {
                    if (i == 0)
                    {
                        where.Append(whereParameters[i] + " = @" + whereParameters[i]);
                    }
                    else
                    {
                        where.Append(" and " + whereParameters[i] + " = @" + whereParameters[i]);
                    }
                }
            }

            StringBuilder sql = new StringBuilder(string.Format("delete from {0} where {1}", tableName, where.ToString()));

            return sql.ToString();
        }

        public virtual bool OnlyTableName(string selectSql)
        {
            if (selectSql.IndexOf(Environment.NewLine) == -1 && selectSql.IndexOf("\t") == -1 && selectSql.IndexOf(" ") == -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual DbParameter CreateDbParameter(DbParameter parameter)
        {
            DbParameter dbParameter = ProviderFactory.CreateParameter();

            dbParameter.ParameterName = ParameterPrefix + parameter.ParameterName;
            dbParameter.Value = parameter.Value;

            DbType? dbType = GetDbType(parameter.Value);
            if (dbType.HasValue) dbParameter.DbType = dbType.Value;

            return dbParameter;
        }


        public virtual DbCommand CreateDbCommand(string sql)
        {
            DbCommand command = ProviderFactory.CreateCommand();
            command.Connection = Connection;

            command.CommandText = sql;
            command.CommandTimeout = CommandTimeout;            

            return command;
        }


        public DbTransaction Transaction { get; set; }


        public int CommandTimeout
        {
            get;
            set;
        }


        public bool Rollbakced { get; set; }
    }
}
