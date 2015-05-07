using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Dagent.Kernels
{
    internal class SqlDagentKernel : DagentKernel
    {
        public SqlDagentKernel(DbProviderFactory providerFactory, DbConnection connection) : base(providerFactory, connection)
        {

        }
    }
}
