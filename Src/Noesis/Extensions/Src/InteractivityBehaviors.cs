////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Microsoft.Xaml.Behaviors;

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

    #region Background Effect Behavior
    /// <summary>
    /// Applies an effect to the contents beneath the associated object.
    ///
    /// This behavior can be attached to a Panel, Border or Shape element to fill its background with
    /// the contents of the specified source element that are just beneath, and post-processed with the
    /// desired effect.
    ///
    /// It is normally used to blur the background that is just below a panel.
    ///
    /// Usage:
    ///
    ///    <Grid 
    ///      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///      xmlns:b="http://schemas.microsoft.com/xaml/behaviors"
    ///      xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=NoesisGUI.GUI.Extensions">
    ///      <Image x:Name="img" Source="Images/landscape.jpg" Stretch="UniformToFill"/>
    ///      <Ellipse Width="600" Height="200">
    ///        <b:Interaction.Behaviors>
    ///          <noesis:BackgroundEffectBehavior Source="{Binding ElementName=img}">
    ///            <BlurEffect Radius="20"/>
    ///          </noesis:BackgroundEffectBehavior>
    ///        </b:Interaction.Behaviors>
    ///      </Ellipse>
    ///    </Grid>
    /// </summary>
    [ContentProperty("Effect")]
    public class BackgroundEffectBehavior : Behavior<FrameworkElement>
    {
        #region Source property
        public UIElement Source
        {
            get { return (UIElement)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(UIElement), typeof(BackgroundEffectBehavior), new PropertyMetadata(null));
        #endregion

        #region Effect property
        public Effect Effect
        {
            get { return (Effect)GetValue(EffectProperty); }
            set { SetValue(EffectProperty, value); }
        }

        public static readonly DependencyProperty EffectProperty = DependencyProperty.Register(
            "Effect", typeof(Effect), typeof(BackgroundEffectBehavior), new PropertyMetadata(null));
        #endregion

        #region Behavior methods
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += OnElementLoaded;
            AssociatedObject.Unloaded += OnElementUnloaded;
            AssociatedObject.LayoutUpdated += OnElementUpdated;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnElementLoaded;
            AssociatedObject.Unloaded -= OnElementUnloaded;
            AssociatedObject.LayoutUpdated -= OnElementUpdated;
        }

        private void OnElementLoaded(object sender, RoutedEventArgs e)
        {
            DependencyProperty targetProperty = null;
            if (AssociatedObject is Border)
            {
                targetProperty = Border.BackgroundProperty;
            }
            else if (AssociatedObject is Panel)
            {
                targetProperty = Panel.BackgroundProperty;
            }
            else if (AssociatedObject is Shape)
            {
                targetProperty = Shape.FillProperty;
            }

            if (targetProperty != null)
            {
                VisualBrush brush = new VisualBrush();
                AssociatedObject.SetValue(targetProperty, brush);

                _adorner = new EffectAdorner(AssociatedObject, this, brush);

                AdornerLayer adorners = AdornerLayer.GetAdornerLayer(AssociatedObject);
                adorners.Add(_adorner);
            }
        }

        private void OnElementUnloaded(object sender, RoutedEventArgs e)
        {
            if (_adorner != null)
            {
                AdornerLayer adorners = AdornerLayer.GetAdornerLayer(AssociatedObject);
                adorners.Remove(_adorner);

                _adorner = null;
            }
        }

        private void OnElementUpdated(object sender, EventArgs e)
        {
            if (_adorner != null)
            {
                AdornerLayer adorners = AdornerLayer.GetAdornerLayer(AssociatedObject);
                adorners.Update(AssociatedObject);
            }
        }

        private EffectAdorner _adorner;
        #endregion

        #region Adorner
        private class EffectAdorner : Adorner
        {
            public EffectAdorner(FrameworkElement adornedElement, BackgroundEffectBehavior behavior,
                VisualBrush targetBrush) : base(adornedElement)
            {
                _targetBrush = targetBrush;
                _desiredTransform = new MatrixTransform();

                VisualBrush brush = new VisualBrush { ViewboxUnits = BrushMappingMode.Absolute };
                BindingOperations.SetBinding(brush, VisualBrush.VisualProperty,
                    new Binding("Source") { Source = behavior });

                Border border = new Border { Background = brush };
                BindingOperations.SetBinding(border, UIElement.EffectProperty,
                    new Binding("Effect") { Source = behavior });

                _child = border;
                AddVisualChild(_child);

                _targetBrush.Visual = border;
                _targetBrush.ViewboxUnits = BrushMappingMode.Absolute;

                Opacity = 0.0f;
                IsHitTestVisible = false;
            }

            protected override int VisualChildrenCount
            {
                get => 1;
            }

            protected override Visual GetVisualChild(int index)
            {
                return _child;
            }

            protected override Size MeasureOverride(Size constraint)
            {
                UIElement adornedElement = AdornedElement;
                Size adornedSize = adornedElement.RenderSize;
                Size finalSize = adornedSize;

                VisualBrush sourceBrush = (VisualBrush)_child.Background;
                Visual source = sourceBrush.Visual;
                if (source != null)
                {
                    GeneralTransform mtx = adornedElement.TransformToVisual(source);
                    Point offset = mtx.Transform(new Point(0, 0));
                    Rect bounds = mtx.TransformBounds(new Rect(adornedSize));
                    finalSize.Width = bounds.Width;
                    finalSize.Height = bounds.Height;

                    sourceBrush.Viewbox = new Rect(offset, finalSize);
                    _targetBrush.Viewbox = new Rect(finalSize);
                }

                _child.Measure(finalSize);
                return finalSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                finalSize = DesiredSize;
                _child.Arrange(new Rect(finalSize));
                return finalSize;
            }

            public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
            {
                Point offset = transform.Transform(new Point(0, 0));
                _desiredTransform.Matrix = new Matrix(1.0f, 0.0f, 0.0f, 1.0f, offset.X, offset.Y);
                return _desiredTransform;
            }

            private Border _child;
            private VisualBrush _targetBrush;
            private MatrixTransform _desiredTransform;
        }
        #endregion
    }
    #endregion
}
