using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NoesisGUIExtensions
{
    /// Event handler type for FilterRequired event
    public delegate void FilterRequiredEventHandler();

    /// <summary>
    /// Determines if an item of a list should be filtered out or not
    /// </summary>
    public abstract class FilterPredicate
    {
        /// <summary>
        /// Called to evaluate the predicate against the provided item
        /// </summary>
        public abstract bool Matches(object item);

        /// <summary>
        /// Indicates if filter should be re-evaluated after an item property change
        /// </summary>
        public abstract bool NeedsRefresh(object item, string propertyName);

        /// <summary>
        /// Re-evaluates the filter over the list
        /// </summary>
        public void Refresh()
        {
            FilterRequiredEventHandler handler =  FilterRequired;
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
        /// Event that indicates that conditions changed and sort needs to be re-evaluated
        /// </summary>
        public event FilterRequiredEventHandler FilterRequired;

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
                    Refresh();
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    UnregisterItem(e.OldItems[0]);
                    Refresh();
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    UnregisterItem(e.OldItems[0]);
                    RegisterItem(e.NewItems[0]);
                    Refresh();
                    break;
                }
                case NotifyCollectionChangedAction.Move:
                {
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                {
                    RegisterItems(_itemsSource);
                    Refresh();
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
