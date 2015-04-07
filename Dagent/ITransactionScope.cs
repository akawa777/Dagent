using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace Dagent
{   
    public interface ITransactionScope : IConnectionScope
    {
        void Commit();
        void Rollback();
    }
}
