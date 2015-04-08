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
        T Map<T>(string prefix, params Expression<Func<T, object>>[] ignorePropertyExpressions) where T : class, new();

        T Map<T>(string prefix) where T : class, new();

        T Map<T>(params Expression<Func<T, object>>[] ignorePropertyExpressions) where T : class, new();

        T Map<T>() where T : class, new();
    }

    public interface IRowPropertyMapDefine
    {
        IRowPropertyMapper<T, P> Map<T, P>(T model, Expression<Func<T, P>> targetPropertyExpression) 
            where T : class, new() 
            where P : class, new();

        IRowPropertyMapper<T, P> MapList<T, P>(T model, Expression<Func<T, List<P>>> targetPropertyExpression, params string[] uniqueColumnNames)
            where T : class, new()
            where P : class, new();
        
    }

    public interface IRowPropertyMapper<T, P> where T : class, new() where P : class, new()
    {
        void Do(string validColumnName, string prefix, params Expression<Func<P, object>>[] ignorePropertyExpressions);

        void Do(string validColumnName, string prefix);

        void Do(string validColumnName, params Expression<Func<P, object>>[] ignorePropertyExpressions);

        void Do(string validColumnName);

        void Do();

        IRowPropertyMapperCallback<T, P> To(string validColumnName, string prefix, params Expression<Func<P, object>>[] ignorePropertyExpressions);

        IRowPropertyMapperCallback<T, P> To(string validColumnName, string prefix);

        IRowPropertyMapperCallback<T, P> To(string validColumnName, params Expression<Func<P, object>>[] ignorePropertyExpressions);

        IRowPropertyMapperCallback<T, P> To(string validColumnName);

        IRowPropertyMapperCallback<T, P> To();         
    }

    public interface IRowPropertyMapperCallback<T, P>
    {
        void Callback(Action<P> callback);
    }
}
