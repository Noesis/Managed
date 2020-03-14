using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Noesis
{

    public class BoxedFreezableCollection<T> : BaseFreezableCollection, IList<T>, INotifyCollectionChanged where T: struct
    {
        protected BoxedFreezableCollection()
        {
        }

        internal BoxedFreezableCollection(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn)
        {
        }

        internal static HandleRef getCPtr(BoxedFreezableCollection<T> obj)
        {
            return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
        }

        public new T this[int index]
        {
            get { return (T)base[index]; }
            set
            {
                CheckReentrancy();

                object oldValue = base[index];
                object newValue = value;
                if (oldValue != newValue)
                {
                    base[index] = newValue;

                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, newValue, oldValue, index));
                }
            }
        }

        public void Add(T item)
        {
            CheckReentrancy();

            base.Add(item);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, item, Count - 1));
        }

        public new void Clear()
        {
            CheckReentrancy();

            base.Clear();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Reset));
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
            CheckReentrancy();

            base.Insert(index, item);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, item, index));
        }

        public bool Remove(T item)
        {
            int index = base.IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public new void RemoveAt(int index)
        {
            CheckReentrancy();

            T oldValue = this[index];
            base.RemoveAt(index);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove, oldValue, index));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                using (BlockReentrancy())
                {
                    CollectionChanged(this, e);
                }
            }
        }

        #region Reentrancy checks
        private IDisposable BlockReentrancy()
        {
            _monitor.Enter();
            return _monitor;
        }

        private void CheckReentrancy()
        {
            if (_monitor.Busy)
            {
                throw new InvalidOperationException("FreezableCollection reentrant operation");
            }
        }

        private class SimpleMonitor : IDisposable
        {
            private int _busyCount;

            public bool Busy
            {
                get { return this._busyCount > 0; }
            }

            public void Enter()
            {
                _busyCount++;
            }

            public void Dispose()
            {
                _busyCount--;
                GC.SuppressFinalize(this);
            }
        }

        private SimpleMonitor _monitor = new SimpleMonitor();
        #endregion

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
            get { return base.IsReadOnly; }
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

            public Enumerator(BoxedFreezableCollection<T> c)
            {
                this._collection = c;
                this._index = -1;
            }

            private BoxedFreezableCollection<T> _collection;
            private int _index;
        }
        #endregion
    }

}

