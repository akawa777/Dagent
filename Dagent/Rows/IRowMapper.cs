using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Dagent.Library;
using System.Data.Common;
using System.Data;

namespace Dagent.Rows
{  
    public interface IRowMapper
    {
        T Map<T>(string prefix, params Expression<Func<T, object>>[] ignorePropertyExpressions) where T : new();

        T Map<T>(string prefix) where T : new();

        T Map<T>(params Expression<Func<T, object>>[] ignorePropertyExpressions) where T : new();        

        T Map<T>() where T : new();        
    }
}
