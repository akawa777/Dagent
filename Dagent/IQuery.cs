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
        List<T> List();
        List<T> Page(int pageNo, int noPerPage, out int count);        
        int Count();
        V Scalar<V>();

        IQuery<T> Unique(params string[] columnNames);
        IQuery<T> Prefix(string prefixColumnName);
        IQuery<T> Parameters(params Parameter[] parameters);                
        IQuery<T> Parameters(object parameters);        
        IQuery<T> Each(Action<T, ICurrentRow> mapAction);
        IQuery<T> Auto(bool autoMapping);        

        IQuery<T> Config(Action<IConfig> setConfigAction);
    }
}
