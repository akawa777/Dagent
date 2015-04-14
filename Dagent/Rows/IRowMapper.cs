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
    public interface IRowModelMapper
    {
        T Map<T>(string[] validColumnNames, string prefixColumnName) where T : class, new();
    }

    public interface IRowPropertyMapDefine
    {
        IRowPropertyMapper<T, P> Map<T, P>(T model, Expression<Func<T, P>> targetPropertyExpression, params string[] validColumnNames) 
            where T : class, new() 
            where P : class, new();

        IRowPropertyMapper<T, P> Map<T, P>(T model, Expression<Func<T, List<P>>> targetListPropertyExpression, params string[] validColumnNames)
            where T : class, new()
            where P : class, new();
    }

    public interface IRowPropertyMapper<T, P> where T : class, new() where P : class, new()
    {
        IRowPropertyMapper<T, P> Unique(params string[] uniqueColumnNames);
        IRowPropertyMapper<T, P> Each(Action<P> mapAction);
        IRowPropertyMapper<T, P> Prefix(string prefixColumnName);
        IRowPropertyMapper<T, P> Auto(bool autoMapping);        

        void Do();        
    }
}
