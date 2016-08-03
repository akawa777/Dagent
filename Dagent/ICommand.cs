using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Dagent.Rows;

namespace Dagent
{
    public interface ICommand<T> where T : class
    {
        int Insert(T entity);
        int Update(T entity);
        int Delete(T entity);        
        
        ICommand<T> Map(Action<IUpdateRow, T> mapAction);
        ICommand<T> AutoMapping(bool autoMapping);
        ICommand<T> Where(string where, params Parameter[] parameters);
        ICommand<T> Where(string where, object parameters);

        ICommand<T> Ignore(params Expression<Func<T, object>>[] ignoreProperties);
    }
}
