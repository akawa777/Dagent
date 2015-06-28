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
    public interface IRowPropertyMapper
    {
        IRowPropertyMapper<T, P> Map<T, P>(T model, Expression<Func<T, P>> targetPropertyExpression, params string[] validColumnNames) 
            where T : class
            where P : class;

        IRowPropertyMapper<T, P> Map<T, P>(T model, Expression<Func<T, List<P>>> targetListPropertyExpression, params string[] validColumnNames)
            where T : class
            where P : class;
    }

    public interface IRowPropertyMapper<T, P> 
        where T : class 
        where P : class
    {
        IRowPropertyMapper<T, P> Create(Func<P> create);
        IRowPropertyMapper<T, P> Unique(params string[] uniqueColumnNames);
        IRowPropertyMapper<T, P> Each(Action<P> mapAction);
        IRowPropertyMapper<T, P> Prefix(string prefixColumnName);
        IRowPropertyMapper<T, P> Auto(bool autoMapping);
        IRowPropertyMapper<T, P> Ignore(params Expression<Func<P, object>>[] ignoreProperties);
        IRowPropertyMapper<T, P> IgnoreCase(bool ignore);

        void Do();        
    }
}
