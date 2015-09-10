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
        protected bool isAlreadyBeginTransaction = false;
        protected bool isEndTransaction = false;
        protected bool isCompleted = false;

        protected override void BeginOpen()
        {
            base.BeginOpen();

            if (this.kernel.Transaction == null)
            {                
                if (settedIsolationLevel)
                {
                    this.kernel.Transaction = this.kernel.Connection.BeginTransaction(isolationLevel);
                }
                else
                {
                    this.kernel.Transaction = this.kernel.Connection.BeginTransaction();                          
                }

                this.kernel.Rollbakced = false;
            }
            else
            {
                isAlreadyBeginTransaction = true;
            }
        }

        public virtual void Complete()
        {
            isCompleted = true;                
        }

        protected override void BeginClose()
        {
            if (isCompleted && !this.kernel.Rollbakced && !isAlreadyBeginTransaction)
            {
                this.kernel.Transaction.Commit();
                this.kernel.Transaction.Dispose();
                this.kernel.Transaction = null;                
            }
            else if (!isCompleted || this.kernel.Rollbakced)
            {   
                this.kernel.Transaction.Rollback();
                this.kernel.Rollbakced = true;
            }

            base.BeginClose();
        }

        public DbTransaction Transaction
        {
            get { return this.kernel.Transaction; }
        }
    }
}
