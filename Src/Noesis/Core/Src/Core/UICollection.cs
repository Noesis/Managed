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

        int ICollection<T>.Count
        {
            get { return base.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(T item)
        {
            base.Add(item);
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
            ((ICollection)this).CopyTo(array, arrayIndex);
        }

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

