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
        protected bool isCommited = false;

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
            }
            else
            {
                isAlreadyBeginTransaction = true;
            }
        }

        public virtual void Complete()
        {
            if (!isAlreadyBeginTransaction)
            {
                isCommited = true;                
            }
        }

        protected override void BeginClose()
        {
            if (!isAlreadyBeginTransaction)
            {
                if (isCommited)
                {
                    this.kernel.Transaction.Commit();                    
                }
                else
                {
                    this.kernel.Transaction.Rollback();
                }

                this.kernel.Transaction.Dispose();
                this.kernel.Transaction = null;
            }            

            base.BeginClose();
        }

        public DbTransaction Transaction
        {
            get { return this.kernel.Transaction; }
        }
    }
}
