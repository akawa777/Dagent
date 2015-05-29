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
    public interface IConfig
    {
        int CommandTimeout { get; set; }
    }

    internal class Config : IConfig
    {
        public Config(IDagentKernel kernel)
        {
            _kernel = kernel;
        }

        private IDagentKernel _kernel;

        public int CommandTimeout
        {
            get
            {
                return _kernel.CommandTimeout;
            }
            set
            {
                _kernel.CommandTimeout = value;
            }
        }
    }
}
