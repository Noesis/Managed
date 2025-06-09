using Noesis;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NoesisGUIExtensions
{
    public class FilteredCollection : ObservableCollection<object> { };

    /// <summary>
    /// A behavior that filter a collection depending on the supplied Predicate object.
    /// </summary>
    public class CollectionFilterBehavior : NoesisApp.Behavior<Noesis.FrameworkElement>
    {
        public CollectionFilterBehavior()
        {
            FilteredItems = new FilteredCollection();
        }

        public new CollectionFilterBehavior Clone()
        {
            return (CollectionFilterBehavior)base.Clone();
        }

        public new CollectionFilterBehavior CloneCurrentValue()
        {
            return (CollectionFilterBehavior)base.CloneCurrentValue();
        }

        #region IsEnabled property
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled", typeof(bool), typeof(CollectionFilterBehavior),
            new PropertyMetadata(true, OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CollectionFilterBehavior behavior = (CollectionFilterBehavior)d;
            behavior.FilterItems();
        }
        #endregion

        #region Predicate property
        public FilterPredicate Predicate
        {
            get { return (FilterPredicate)GetValue(PredicateProperty); }
            set { SetValue(PredicateProperty, value); }
        }

        public static readonly DependencyProperty PredicateProperty = DependencyProperty.Register(
            "Predicate", typeof(FilterPredicate), typeof(CollectionFilterBehavior),
            new PropertyMetadata(null, OnPredicateChanged));

        private static void OnPredicateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CollectionFilterBehavior behavior = (CollectionFilterBehavior)d;
            behavior.UnregisterPredicate((FilterPredicate)e.OldValue);
            behavior.RegisterPredicate((FilterPredicate)e.NewValue);
            behavior.FilterItems();
        }

        private void RegisterPredicate(FilterPredicate predicate)
        {
            if (predicate != null)
            {
                predicate.FilterRequired += OnFilterRequired;
            }
        }

        private void UnregisterPredicate(FilterPredicate predicate)
        {
            if (predicate != null)
            {
                predicate.FilterRequired -= OnFilterRequired;
            }
        }

        private void OnFilterRequired()
        {
            FilterItems();
        }
        #endregion

        #region ItemsSource property
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(CollectionFilterBehavior),
            new PropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CollectionFilterBehavior behavior = (CollectionFilterBehavior)d;
            behavior.UnregisterItemsSource((IEnumerable)e.OldValue);
            behavior.RegisterItemsSource((IEnumerable)e.NewValue);
            behavior.FilterItems();
        }

        private void RegisterItemsSource(IEnumerable source)
        {
            RegisterItems(source);

            INotifyCollectionChanged notify = source as INotifyCollectionChanged;
            if (notify != null)
            {
                notify.CollectionChanged += OnItemsChanged;
            }
        }

        private void UnregisterItemsSource(IEnumerable source)
        {
            UnregisterItems(source);

            INotifyCollectionChanged notify = source as INotifyCollectionChanged;
            if (notify != null)
            {
                notify.CollectionChanged -= OnItemsChanged;
            }
        }

        private void InsertOrdered(IEnumerable list, FilteredCollection filtered,
            object newItem, int newIndex)
        {
            if (list != null)
            {
                var enumerator = list.GetEnumerator();
                bool listEnd = enumerator.MoveNext();

                int numFilteredItems = filtered.Count;
                for (int i = 0, j = 0; i < numFilteredItems; ++i)
                {
                    object filteredItem = filtered[i];

                    object item = null;
                    while (item != filteredItem && j < newIndex && !listEnd)
                    {
                        item = enumerator.Current;
                        listEnd = !enumerator.MoveNext();
                        j++;
                    }

                    if (j >= newIndex)
                    {
                        filtered.Insert(i + (item != filteredItem ? 0 : 1), newItem);
                        break;
                    }
                }

                return;
            }

            filtered.Add(newItem);
        }

        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FilterPredicate predicate = Predicate;
            FilteredCollection filtered = FilteredItems;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    object item = e.NewItems[0];
                    RegisterItem(item);

                    if (IsEnabled)
                    {
                        if (predicate == null || predicate.Matches(item))
                        {
                            InsertOrdered(ItemsSource, filtered, e.NewItems[0], e.NewStartingIndex);
                        }
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    object item = e.OldItems[0];
                    UnregisterItem(item);

                    if (IsEnabled)
                    {
                        filtered.Remove(item);
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    object oldItem = e.OldItems[0];
                    object newItem = e.NewItems[0];
                    UnregisterItem(oldItem);
                    RegisterItem(newItem);

                    if (IsEnabled)
                    {
                        int pos = filtered.IndexOf(oldItem);
                        if (pos != -1)
                        {
                            if (predicate != null && !predicate.Matches(newItem))
                            {
                                filtered.RemoveAt(pos);
                            }
                            else
                            {
                                filtered[pos] = newItem;
                            }
                        }
                        else if (predicate == null || predicate.Matches(newItem))
                        {
                            InsertOrdered(ItemsSource, filtered, e.NewItems[0], e.NewStartingIndex);
                        }
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Move:
                {
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                {
                    RegisterItems(ItemsSource);

                    FilterItems();

                    break;
                }
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

        private void OnItemChanged(object item, PropertyChangedEventArgs e)
        {
            if (IsEnabled)
            {
                FilterPredicate predicate = Predicate;
                if (predicate != null && predicate.NeedsRefresh(item, e.PropertyName))
                {
                    FilteredCollection filtered = FilteredItems;
                    int pos = filtered.IndexOf(item);
                    if (pos != -1)
                    {
                        if (!predicate.Matches(item))
                        {
                            filtered.RemoveAt(pos);
                        }
                    }
                    else
                    {
                        if (predicate.Matches(item))
                        {
                            filtered.Add(item);
                        }
                    }
                }
            }
        }
        #endregion

        #region FilteredItems property
        public FilteredCollection FilteredItems
        {
            get { return (FilteredCollection)GetValue(FilteredItemsProperty); }
            private set { SetValue(FilteredItemsProperty, value); }
        }

        public static readonly DependencyProperty FilteredItemsProperty = DependencyProperty.Register(
            "FilteredItems", typeof(FilteredCollection), typeof(CollectionFilterBehavior),
            new PropertyMetadata(null));
        #endregion

        #region Private members
        protected override void OnDetaching()
        {
            UnregisterPredicate(Predicate);
            UnregisterItemsSource(ItemsSource);
            IsEnabled = false;
        }

        private void FilterItems()
        {
            if (IsEnabled)
            {
                IEnumerable source = ItemsSource;
                if (source == null)
                {
                    FilteredItems.Clear();
                    return;
                }

                IEnumerator enumerator = source.GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    FilteredItems.Clear();
                    return;
                }

                FilterPredicate predicate = Predicate;
                FilteredCollection filtered = new FilteredCollection();

                do
                {
                    object item = enumerator.Current;
                    if (predicate == null || predicate.Matches(item))
                    {
                        filtered.Add(item);
                    }
                }
                while (enumerator.MoveNext());

                FilteredItems = filtered;
            }
        }
        #endregion
    }
}
