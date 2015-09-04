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
    internal class Enumerable<T> : IEnumerable<T>
    {
        public Enumerable(IEnumerable<T> enumerable)
        {
            _enumerator = new Enumerator<T>(enumerable);
        }

        private IEnumerator<T> _enumerator;

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerator;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _enumerator;
        }
    }

    internal class Enumerator<T> : IEnumerator<T>
    {
        public Enumerator(IEnumerable<T> enumerable)
        {            
            _enumerator = enumerable.GetEnumerator();
        }

        private IEnumerator<T> _enumerator;
        
        private List<T> _list = new List<T>();
        private int _count = -1;        

        public T Current
        {
            get
            {
                return _list[_count];
            }
        }

        public void Dispose()
        {   
            _count = -1;
        }

        object System.Collections.IEnumerator.Current
        {
            get { return this.Current; }
        }

        public bool MoveNext()
        {
            _count++;

            if (_count < _list.Count)
            {
                return true;
            }
            else
            {
                bool canMove = _enumerator.MoveNext();                

                if (canMove)
                {
                    _list.Add(_enumerator.Current);
                }

                return canMove;
            }
        }

        public void Reset()
        {            
            _count = -1;
        }
    }

    public interface IQuery<T> where T : class
    {
        T Single();
        List<T> List();        
        List<T> Page(int pageNo, int noPerPage, out int count);
        IEnumerable<T> EnumerateList();
        IEnumerable<T> EnumeratePage(int pageNo, int noPerPage, out int count);

        IQuery<T> Create(Func<ICurrentRow, T> create);
        IQuery<T> Unique(params string[] columnNames);
        IQuery<T> Prefix(string prefixColumnName);
        IQuery<T> Parameters(params Parameter[] parameters);                
        IQuery<T> Parameters(object parameters);        
        IQuery<T> Each(Action<T, ICurrentRow> mapAction);
        IQuery<T> AutoMapping(bool autoMapping);        

        IQuery<T> Ignore(params Expression<Func<T, object>>[] ignoreProperties);
        IQuery<T> IgnoreCase(bool ignore);
    }

    public interface IQuery
    {   
        int Count();
        V Scalar<V>();
        void Execute();

        IQuery Each(Action<IBaseRow> mapAction);
        IQuery Unique(params string[] columnNames);        
        IQuery Parameters(params Parameter[] parameters);
        IQuery Parameters(object parameters);        
    }
}
