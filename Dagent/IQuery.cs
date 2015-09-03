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
    //internal class Enumerable<T> : IEnumerable<T>
    //{
    //    public Enumerable(Func<IEnumerable<T>> getEnumerable)
    //    {
    //        _enumerator = new Enumerator<T>(getEnumerable);
    //    }

    //    private IEnumerator<T> _enumerator;

    //    public IEnumerator<T> GetEnumerator()
    //    {
    //        return _enumerator;
    //    }

    //    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    //    {
    //        return _enumerator;
    //    }
    //}

    //internal class Enumerator<T> : IEnumerator<T>
    //{
    //    public Enumerator(Func<IEnumerable<T>> getEnumerable)
    //    {
    //        _getEnumerable = getEnumerable;
    //        _enumerator = _getEnumerable().GetEnumerator();
    //    }

    //    private IEnumerator<T> _enumerator;
    //    private Func<IEnumerable<T>> _getEnumerable;
    //    private List<T> _list = new List<T>();
    //    private int _count = -1;
    //    private bool _completed = false;

    //    public T Current
    //    {
    //        get 
    //        {
    //            if (_count < _list.Count || _completed)
    //            {
    //                return _list[_count];
    //            }
    //            else
    //            {
    //                var current = _enumerator.Current;
    //                _list.Add(current);
    //                return current;
    //            }
    //        }
    //    }

    //    public void Dispose()
    //    {
    //        _enumerator.Dispose();

    //        if (!_completed)
    //        {
    //            _enumerator = _getEnumerable().GetEnumerator();
    //        }

    //        _count = -1;
    //    }

    //    object System.Collections.IEnumerator.Current
    //    {
    //        get { return this.Current; }
    //    }

    //    public bool MoveNext()
    //    {
    //        _count++;

    //        if (_count < _list.Count)
    //        {
    //            if (_completed)
    //            {                    
    //                return true;
    //            }
    //            else
    //            {
    //                _enumerator.MoveNext();                    
    //                return true;
    //            }
    //        }
    //        else
    //        {
    //            if (_completed)
    //            {
    //                _count = -1;
    //                return false;
    //            }

    //            bool canMove = _enumerator.MoveNext();
    //            object current = Current;

    //            if (!canMove)
    //            {
    //                _count = -1;
    //                _completed = true;
    //            }

    //            return canMove;
    //        }
    //    }

    //    public void Reset()
    //    {
    //        _enumerator.Dispose();

    //        if (!_completed)
    //        {
    //            _enumerator = _getEnumerable().GetEnumerator();
    //        }

    //        _count = -1;
    //    }
    //}

    public interface IQuery<T> where T : class
    {
        T Single();
        List<T> List();        
        List<T> Page(int pageNo, int noPerPage, out int count);
        IEnumerable<T> Iterator();

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
