using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// Moves the attached element in response to mouse drag and touch gestures on the element.
    /// </summary>
    public class TranslateZoomRotateBehavior : Behavior<FrameworkElement>
    {
        public new TranslateZoomRotateBehavior Clone()
        {
            return (TranslateZoomRotateBehavior)base.Clone();
        }

        public new TranslateZoomRotateBehavior CloneCurrentValue()
        {
            return (TranslateZoomRotateBehavior)base.CloneCurrentValue();
        }

        /// <summary>
        /// Gets or sets a value specifying wich zooming and translation variants to support
        /// </summary>
        public ManipulationModes SupportedGestures
        {
            get { return (ManipulationModes)GetValue(SupportedGesturesProperty); }
            set { SetValue(SupportedGesturesProperty, value); }
        }

        public static readonly DependencyProperty SupportedGesturesProperty = DependencyProperty.Register(
            "SupportedGestures", typeof(ManipulationModes), typeof(TranslateZoomRotateBehavior),
            new PropertyMetadata(ManipulationModes.All));

        /// <summary>
        /// Gets or sets a number describing the rate at which the translation will decrease
        /// </summary>
        public float TranslateFriction
        {
            get { return (float)GetValue(TranslateFrictionProperty); }
            set { SetValue(TranslateFrictionProperty, value); }
        }

        public static readonly DependencyProperty TranslateFrictionProperty = DependencyProperty.Register(
            "TranslateFriction", typeof(float), typeof(TranslateZoomRotateBehavior),
            new PropertyMetadata(0.0f));

        /// <summary>
        /// Gets or sets a number describing the rate at which the rotation will decrease
        /// </summary>
        public float RotationalFriction
        {
            get { return (float)GetValue(RotationalFrictionProperty); }
            set { SetValue(RotationalFrictionProperty, value); }
        }

        public static readonly DependencyProperty RotationalFrictionProperty = DependencyProperty.Register(
            "RotationalFriction", typeof(float), typeof(TranslateZoomRotateBehavior),
            new PropertyMetadata(0.0f));

        /// <summary>
        /// Gets or sets the value indicating whether the zoom and translate position of the attached
        /// object is limited by the bounds of the parent object
        /// </summary>
        public bool ConstrainToParentBounds
        {
            get { return (bool)GetValue(ConstrainToParentBoundsProperty); }
            set { SetValue(ConstrainToParentBoundsProperty, value); }
        }

        public static readonly DependencyProperty ConstrainToParentBoundsProperty = DependencyProperty.Register(
            "ConstrainToParentBounds", typeof(bool), typeof(TranslateZoomRotateBehavior),
            new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a number indicating the minimum zoom value allowed
        /// </summary>
        public float MinimumScale
        {
            get { return (float)GetValue(MinimumScaleProperty); }
            set { SetValue(MinimumScaleProperty, value); }
        }

        public static readonly DependencyProperty MinimumScaleProperty = DependencyProperty.Register(
            "MinimumScale", typeof(float), typeof(TranslateZoomRotateBehavior),
            new PropertyMetadata(0.1f));

        /// <summary>
        /// Gets or sets a number indicating the maximum zoom value allowed
        /// </summary>
        public float MaximumScale
        {
            get { return (float)GetValue(MaximumScaleProperty); }
            set { SetValue(MaximumScaleProperty, value); }
        }

        public static readonly DependencyProperty MaximumScaleProperty = DependencyProperty.Register(
            "MaximumScale", typeof(float), typeof(TranslateZoomRotateBehavior),
            new PropertyMetadata(10.0f));

        /// <summary>
        /// Gets or sets the sensitivity factor used to zoom using mouse wheel. Default value is 1.0f.
        /// </summary>
        public float WheelSensitivity
        {
            get { return (float)GetValue(WheelSensitivityProperty); }
            set { SetValue(WheelSensitivityProperty, value); }
        }

        public static readonly DependencyProperty WheelSensitivityProperty = DependencyProperty.Register(
            "WheelSensitivity", typeof(float), typeof(TranslateZoomRotateBehavior),
            new PropertyMetadata(1.0f));

        protected override void OnAttached()
        {
            FrameworkElement associatedObject = AssociatedObject;

            _scale = new ScaleTransform();
            _rotate = new RotateTransform();
            _translate = new TranslateTransform();
            TransformGroup transform = new TransformGroup();
            TransformCollection children = transform.Children;
            children.Add(_scale);
            children.Add(_rotate);
            children.Add(_translate);
            associatedObject.RenderTransform = transform;
            associatedObject.RenderTransformOrigin = new Point(0.5f, 0.5f);

            associatedObject.IsManipulationEnabled = true;
            associatedObject.ManipulationStarting += OnManipulationStarting;
            associatedObject.ManipulationInertiaStarting += OnManipulationInertia;
            associatedObject.ManipulationDelta += OnManipulationDelta;
            associatedObject.MouseLeftButtonDown += OnMouseDown;
            associatedObject.MouseLeftButtonUp += OnMouseUp;
            associatedObject.MouseWheel += OnMouseWheel;
        }

        protected override void OnDetaching()
        {
            _scale = null;
            _rotate = null;
            _translate = null;

            FrameworkElement associatedObject = AssociatedObject;
            if (associatedObject != null)
            {
                associatedObject.RenderTransform = null;
                associatedObject.RenderTransformOrigin = new Point(0.0f, 0.0f);

                associatedObject.IsManipulationEnabled = false;
                associatedObject.ManipulationStarting -= OnManipulationStarting;
                associatedObject.ManipulationInertiaStarting -= OnManipulationInertia;
                associatedObject.ManipulationDelta -= OnManipulationDelta;
                associatedObject.MouseLeftButtonDown -= OnMouseDown;
                associatedObject.MouseLeftButtonUp -= OnMouseUp;
                associatedObject.MouseWheel -= OnMouseWheel;
            }
        }

        private void OnManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            FrameworkElement associatedObject = AssociatedObject;
            FrameworkElement container = associatedObject.Parent;
            if (container == null)
            {
                container = associatedObject;
            }

            e.ManipulationContainer = container;
            e.Mode = SupportedGestures;
            e.Handled = true;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            ManipulationDelta delta = e.DeltaManipulation;

            // Update Scale transform
            float scaleX = delta.Scale.X * _scale.ScaleX;
            _scale.ScaleX = Math.Max(Math.Max(0.00001f, MinimumScale), Math.Min(scaleX, MaximumScale));
            float scaleY = delta.Scale.Y * _scale.ScaleY;
            _scale.ScaleY = Math.Max(Math.Max(0.00001f, MinimumScale), Math.Min(scaleY, MaximumScale));

            // Update Rotate transform
            _rotate.Angle += delta.Rotation;

            // Update Translate transform
            _translate.X += delta.Translation.X;
            _translate.Y += delta.Translation.Y;

            // Limit transformation to parent bounds?
            if (e.IsInertial && ConstrainToParentBounds)
            {
                FrameworkElement container = (FrameworkElement)e.ManipulationContainer;
                FrameworkElement associatedObject = AssociatedObject;

                Matrix4 m = associatedObject.TransformToAncestor(container);

                Rect bounds = new Rect(container.RenderSize);
                Rect rect = new Rect(associatedObject.RenderSize);
                rect.Transform(m);

                if (!bounds.Contains(rect))
                {
                    e.Complete();
                }
            }

            e.Handled = true;
        }

        private float Deceleration(float friction, float velocity)
        {
            float k = friction == 1.0f ? 1.0f : (float)Math.Log(1 - friction) * 2.0f / 300.0f;
            return Math.Max(0.0f, velocity * k);
        }

        private void OnManipulationInertia(object sender, ManipulationInertiaStartingEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = Deceleration(TranslateFriction,
                ((Vector)e.InitialVelocities.LinearVelocity).Length);

            e.RotationBehavior.DesiredDeceleration = Deceleration(RotationalFriction,
                Math.Abs(e.InitialVelocities.AngularVelocity));

            e.Handled = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement associatedObject = AssociatedObject;

            associatedObject.MouseMove += OnMouseMove;
            associatedObject.LostMouseCapture += OnMouseLost;

            associatedObject.CaptureMouse();

            _relativePosition = e.GetPosition(associatedObject);

            e.Handled = true;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            AssociatedObject.ReleaseMouseCapture();
            e.Handled = true;
        }

        private void OnMouseMove(object seneder, MouseEventArgs e)
        {
            if (!_settingPosition)
            {
                _settingPosition = true;

                FrameworkElement associatedObject = AssociatedObject;
                Vector delta = e.GetPosition(associatedObject) - _relativePosition;

                ManipulationModes supportedGestures = SupportedGestures;
                if ((supportedGestures & ManipulationModes.TranslateX) == 0)
                {
                    delta.X = 0.0f;
                }
                if ((supportedGestures & ManipulationModes.TranslateY) == 0)
                {
                    delta.Y = 0.0f;
                }

                delta.X *= _scale.ScaleX;
                delta.Y *= _scale.ScaleY;

                _translate.X += delta.X;
                _translate.Y += delta.Y;

                _settingPosition = false;

                e.Handled = true;
            }
        }

        void OnMouseLost(object sender, MouseEventArgs e)
        {
            FrameworkElement associatedObject = AssociatedObject;
            associatedObject.MouseMove -= OnMouseMove;
            associatedObject.LostMouseCapture -= OnMouseLost;

            e.Handled = true;
        }

        void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ManipulationModes supportedGestures = SupportedGestures;
            if ((supportedGestures & ManipulationModes.Scale) != 0)
            {
                float sensitivity = WheelSensitivity;
                float factor = 1.0f + sensitivity;
                float prevScale = _scale.ScaleX;
                float scale = prevScale * (e.Delta > 0 ? factor : 1.0f / factor);

                FrameworkElement target = AssociatedObject;
                Size renderSize = target.RenderSize;
                Point center = new Point(renderSize.Width* 0.5f, renderSize.Height * 0.5f);
                Point point = e.GetPosition(target);
                Point pointScaled = center + (point - center) * scale;

                _scale.ScaleX = 1.0f;
                _scale.ScaleY = 1.0f;
                _translate.X = 0.0f;
                _translate.Y = 0.0f;

                point = e.GetPosition(target);
                Vector offset = point - pointScaled;

                _scale.ScaleX = scale;
                _scale.ScaleY = scale;
                _translate.X = offset.X;
                _translate.Y = offset.Y;

                e.Handled = true;
            }
        }

        private ScaleTransform _scale = null;
        private RotateTransform _rotate = null;
        private TranslateTransform _translate = null;
        private Point _relativePosition = new Point(0.0f, 0.0f);
        private bool _settingPosition = false;
    }
}
