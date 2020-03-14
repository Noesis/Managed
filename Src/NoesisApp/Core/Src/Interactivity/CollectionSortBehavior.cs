using Noesis;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace NoesisGUIExtensions
{
    public class SortedCollection : ObservableCollection<object> { };

    /// <summary>
    /// A behavior that sorts a collection depending on the supplied Comparer object.
    /// </summary>
    public class CollectionSortBehavior : NoesisApp.Behavior<Noesis.FrameworkElement>
    {
        public CollectionSortBehavior()
        {
            SortedItems = new SortedCollection();
        }

        public new CollectionSortBehavior Clone()
        {
            return (CollectionSortBehavior)base.Clone();
        }

        public new CollectionSortBehavior CloneCurrentValue()
        {
            return (CollectionSortBehavior)base.CloneCurrentValue();
        }

        private int BinarySearch(SortComparer comparer, SortedCollection list, object item, int low, int high)
        {
            if (comparer == null)
                return high + 1;

            if (high <= low)
                return comparer.Compare(item, list[low]) >= 0 ? (low + 1) : low;

            int mid = (low + high) / 2;
            int comparisson = comparer.Compare(item, list[mid]);

            if (comparisson == 0)
                return mid + 1;

            if (comparisson > 0)
                return BinarySearch(comparer, list, item, mid + 1, high);

            return BinarySearch(comparer, list, item, low, mid - 1);
        }

        private void SortItems()
        {
            IEnumerable source = ItemsSource;
            if (source == null)
            {
                SortedItems.Clear();
                return;
            }

            IEnumerator enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                SortedItems.Clear();
                return;
            }

            SortComparer comparer = Comparer;
            SortedCollection sorted = new SortedCollection();

            // using binary insertion sort algorithm
            int i = 1;
            sorted.Add(enumerator.Current);
            while (enumerator.MoveNext())
            {
                object item = enumerator.Current;
                int insertIndex = BinarySearch(comparer, sorted, item, 0, i - 1);
                sorted.Insert(insertIndex, item);
                ++i;
            }

            SortedItems = sorted;
        }

        private void OnSortRequired()
        {
            SortItems();
        }

        public SortComparer Comparer
        {
            get { return (SortComparer)GetValue(ComparerProperty); }
            set { SetValue(ComparerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Comparer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ComparerProperty = DependencyProperty.Register(
            "Comparer", typeof(SortComparer), typeof(CollectionSortBehavior),
            new PropertyMetadata(null, OnComparerChanged));

        private static void OnComparerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CollectionSortBehavior behavior = (CollectionSortBehavior)d;

            SortComparer oldComparer = (SortComparer)e.OldValue;
            behavior.UnregisterComparer(oldComparer);

            SortComparer newComparer = (SortComparer)e.NewValue;
            behavior.RegisterComparer(newComparer);

            behavior.SortItems();
        }

        private void RegisterComparer(SortComparer comparer)
        {
            if (comparer != null)
            {
                comparer.ItemsSource = ItemsSource;
                comparer.SortRequired += OnSortRequired;
            }
        }

        private void UnregisterComparer(SortComparer comparer)
        {
            if (comparer != null)
            {
                comparer.SortRequired -= OnSortRequired;
                comparer.ItemsSource = null;
            }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(CollectionSortBehavior),
            new PropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CollectionSortBehavior behavior = (CollectionSortBehavior)d;

            IEnumerable oldSource = (IEnumerable)e.OldValue;
            behavior.UnregisterItemsSource(oldSource);

            IEnumerable newSource = (IEnumerable)e.NewValue;
            behavior.RegisterItemsSource(newSource);

            SortComparer comparer = behavior.Comparer;
            if (comparer != null)
            {
                comparer.ItemsSource = newSource;
            }

            behavior.SortItems();
        }

        private void AddSortedItem(SortComparer comparer, SortedCollection list, object item)
        {
            int count = list.Count;
            int insertIndex = count == 0 ? 0 : BinarySearch(comparer, list, item, 0, count - 1);
            list.Insert(insertIndex, item);
        }

        private void RemSortedItem(SortComparer comparer, SortedCollection list, object item)
        {
            int count = list.Count;
            int index = BinarySearch(comparer, list, item, 0, count - 1) - 1;
            list.RemoveAt(index);
        }

        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    AddSortedItem(Comparer, SortedItems, e.NewItems[0]);
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    RemSortedItem(Comparer, SortedItems, e.OldItems[0]);
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    SortComparer comparer = Comparer;
                    SortedCollection sorted = SortedItems;
                    RemSortedItem(comparer, sorted, e.OldItems[0]);
                    AddSortedItem(comparer, sorted, e.NewItems[0]);
                    break;
                }
                case NotifyCollectionChangedAction.Move:
                {
                    // nothing to do
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                {
                    SortItems();
                    break;
                }
            }
        }

        private void RegisterItemsSource(IEnumerable source)
        {
            INotifyCollectionChanged notify = source as INotifyCollectionChanged;
            if (notify != null)
            {
                notify.CollectionChanged += OnItemsChanged;
            }
        }

        private void UnregisterItemsSource(IEnumerable source)
        {
            INotifyCollectionChanged notify = source as INotifyCollectionChanged;
            if (notify != null)
            {
                notify.CollectionChanged -= OnItemsChanged;
            }
        }

        public SortedCollection SortedItems
        {
            get { return (SortedCollection)GetValue(SortedItemsProperty); }
            private set { SetValue(SortedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SortedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SortedItemsProperty = DependencyProperty.Register(
            "SortedItems", typeof(SortedCollection), typeof(CollectionSortBehavior),
            new PropertyMetadata(null));
    }
}
