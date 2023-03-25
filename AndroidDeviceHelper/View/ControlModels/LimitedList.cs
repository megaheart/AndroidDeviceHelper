using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AndroidDeviceHelper
{
    /// <summary>
    /// The capacity of limited list is limited, 
    /// if add new element when limited list is full, the last element will be removed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LimitedList<T> : IEnumerable<T>, ICollection<T>, IList<T>
    {
        private int _version = Int32.MinValue;
        //private T[] _emptyArray = new T[0];
        protected T[] _items { get; private set; }
        //string name = "";
        //public LimitedList(int capacity, string _name)
        //{
        //    _items = new T[capacity];
        //    Capacity = capacity;
        //    Count = 0;
        //    this.name = _name;
        //}
        public LimitedList(int capacity)
        {
            _items = new T[capacity];
            Capacity = capacity;
            Count = 0;
        }
        public LimitedList(IEnumerable<T> collection, int capacity)
        {
            _items = new T[capacity];
            Capacity = capacity;
            Count = 0;
            if (collection == null) throw new Exception("\"collection\" parameter must be different from null.");
            ICollection<T> c = collection as ICollection<T>;
            if (c != null)
            {
                if (capacity < c.Count) throw new Exception("Capacity of this LimitedList is not enough to contain all elements of \"collection\" parameter.");
                c.CopyTo(_items, 0);
                Count = Math.Min(c.Count, capacity);
            }
            else
            {
                using (IEnumerator<T> en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        Add(en.Current);
                    }
                }
            }
        }
        public LimitedList(IEnumerable<T> collection)
        {
            Count = 0;
            if (collection == null) throw new Exception("\"collection\" parameter must be different from null.");
            ICollection<T> c = collection as ICollection<T>;
            if (c != null)
            {
                _items = new T[c.Count];
                Capacity = c.Count;
                Count = c.Count;
                c.CopyTo(_items, 0);
            }
            else
            {
                c = new List<T>(collection);
                _items = new T[c.Count];
                Capacity = c.Count;
                Count = c.Count;
                c.CopyTo(_items, 0);
            }
        }
        public T this[int index]
        {
            get
            {
                if (index >= Count || index < 0)
                {
                    if (Count == 0) throw new IndexOutOfRangeException("Empty list doesn't have any element to get.");
                    throw new IndexOutOfRangeException(String.Format("Index = {0} is out of range [0 -> {1}]. List's count is {2}.", index, Count - 1, Count));
                }
                return _items[index];
            }
            set => SetItem(index, value);
        }

        public int Count { get; private set; }
        public int Capacity { get; private set; }
        public bool IsReadOnly => false;
        public virtual void SetItem(int index, T value)
        {
            if (index >= Count || index < 0)
            {
                if (Count == 0) throw new IndexOutOfRangeException("Empty list doesn't have any element to set.");
                throw new IndexOutOfRangeException(String.Format("Index = {0} is out of range [0 -> {1}]. List's count is {2}.", index, Count - 1, Count));
            }
            if (!_items[index].Equals(value))
            {
                _items[index] = value;
                _version++;
            }
        }

        public void Add(T item)
        {
            Insert(Count, item);
        }

        public virtual void Clear()
        {
            if (Count == 0) return;
            Array.Clear(_items, 0, _items.Length);
            Count = 0;
            _version++;
        }

        public bool Contains(T item)
        {
            var index = Array.IndexOf(_items, item);
            return index > -1 && index < Count;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_items, 0, array, arrayIndex, _items.Length);
        }
        public class Enumerator : IEnumerator<T>
        {
            private LimitedList<T> list;
            private int index;
            private int version;
            private T current;
            internal Enumerator(LimitedList<T> list)
            {
                this.list = list;
                index = 0;
                version = list._version;
                current = default(T);
            }
            public T Current
            {
                get
                {
                    return current;
                }
            }
            object System.Collections.IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                LimitedList<T> localList = list;
                if (version == localList._version && index < localList.Count)
                {
                    current = localList._items[index];
                    index++;
                    return true;
                }
                return MoveNextRare();
            }

            public void Reset()
            {
                if (version != this.list._version)
                {
                    throw new Exception("IEnumerator has wrong version.");
                }
                index = 0;
                current = default(T);
            }

            private bool MoveNextRare()
            {
                if (version != this.list._version)
                {
                    throw new Exception("IEnumerator has wrong version.");
                }
                index = this.list.Count + 1;
                current = default(T);
                return false;
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        public int IndexOf(T item)
        {
            var index = Array.IndexOf(_items, item);
            return index < Count ? index : -1;
        }

        public virtual void Insert(int index, T item)
        {
            if (index > Count) throw new ArgumentOutOfRangeException("\"index\" paramater cannot be more than LimitedList.Count");
            if (index == Count && Count == Capacity) return;
            Array.Copy(_items, index, _items, index + 1, _items.Length - index - 1);
            _items[index] = item;
            _version++;
            if (Count < _items.Length) Count++;
            GC.Collect();
        }

        public bool Remove(T item)
        {
            var index = Array.IndexOf(_items, item);
            if (index > -1 && index < Count)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public virtual void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                if(Count == 0)
                    throw new ArgumentOutOfRangeException("This list doesn't have any elements to be removed.");
                throw new ArgumentOutOfRangeException(
                    String.Format("\"index\" = {2} is out of range [0...{0}]. (This LimitedList).Count = {1}.", Count - 1, Count, index));
            }    
            Array.Copy(_items, index + 1, _items, index, _items.Length - index - 1);
            Count--;
            _version++;
            GC.Collect();
        }

        System.Collections.IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}
