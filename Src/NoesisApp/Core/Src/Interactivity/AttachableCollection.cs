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
            _associatedObject = null;
            _items = new List<T>();

            CollectionChanged += OnCollectionChanged;
        }

        ~AttachableCollection()
        {
            CollectionChanged -= OnCollectionChanged;
        }

        protected DependencyObject AssociatedObject { get { return _associatedObject; } }

        DependencyObject IAttachedObject.AssociatedObject { get { return this.AssociatedObject; } }

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

                _associatedObject = associatedObject;

                OnAttached();
            }
        }

        public void Detach()
        {
            if (AssociatedObject != null)
            {
                OnDetaching();

                _associatedObject = null;
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
                        _items.Insert(IndexOf(item), (T)item);
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (object item in e.OldItems)
                    {
                        ItemRemoved((T)item);
                        _items.Remove((T)item);
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (object item in e.OldItems)
                    {
                        ItemRemoved((T)item);
                        _items.Remove((T)item);
                    }
                    foreach (object item in e.NewItems)
                    {
                        ItemAdded((T)item);
                        _items.Insert(IndexOf(item), (T)item);
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Move:
                {
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                {
                    foreach (object item in e.OldItems)
                    {
                        ItemRemoved((T)item);
                    }
                    _items.Clear();
                    break;
                }
            }
        }

        DependencyObject _associatedObject;
        List<T> _items;
    }
}
