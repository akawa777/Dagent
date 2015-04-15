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
    internal class TransactionScope : ConnectionScope, ITransactionScope
    {
        public TransactionScope(IDagentKernel kernel)
            : base(kernel)
        {
            
        }

        public TransactionScope(IDagentKernel kernel, IsolationLevel isolationLevel)
            : base(kernel)
        {
            this.isolationLevel = isolationLevel;
            this.settedIsolationLevel = true;
        }
        
        protected bool settedIsolationLevel;
        protected IsolationLevel isolationLevel;

        protected override void BeginOpen()
        {
            base.BeginOpen();

            if (this.kernel.Connection.State == ConnectionState.Open)
            {
                if (settedIsolationLevel)
                {
                    this.kernel.Transaction = this.kernel.Connection.BeginTransaction(isolationLevel);                    
                }
                else
                {
                    this.kernel.Transaction = this.kernel.Connection.BeginTransaction();
                }
            }
        }

        public virtual void Commit()
        {
            this.kernel.Transaction.Commit();
        }

        public virtual void Rollback()
        {
            this.kernel.Transaction.Rollback();
        }

        protected override void BeginClose()
        {
            if (this.kernel.Transaction != null && this.kernel.Transaction.Connection != null)
            {
                this.kernel.Transaction.Rollback();
            }

            this.kernel.Transaction.Dispose();
            this.kernel.Transaction = null;

            base.BeginClose();
        }


        public DbTransaction Transaction
        {
            get { return this.kernel.Transaction; }
        }
    }
}
