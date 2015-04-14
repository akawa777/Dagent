using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Dagent.Rows;

namespace Dagent
{
    public interface ICommand<T> where T : class, new()
    {
        int Insert(T entity);
        int Update(T entity);
        int Delete(T entity);        
        
        ICommand<T> Map(Action<IUpdateRow, T> mapAction);
        ICommand<T> Auto(bool autoMapping);                

        ICommand<T> Config(Action<IConfig> setConfigAction);
    }
}
