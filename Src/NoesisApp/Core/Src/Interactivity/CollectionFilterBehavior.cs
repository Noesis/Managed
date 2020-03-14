using Noesis;
using System;
using System.Collections;
using System.Collections.ObjectModel;

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

        private void FilterItems()
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

        private void OnFilterRequired()
        {
            FilterItems();
        }

        public FilterPredicate Predicate
        {
            get { return (FilterPredicate)GetValue(PredicateProperty); }
            set { SetValue(PredicateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Predicate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PredicateProperty = DependencyProperty.Register(
            "Predicate", typeof(FilterPredicate), typeof(CollectionFilterBehavior),
            new PropertyMetadata(null, OnPredicateChanged));

        private static void OnPredicateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CollectionFilterBehavior behavior = (CollectionFilterBehavior)d;

            FilterPredicate oldPredicate = (FilterPredicate)e.OldValue;
            behavior.UnregisterPredicate(oldPredicate);

            FilterPredicate newPredicate = (FilterPredicate)e.NewValue;
            behavior.RegisterPredicate(newPredicate);

            behavior.FilterItems();
        }

        private void RegisterPredicate(FilterPredicate predicate)
        {
            if (predicate != null)
            {
                predicate.ItemsSource = ItemsSource;
                predicate.FilterRequired += OnFilterRequired;
            }
        }

        private void UnregisterPredicate(FilterPredicate predicate)
        {
            if (predicate != null)
            {
                predicate.FilterRequired -= OnFilterRequired;
                predicate.ItemsSource = null;
            }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(CollectionFilterBehavior),
            new PropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CollectionFilterBehavior behavior = (CollectionFilterBehavior)d;

            FilterPredicate predicate = behavior.Predicate;
            if (predicate != null)
            {
                predicate.ItemsSource = (IEnumerable)e.NewValue;
            }

            behavior.FilterItems();
        }

        public FilteredCollection FilteredItems
        {
            get { return (FilteredCollection)GetValue(FilteredItemsProperty); }
            private set { SetValue(FilteredItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilteredItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilteredItemsProperty = DependencyProperty.Register(
            "FilteredItems", typeof(FilteredCollection), typeof(CollectionFilterBehavior),
            new PropertyMetadata(null));
    }
}
