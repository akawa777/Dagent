using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Dagent.Models;
using Dagent.Kernels;
using Dagent.Library;
using System.Linq.Expressions;

namespace Dagent.Define
{
    public interface IDagentDefine
    {
        string GetConnectionString();
        DbProviderFactory CreateDbProviderFactory();
        void SetConfig(IConfig config);
    }
}
