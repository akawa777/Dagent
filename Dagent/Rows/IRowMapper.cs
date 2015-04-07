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

    public interface IRowPropertyMapper
    {
        void Map<T, P>(T model, Expression<Func<T, P>> targetPropertyExpression, string prefix, params Expression<Func<P, object>>[] ignorePropertyExpressions) 
            where T : class, new() 
            where P : class, new();

        void Map<T, P>(T model, Expression<Func<T, P>> targetPropertyExpression, string prefix)
            where T : class, new()
            where P : class, new();

        void Map<T, P>(T model, Expression<Func<T, P>> targetPropertyExpression, params Expression<Func<P, object>>[] ignorePropertyExpressions)
            where T : class, new()
            where P : class, new();

        void Map<T, P>(T model, Expression<Func<T, P>> targetPropertyExpression)
            where T : class, new()
            where P : class, new();

        void Map<T, P>(T model, Expression<Func<T, List<P>>> targetPropertyExpression, string prefix, params Expression<Func<P, object>>[] ignorePropertyExpressions)
            where T : class, new()
            where P : class, new();

        void Map<T, P>(T model, Expression<Func<T, List<P>>> targetPropertyExpression, string prefix)
            where T : class, new()
            where P : class, new();

        void Map<T, P>(T model, Expression<Func<T, List<P>>> targetPropertyExpression, params Expression<Func<P, object>>[] ignorePropertyExpressions)
            where T : class, new()
            where P : class, new();

        void Map<T, P>(T model, Expression<Func<T, List<P>>> targetPropertyExpression)
            where T : class, new()
            where P : class, new();
    }
}
