using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class TokenList<T> : IEnumerable<T>, IEnumerable where T : SqlToken
    {
        public SqlToken Parent { get; protected set; }
        protected List<T> _list;

        public TokenList(SqlToken parent)
        {
            Parent = parent;
            _list = new List<T>();
        }
        
        public int Count
        {
            get { return _list.Count; }
        }

        public void Replace(IEnumerable<T> items)
        {
            Clear();
            foreach (var item in items)
            {
                item.ParentToken = Parent;
            }
            _list.AddRange(items);
        }

        public void Add(T item)
        {
            item.ParentToken = Parent;
            _list.Add(item);
        }

        public void Insert(int index, T item)
        {
            item.ParentToken = Parent;
            _list.Insert(index, item);
        }

        public T this[int index]
        {
            get { return _list[index]; }
            set
            {
                value.ParentToken = Parent;
                _list[index] = value;
            }
        }

        public void Clear()
        {
            _list.Clear();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public void Remove(T item)
        {
            _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public void ForEach(Action<T> action)
        {
            _list.ForEach(action);
        }
    }
}
