using Noesis;
using NoesisApp;

namespace NoesisGUIExtensions
{
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
            FrameworkElement element = AssociatedObject;
            if (element != null)
            {
                element.Loaded += OnElementLoaded;
                element.Unloaded += OnElementUnloaded;
                element.LayoutUpdated += OnElementUpdated;
            }
        }

        protected override void OnDetaching()
        {
            FrameworkElement element = AssociatedObject;
            if (element != null)
            {
                element.Loaded -= OnElementLoaded;
                element.Unloaded -= OnElementUnloaded;
                element.LayoutUpdated -= OnElementUpdated;
            }
        }

        private void OnElementLoaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = AssociatedObject;
            DependencyProperty targetProperty = null;
            if (element is Border)
            {
                targetProperty = Border.BackgroundProperty;
            }
            else if (element is Panel)
            {
                targetProperty = Panel.BackgroundProperty;
            }
            else if (element is Shape)
            {
                targetProperty = Shape.FillProperty;
            }

            if (targetProperty != null)
            {
                VisualBrush brush = new VisualBrush();
                element.SetValue(targetProperty, brush);

                _adorner = new EffectAdorner(element, this, brush);

                AdornerLayer adorners = AdornerLayer.GetAdornerLayer(element);
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

#if UNITY_5_3_OR_NEWER
            internal
#endif
            protected override int VisualChildrenCount
            {
                get => 1;
            }

#if UNITY_5_3_OR_NEWER
            internal
#endif
            protected override Visual GetVisualChild(int index)
            {
                return _child;
            }

#if UNITY_5_3_OR_NEWER
            internal
#endif
            protected override Size MeasureOverride(Size constraint)
            {
                UIElement adornedElement = AdornedElement;
                Size adornedSize = adornedElement.RenderSize;
                Size finalSize = adornedSize;

                VisualBrush sourceBrush = (VisualBrush)_child.Background;
                Visual source = sourceBrush.Visual;
                if (source != null)
                {
                    Matrix4 mtx = adornedElement.TransformToVisual(source);
                    Point offset = mtx[3].XY + new Point(-1.5f, 1.5f);
                    Rect bounds = mtx.TransformBounds(new Rect(adornedSize));
                    finalSize.Width = bounds.Width;
                    finalSize.Height = bounds.Height;

                    sourceBrush.Viewbox = new Rect(offset, finalSize);
                    _targetBrush.Viewbox = new Rect(finalSize);
                }

                _child.Measure(finalSize);
                return finalSize;
            }

#if UNITY_5_3_OR_NEWER
            internal
#endif
            protected override Size ArrangeOverride(Size finalSize)
            {
                finalSize = DesiredSize;
                _child.Arrange(new Rect(finalSize));
                return finalSize;
            }

            public override Matrix4 GetDesiredTransform(Matrix4 transform)
            {
                Matrix4 t = Matrix4.Identity;
                t[3] = new Vector4(transform[3].XY, 0.0f, 1.0f);
                return t;
            }

            private Border _child;
            private VisualBrush _targetBrush;
        }
        #endregion
    }
}
