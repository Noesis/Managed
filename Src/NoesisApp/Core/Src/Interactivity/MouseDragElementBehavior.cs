using Noesis;
using System;

namespace NoesisApp
{
    public delegate void MouseEventHandler(object sender, MouseEventArgs e);

    /// <summary>
    /// An action that will launch a process to open a file or uri.
    /// </summary>
    public class MouseDragElementBehavior : Behavior<FrameworkElement>
    {
        public new MouseDragElementBehavior Clone()
        {
            return (MouseDragElementBehavior)base.Clone();
        }

        public new MouseDragElementBehavior CloneCurrentValue()
        {
            return (MouseDragElementBehavior)base.CloneCurrentValue();
        }

        /// <summary>
        /// Occurs when a drag gesture is initiated
        /// </summary>
        public event MouseEventHandler DragBegun;

        /// <summary>
        /// Occurs when a drag gesture update is processed
        /// </summary>
        public event MouseEventHandler Dragging;

        /// <summary>
        /// Occurs when a drag gesture is finished
        /// </summary>
        public event MouseEventHandler DragFinished;

        /// <summary>
        /// Gets or sets the X position of the dragged element, relative to the left of the root element
        /// </summary>
        public float X
        {
            get { return (float)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        public static readonly DependencyProperty XProperty = DependencyProperty.Register(
            "X", typeof(float), typeof(MouseDragElementBehavior),
            new PropertyMetadata(float.NaN, OnXChanged));

        private static void OnXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MouseDragElementBehavior behavior = (MouseDragElementBehavior)d;
            behavior.UpdatePosition((float)e.NewValue, behavior.Y);
        }

        /// <summary>
        /// Gets or sets the Y position of the dragged element, relative to the top of the root element
        /// </summary>
        public float Y
        {
            get { return (float)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        public static readonly DependencyProperty YProperty = DependencyProperty.Register(
            "Y", typeof(float), typeof(MouseDragElementBehavior),
            new PropertyMetadata(float.NaN, OnYChanged));

        private static void OnYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MouseDragElementBehavior behavior = (MouseDragElementBehavior)d;
            behavior.UpdatePosition(behavior.X, (float)e.NewValue);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the dragged element is constrained to stay within
        /// the bounds of its Parent container
        /// </summary>
        public bool ConstrainToParentBounds
        {
            get { return (bool)GetValue(ConstrainToParentBoundsProperty); }
            set { SetValue(ConstrainToParentBoundsProperty, value); }
        }

        public static readonly DependencyProperty ConstrainToParentBoundsProperty = DependencyProperty.Register(
            "ConstrainToParentBounds", typeof(bool), typeof(MouseDragElementBehavior),
            new PropertyMetadata(false, OnConstrainToParentBoundsChanged));

        private static void OnConstrainToParentBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MouseDragElementBehavior behavior = (MouseDragElementBehavior)d;
            behavior.UpdatePosition(behavior.X, behavior.Y);
        }

        protected override void OnAttached()
        {
            FrameworkElement associatedObject = AssociatedObject;

            _transform = new TranslateTransform();
            associatedObject.RenderTransform = _transform;
            associatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            _transform = null;

            FrameworkElement associatedObject = AssociatedObject;
            if (associatedObject != null)
            {
                associatedObject.RenderTransform = null;
                associatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            StartDrag(e.GetPosition(AssociatedObject));
            if (DragBegun != null)
            {
                DragBegun(this, e);
            }
            e.Handled = true;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AssociatedObject.ReleaseMouseCapture();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            Drag(e.GetPosition(AssociatedObject));
            if (Dragging != null)
            {
                Dragging(this, e);
            }
            e.Handled = true;
        }

        private void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            EndDrag();
            if (DragFinished != null)
            {
                DragFinished(this, e);
            }
            e.Handled = true;
        }

        private void StartDrag(Point relativePosition)
        {
            _relativePosition = relativePosition;

            FrameworkElement associatedObject = AssociatedObject;
            associatedObject.MouseMove += OnMouseMove;
            associatedObject.LostMouseCapture += OnLostMouseCapture;
            associatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;

            associatedObject.CaptureMouse();
        }

        private void Drag(Point relativePosition)
        {
            Vector delta = relativePosition - _relativePosition;
            UpdateTransform(delta.X, delta.Y);

            _settingPosition = true;
            UpdatePosition();
            _settingPosition = false;
        }

        private void EndDrag()
        {
            FrameworkElement associatedObject = AssociatedObject;
            associatedObject.MouseMove -= OnMouseMove;
            associatedObject.LostMouseCapture -= OnLostMouseCapture;
            associatedObject.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        }

        private void UpdatePosition(float x, float y)
        {
            FrameworkElement associatedObject = AssociatedObject;
            if (!_settingPosition && associatedObject != null)
            {
                Point pos = new Point(float.IsNaN(x) ? 0.0f : x, float.IsNaN(y) ? 0.0f : y);
                Point delta = associatedObject.PointFromScreen(pos);
                UpdateTransform(delta.X, delta.Y);
            }
        }

        private void UpdatePosition()
        {
            FrameworkElement associatedObject = AssociatedObject;
            Visual root = (Visual)VisualTreeHelper.GetRoot(associatedObject);
            Matrix4 m = associatedObject.TransformToAncestor(root);
            X = m[3][0];
            Y = m[3][1];
        }

        private void UpdateTransform(float x, float y)
        {
            if (ConstrainToParentBounds)
            {
                FrameworkElement associatedObject = AssociatedObject;
                FrameworkElement parent = associatedObject.Parent;
                if (parent != null)
                {
                    Point minXY = new Point(0.0f, 0.0f);
                    Point maxXY = new Point(parent.ActualWidth, parent.ActualHeight);

                    Matrix4 m = associatedObject.TransformToAncestor(parent);
                    Vector v0 = new Vector(m[3][0] + x, m[3][1] + y);
                    Vector v1 = new Vector(associatedObject.ActualWidth, associatedObject.ActualHeight);
                    v1 += v0;

                    if (v1.X > maxXY.X)
                    {
                        float dif = v1.X - maxXY.X;
                        x -= dif;
                        v0.X -= dif;
                        v1.X -= dif;
                    }
                    if (v0.X < minXY.X)
                    {
                        float dif = minXY.X - v0.X;
                        x += dif;
                    }
                    if (v1.Y > maxXY.Y)
                    {
                        float dif = v1.Y - maxXY.Y;
                        y -= dif;
                        v0.Y -= dif;
                        v1.Y -= dif;
                    }
                    if (v0.Y < minXY.Y)
                    {
                        float dif = minXY.Y - v0.Y;
                        y += dif;
                    }
                }
            }

            _transform.X += x;
            _transform.Y += y;
        }

        private TranslateTransform _transform = null;
        private Point _relativePosition = new Point(0.0f, 0.0f);
        private bool _settingPosition = false;
    }
}
