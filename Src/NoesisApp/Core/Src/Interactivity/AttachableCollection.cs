using Noesis;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace NoesisApp
{
    /// <summary>
    /// Represents a collection of IAttachedObject with a shared AssociatedObject and provides
    /// change notifications to its contents when that AssociatedObject changes.
    /// </summary>
    public abstract class AttachableCollection<T> : FreezableCollection<T>, IAttachedObject where T: Freezable
    {
        protected AttachableCollection()
        {
            _associatedObject = IntPtr.Zero;
            _items = new List<T>();

            CollectionChanged += OnCollectionChanged;
        }

        protected DependencyObject AssociatedObject
        {
            get { return (DependencyObject)GetProxy(_associatedObject); }
        }

        DependencyObject IAttachedObject.AssociatedObject
        {
            get { return this.AssociatedObject; }
        }

        public void Attach(DependencyObject associatedObject)
        {
            if (AssociatedObject != associatedObject)
            {
                if (AssociatedObject != null)
                {
                    throw new InvalidOperationException(string.Format(
                        "'{0}' already attached to another object '{1}'",
                        GetType(), AssociatedObject.GetType()));
                }

                if (associatedObject == null)
                {
                    throw new InvalidOperationException("Cannot attach to a null object");
                }

                _associatedObject = GetPtr(associatedObject);

                InitObject();

                OnAttached();
            }
        }

        public void Detach()
        {
            if (_associatedObject != IntPtr.Zero)
            {
                OnDetaching();

                _associatedObject = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Called immediately after the collection is attached to an AssociatedObject
        /// </summary>
        protected virtual void OnAttached()
        {
        }

        /// <summary>
        /// Called when the collection is being detached from its AssociatedObject, but before it
        /// has actually occurred
        /// </summary>
        protected virtual void OnDetaching()
        {
        }

        /// <summary>
        /// Called when a new item is added to the collection
        /// </summary>
        protected virtual void ItemAdded(T item)
        {
        }

        /// <summary>
        /// Called when an item is removed from the collection
        /// </summary>
        protected virtual void ItemRemoved(T item)
        {
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (object item in e.NewItems)
                    {
                        ItemAdded((T)item);
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (object item in e.OldItems)
                    {
                        ItemRemoved((T)item);
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (object item in e.OldItems)
                    {
                        ItemRemoved((T)item);
                    }
                    foreach (object item in e.NewItems)
                    {
                        ItemAdded((T)item);
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Move:
                {
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                {
                    foreach (object item in _items)
                    {
                        ItemRemoved((T)item);
                    }
                    break;
                }
            }

            _items.Clear();
            foreach (object item in this)
            {
                _items.Add((T)item);
            }
        }

        IntPtr _associatedObject;
        List<T> _items;
    }
}
