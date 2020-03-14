using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

namespace Noesis
{

    public class UICollection<T> : BaseUICollection, IList<T>
    {
        protected UICollection()
        {
        }

        internal UICollection(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn)
        {
        }

        internal static HandleRef getCPtr(UICollection<T> obj)
        {
            return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
        }

        public new T this[int index]
        {
            get { return (T)base[index]; }
            set { base[index] = value; }
        }

        public void Add(T item)
        {
            base.Add(item);
        }

        public void AddRange(IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                base.Add(item);
            }
        }

        public bool Contains(T item)
        {
            return base.Contains(item);
        }

        public int IndexOf(T item)
        {
            return base.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            base.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return base.Remove(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        #region IList<T>
        T IList<T>.this[int index]
        {
            get { return this[index]; }
            set { this[index] = value; }
        }

        int IList<T>.IndexOf(T item)
        {
            return this.IndexOf(item);
        }

        void IList<T>.Insert(int index, T item)
        {
            this.Insert(index, item);
        }

        void IList<T>.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }
        #endregion

        #region ICollection<T>
        int ICollection<T>.Count
        {
            get { return base.Count; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        void ICollection<T>.Add(T item)
        {
            this.Add(item);
        }

        void ICollection<T>.Clear()
        {
            this.Clear();
        }

        bool ICollection<T>.Contains(T item)
        {
            return this.Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            return this.Remove(item);
        }
        #endregion

        #region Enumerator
        public new Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public new struct Enumerator : IEnumerator<T>
        {
            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public T Current
            {
                get { return this._collection[this._index]; }
            }

            public bool MoveNext()
            {
                if (++this._index >= this._collection.Count)
                {
                    return false;
                }
                return true;
            }

            public void Reset()
            {
                this._index = -1;
            }

            public void Dispose()
            {
            }

            public Enumerator(UICollection<T> c)
            {
                this._collection = c;
                this._index = -1;
            }

            private UICollection<T> _collection;
            private int _index;
        }
        #endregion
    }

}
