﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Data.Common;
using Dagent.Kernels;

namespace Dagent.Models
{
    internal class DagentKernelFactory
    {
        public virtual IDagentKernel CreateKernel()
        {
            ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[0];

            return CreateKernel(connectionStringSettings.Name, connectionStringSettings.ConnectionString, connectionStringSettings.ProviderName);
        }

        public virtual IDagentKernel CreateKernel(string connectionStringName)
        {
            ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];

            return CreateKernel(connectionStringSettings.Name, connectionStringSettings.ConnectionString, connectionStringSettings.ProviderName);
        }

        public virtual IDagentKernel CreateKernel(string connectionString, string providerName)
        {
            return CreateKernel(string.Empty, connectionString, providerName);
        }

        public virtual IDagentKernel CreateKernel(string connectionString, DbProviderFactory providerFactory)
        {
            return CreateKernel(string.Empty, connectionString, string.Empty, providerFactory);
        }

        protected virtual IDagentKernel CreateKernel(string connectionStringName, string connectionString, string providerName, DbProviderFactory providerFactory = null)
        {
            if (providerFactory == null)
            {
                providerFactory = DbProviderFactories.GetFactory(providerName);
            }

            DbConnection connection = providerFactory.CreateConnection();
            connection.ConnectionString = connectionString;

            Type providerFactoryType = providerFactory.GetType();

            string providerFactoryName = providerFactoryType.Namespace + "." + providerFactoryType.Name;

            if (providerFactoryName == "System.Data.SqlClient.SqlClientFactory")
            {                 
                return new SqlDagentKernel(providerFactory, connection);
            }
            else if (providerFactoryName == "System.Data.SQLite.SQLiteFactory")
            {
                return new SqlDagentKernel(providerFactory, connection);
            }
            else if (providerFactoryName == "MySql.Data.MySqlClient.MySqlClientFactory")
            {
                return new MySqlDagentKernel(providerFactory, connection);
            }

            return null;
        }
    }
}
