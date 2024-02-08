using Noesis;
using NoesisApp;
using System.Collections.Generic;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Renders a line decoration on the associated TextBlock.
    ///
    /// Usage:
    ///
    ///   <Grid
    ///     xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///     xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///     xmlns:b="http://schemas.microsoft.com/xaml/behaviors"
    ///     xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=NoesisGUI.GUI.Extensions">
    ///     <TextBlock TextWrapping="Wrap" FontSize="32" Foreground="White" Width="400"
    ///         Text="Some long text that will be rendered with an strikethrough line">
    ///       <b:Interaction.Behaviors>
    ///         <noesis:LineDecorationBehavior Offset="-10" Progress="0.8">
    ///           <noesis:LineDecorationBehavior.Pen>
    ///             <Pen Brush="Red" Thickness="3"/>
    ///           </noesis:LineDecorationBehavior.Pen>
    ///         </noesis:LineDecorationBehavior>
    ///       </b:Interaction.Behaviors>
    ///     </TextBlock>
    ///   </Grid>
    /// </summary>
    public class LineDecorationBehavior : Behavior<TextBlock>
    {
        #region Pen property
        public Pen Pen
        {
            get { return (Pen)GetValue(PenProperty); }
            set { SetValue(PenProperty, value); }
        }

        public static readonly DependencyProperty PenProperty = DependencyProperty.Register(
            "Pen", typeof(Pen), typeof(LineDecorationBehavior), new PropertyMetadata(null));
        #endregion

        #region Offset property
        public float Offset
        {
            get { return (float)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
            "Offset", typeof(float), typeof(LineDecorationBehavior), new PropertyMetadata(0.0f));
        #endregion

        #region Progress property
        public float Progress
        {
            get { return (float)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
            "Progress", typeof(float), typeof(LineDecorationBehavior), new PropertyMetadata(1.0f));
        #endregion

        #region Behavior methods
        protected override void OnAttached()
        {
            TextBlock target = AssociatedObject;
            if (target != null)
            {
                target.Loaded += OnTargetLoaded;
                target.Unloaded += OnTargetUnloaded;
                target.LayoutUpdated += OnTargetUpdated;
            }
        }

        protected override void OnDetaching()
        {
            TextBlock target = AssociatedObject;
            if (target != null)
            {
                target.Loaded -= OnTargetLoaded;
                target.Unloaded -= OnTargetUnloaded;
                target.LayoutUpdated -= OnTargetUpdated;
            }
        }

        private void OnTargetLoaded(object sender, RoutedEventArgs e)
        {
            TextBlock target = AssociatedObject;
            if (target != null)
            {
                AdornerLayer adorners = AdornerLayer.GetAdornerLayer(target);
                _adorner = new LineAdorner(target, this);
                adorners.Add(_adorner);
            }
        }

        private void OnTargetUnloaded(object sender, RoutedEventArgs e)
        {
            TextBlock target = AssociatedObject;
            if (target != null)
            {
                AdornerLayer adorners = AdornerLayer.GetAdornerLayer(target);
                adorners.Remove(_adorner);
                _adorner = null;
            }
        }

        private void OnTargetUpdated(object sender, EventArgs e)
        {
            TextBlock target = AssociatedObject;
            if (_adorner != null && target != null && target.IsVisible && target.Opacity > 0.0f)
            {
                Matrix4 mtx = target.TransformToVisual((Visual)VisualTreeHelper.GetRoot(target));
                if (_adorner.TransformToTarget != mtx)
                {
                    _adorner.TransformToTarget = mtx;

                    AdornerLayer adorners = AdornerLayer.GetAdornerLayer(target);
                    adorners.Update(target);
                }
            }
        }

        private LineAdorner _adorner;
        #endregion

        #region Adorner
        private class LineAdorner : Adorner
        {
            public static readonly DependencyProperty PenProperty = DependencyProperty.Register(
                "Pen", typeof(Pen), typeof(LineAdorner),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

            public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
                "Offset", typeof(float), typeof(LineAdorner),
                new FrameworkPropertyMetadata(0.0f, FrameworkPropertyMetadataOptions.AffectsRender));

            public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
                "Progress", typeof(float), typeof(LineAdorner),
                new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.AffectsRender));

            public LineAdorner(TextBlock adornedElement, LineDecorationBehavior owner) :
                base(adornedElement)
            {
                BindingOperations.SetBinding(this, PenProperty,
                    new Binding(LineDecorationBehavior.PenProperty, owner));

                BindingOperations.SetBinding(this, OffsetProperty,
                    new Binding(LineDecorationBehavior.OffsetProperty, owner));

                BindingOperations.SetBinding(this, ProgressProperty,
                    new Binding(LineDecorationBehavior.ProgressProperty, owner));
            }

            private Matrix4 _mtx = Matrix4.Identity;
            public Matrix4 TransformToTarget
            {
                get => _mtx;
                set { _mtx = value; }
            }

            private struct LineData
            {
                public float x0;
                public float x1;
                public float y;
            };

#if UNITY_5_3_OR_NEWER
            internal
#endif
            protected override void OnRender(DrawingContext context)
            {
                // Check if we are going to render anything
                float progress = (float)GetValue(ProgressProperty);
                if (progress <= 0.0f) return;
                Pen pen = (Pen)GetValue(PenProperty);
                if (pen == null || pen.Brush == null || pen.Thickness == 0.0f) return;

                // Get line render data and calculate the total length in pixels
                List<LineData> lineData = new List<LineData>();

                TextBlock tb = (TextBlock)AdornedElement;
                FormattedText text = tb.FormattedText;

                float totalLength = 0.0f;
                uint glyphIndex = 0;
                uint numLines = text.NumLines;
                for (uint i = 0; i < numLines; ++i)
                {
                    FormattedText.LineInfo line = text.GetLineInfo(i);

                    LineData l = new LineData();
                    text.GetGlyphPosition(glyphIndex, false, ref l.x0, ref l.y);
                    text.GetGlyphPosition(glyphIndex + line.NumGlyphs - 1, true, ref l.x1, ref l.y);
                    totalLength += l.x1 - l.x0;
                    glyphIndex += line.NumGlyphs;
                    lineData.Add(l);
                }

                // Render the lines until we reach the specified length progress
                float offset = (float)GetValue(OffsetProperty);
                float endLength = totalLength * progress;
                float length = 0.0f;
                for (int i = 0; i < numLines; ++i)
                {
                    if (length >= endLength) break;

                    LineData l = lineData[i];
                    float x0 = l.x0;
                    float x1 = System.Math.Min(l.x1, l.x0 + endLength - length);
                    float y = l.y + offset;

                    context.DrawLine(pen, new Point(x0, y), new Point(x1, y));

                    length += x1 - x0;
                }
            }
        }
        #endregion
    }
}
