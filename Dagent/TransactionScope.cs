using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace Dagent
{  
    internal class TransactionScope : ConnectionScope, ITransactionScope
    {
        public TransactionScope(DbConnection connection)
            : base(connection)
        {
            
        }

        public TransactionScope(DbConnection connection, IsolationLevel isolationLevel)
            : base(connection)
        {
            this.isolationLevel = isolationLevel;
            this.settedIsolationLevel = true;
        }

        protected DbTransaction transaction;
        protected bool settedIsolationLevel;
        protected IsolationLevel isolationLevel;

        protected override void BeginOpen()
        {
            base.BeginOpen();

            if (this.connection.State == ConnectionState.Open)
            {
                if (settedIsolationLevel)
                {
                    transaction = this.connection.BeginTransaction(isolationLevel);                    
                }
                else
                {
                    transaction = this.connection.BeginTransaction();
                }
            }
        }

        public virtual void Commit()
        {
            transaction.Commit();
        }

        public virtual void Rollback()
        {
            transaction.Rollback();
        }

        protected override void BeginClose()
        {
            if (transaction != null && transaction.Connection != null)
            {   
                transaction.Rollback();
            }

            base.BeginClose();
        }          
    }
}
