using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Event handler type for SortRequired event
    /// </summary>
    public delegate void SortRequiredEventHandler();

    /// <summary>
    /// Base class for Comparer object used by CollectionSortBehavior. It compares 2 items.
    /// </summary>
    public abstract class SortComparer
    {
        /// <summary>
        /// Compares two objects indicating whether one is less than, equal to, or greater than the other
        /// </summary>
        public abstract int Compare(object i0, object i1);

        /// <summary>
        /// Indicates if sort should be re-evaluated after an item property change
        /// </summary>
        public abstract bool NeedsRefresh(object item, string propertyName);

        /// <summary>
        /// Triggers a new sort of the list
        /// </summary>
        public void Refresh()
        {
            SortRequiredEventHandler handler = SortRequired;
            if (handler != null)
            {
                handler();
            }
        }

        /// <summary>
        /// Gets or sets the source collection of items
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                if (_itemsSource != value)
                {
                    UnregisterItemsSource(_itemsSource);
                    _itemsSource = value;
                    RegisterItemsSource(_itemsSource);
                }
            }
        }

        /// <summary>
        /// Indicates that conditions changed and sort needs to be re-evaluated
        /// </summary>
        public event SortRequiredEventHandler SortRequired;

        #region Private members
        private void RegisterItemsSource(IEnumerable source)
        {
            INotifyCollectionChanged notify = source as INotifyCollectionChanged;
            if (notify != null)
            {
                notify.CollectionChanged += OnItemsChanged;
            }

            RegisterItems(_itemsSource);
        }

        private void UnregisterItemsSource(IEnumerable source)
        {
            UnregisterItems(_itemsSource);

            INotifyCollectionChanged notify = source as INotifyCollectionChanged;
            if (notify != null)
            {
                notify.CollectionChanged -= OnItemsChanged;
            }
        }

        private void RegisterItems(IEnumerable source)
        {
            if (source != null)
            {
                foreach (object item in source)
                {
                    RegisterItem(item);
                }
            }
        }

        private void UnregisterItems(IEnumerable source)
        {
            if (source != null)
            {
                foreach (object item in source)
                {
                    UnregisterItem(item);
                }
            }
        }

        private void RegisterItem(object item)
        {
            INotifyPropertyChanged notify = item as INotifyPropertyChanged;
            if (notify != null)
            {
                notify.PropertyChanged += OnItemChanged;
            }
        }

        private void UnregisterItem(object item)
        {
            INotifyPropertyChanged notify = item as INotifyPropertyChanged;
            if (notify != null)
            {
                notify.PropertyChanged -= OnItemChanged;
            }
        }

        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    RegisterItem(e.NewItems[0]);
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    UnregisterItem(e.OldItems[0]);
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    UnregisterItem(e.OldItems[0]);
                    RegisterItem(e.NewItems[0]);
                    break;
                }
                case NotifyCollectionChangedAction.Move:
                {
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                {
                    RegisterItems(_itemsSource);
                    break;
                }
            }
        }

        private void OnItemChanged(object item, PropertyChangedEventArgs e)
        {
            if (NeedsRefresh(item, e.PropertyName))
            {
                Refresh();
            }
        }

        private IEnumerable _itemsSource;
        #endregion
    }
}
