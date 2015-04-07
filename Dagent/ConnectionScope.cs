using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace Dagent
{
    internal class ConnectionScope : IConnectionScope
    {
        public ConnectionScope(DbConnection connection)
        {
            this.connection = connection;
            BeginOpen();
        }

        protected virtual void BeginOpen()
        {
            if (this.connection.State == ConnectionState.Closed)
            {
                hasConnectionOpened = true;
                this.connection.Open();
            }
        }

        protected DbConnection connection;
        protected bool hasConnectionOpened = false;

        public virtual void Dispose()
        {
            BeginClose();
        }

        protected virtual void BeginClose()
        {
            if (hasConnectionOpened && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }
    }
}
