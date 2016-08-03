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
    internal class ConnectionScope : IConnectionScope
    {
        public ConnectionScope(IDagentKernel kernel)
        {
            this.kernel = kernel;
            BeginOpen();
        }

        protected virtual void BeginOpen()
        {
            if (this.kernel.Connection.State == ConnectionState.Open)
            {
                isAlreadyOpen = true;                
            }
            else
            {
                this.kernel.Connection.Open();
            }
        }

        protected IDagentKernel kernel;
        protected bool isAlreadyOpen = false;

        public virtual void Dispose()
        {
            BeginClose();            
        }

        protected virtual void BeginClose()
        {
            if (!isAlreadyOpen)
            {
                this.kernel.Connection.Close();
            }
        }
    }
}
