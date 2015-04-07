using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Dagent.Library;
using System.Reflection;
using Dagent.Models;
using Dagent.Rows;

namespace Dagent
{
    public interface IQuery<T> where T : class, new()
    {
        T Single();
        List<T> Fetch();        
        List<T> Page(int pageNo, int noPerPage, out int count);        
        int Count();
        V Scalar<V>();

        IQuery<T> Unique(params string[] columnNames);
        IQuery<T> Parameters(params Parameter[] parameters);
        IQuery<T> Parameters(object parameters);
        IQuery<T> ForEach(Action<T, ICurrentRow, IMappingState<T>> mapAction);
        IQuery<T> AutoMapping(bool autoMapping);
        IQuery<T> IgnoreProperties(params Expression<Func<T, object>>[] ignorePropertyExpressions);
    }
}
