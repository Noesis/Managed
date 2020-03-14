////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Interactivity;

namespace NoesisGUIExtensions
{
    #region Filter Behavior
    public delegate void FilterRequiredEventHandler();

    public abstract class FilterPredicate
    {
        public abstract bool Matches(object item);
        public abstract bool NeedsRefresh(object item, string propertyName);

        public void Refresh() { FilterRequired?.Invoke(); }

        public event FilterRequiredEventHandler FilterRequired;
    }

    public class FilteredCollection : ObservableCollection<object> { };

    public class CollectionFilterBehavior : Behavior<FrameworkElement>
    {
        public FilterPredicate Predicate
        {
            get { return (FilterPredicate)GetValue(PredicateProperty); }
            set { SetValue(PredicateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Predicate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PredicateProperty = DependencyProperty.Register(
            "Predicate", typeof(FilterPredicate), typeof(CollectionFilterBehavior), new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(CollectionFilterBehavior), new PropertyMetadata(null));

        public FilteredCollection FilteredItems
        {
            get { return (FilteredCollection)GetValue(FilteredItemsProperty); }
            private set { SetValue(FilteredItemsPropertyKey, value); }
        }

        // Using a DependencyProperty as the backing store for FilteredItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyPropertyKey FilteredItemsPropertyKey = DependencyProperty.RegisterReadOnly(
            "FilteredItems", typeof(FilteredCollection), typeof(CollectionFilterBehavior), new PropertyMetadata(null));

        private static readonly DependencyProperty FilteredItemsProperty = FilteredItemsPropertyKey.DependencyProperty;
    }
    #endregion

    #region Sort Behavior
    public delegate void SortRequiredEventHandler();

    public abstract class SortComparer
    {
        public abstract int Compare(object i0, object i1);
        public abstract bool NeedsRefresh(object item, string propertyName);

        public void Refresh() { SortRequired?.Invoke(); }

        public event SortRequiredEventHandler SortRequired;
    }

    public class SortedCollection : ObservableCollection<object> { };

    public class CollectionSortBehavior : Behavior<FrameworkElement>
    {
        public SortComparer Comparer
        {
            get { return (SortComparer)GetValue(ComparerProperty); }
            set { SetValue(ComparerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Comparer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ComparerProperty = DependencyProperty.Register(
            "Comparer", typeof(SortComparer), typeof(CollectionSortBehavior), new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(CollectionSortBehavior), new PropertyMetadata(null));

        public SortedCollection SortedItems
        {
            get { return (SortedCollection)GetValue(SortedItemsProperty); }
            private set { SetValue(SortedItemsPropertyKey, value); }
        }

        // Using a DependencyProperty as the backing store for SortedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyPropertyKey SortedItemsPropertyKey = DependencyProperty.RegisterReadOnly(
            "SortedItems", typeof(SortedCollection), typeof(CollectionSortBehavior), new PropertyMetadata(null));

        private static readonly DependencyProperty SortedItemsProperty = SortedItemsPropertyKey.DependencyProperty;
    }
    #endregion
}
