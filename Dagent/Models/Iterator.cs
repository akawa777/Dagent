using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dagent.Models
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
                else
                {
                    _enumerator.Dispose();
                }

                return canMove;
            }
        }

        public void Reset()
        {
            _count = -1;
        }
    }
}
