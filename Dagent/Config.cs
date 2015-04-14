using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dagent.Library;
using System.Linq.Expressions;

namespace Dagent
{    
    public interface IConfig
    {
        IMap<T> Map<T>() where T : class, new();
    }

    internal class Config : IConfig
    {
        public Config(ColumnNamePropertyMap columnNamePropertyMap)
        {
            this.columnNamePropertyMap = columnNamePropertyMap;
        }

        private ColumnNamePropertyMap columnNamePropertyMap;
        private Dictionary<string, object> mapCache = new Dictionary<string, object>();

        public IMap<T> Map<T>() where T : class, new()
        {
            object obj;
            if (mapCache.TryGetValue(typeof(T).FullName, out obj))
            {
                return obj as IMap<T>;
            }

            IMap<T> map = new Map<T>(columnNamePropertyMap);

            mapCache[typeof(T).FullName] = map;

            return map;
        }
    }

    public interface IMap<T> where T : class, new()
    {
        IMap<T> Column<P>(Expression<Func<T, P>> propertyExpression, string columnName);
        IMap<T> Ignore<P>(Expression<Func<T, P>> propertyExpression);
        IMap<T> Clear();
    }

    internal class Map<T> : IMap<T> where T : class, new()
    {
        public Map(ColumnNamePropertyMap columnNamePropertyMap)
        {
            this.columnNamePropertyMap = columnNamePropertyMap;
        }

        private ColumnNamePropertyMap columnNamePropertyMap = new ColumnNamePropertyMap();

        public IMap<T> Column<P>(Expression<Func<T, P>> propertyExpression, string columnName)
        {
            columnNamePropertyMap.Column<T>(columnName, PropertyCache<T>.GetProperty(propertyExpression.Body.ToString().Split('.')[1]));

            return this;
        }

        public IMap<T> Clear()
        {
            columnNamePropertyMap.Clear();
            return this;
        }


        public IMap<T> Ignore<P>(Expression<Func<T, P>> propertyExpression)
        {
            columnNamePropertyMap.Ignore<T>(PropertyCache<T>.GetProperty(propertyExpression.Body.ToString().Split('.')[1]));

            return this;
        }
    }
}
