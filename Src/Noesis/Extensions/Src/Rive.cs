using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Renders a Rive scene.
    ///
    /// The scene can contain inputs (bool and float) to control the state machine and animations.
    ///
    /// Usage:
    ///
    ///  <noesis:RiveControl Source="scene.rive">
    ///    <noesis:RiveInput Name="life" Value="{Binding PlayerLife}"/>
    ///  </noesis:RiveControl>
    ///
    /// </summary>
    [ContentProperty("Inputs")]
    public class RiveControl : FrameworkElement
    {
        #region Source property
        public static Uri GetSource(DependencyObject element)
        {
            if (element == null) throw new ArgumentNullException("element");
            return (Uri)element.GetValue(SourceProperty);
        }
        public static void SetSource(DependencyObject element, Uri value)
        {
            if (element == null) throw new ArgumentNullException("element");
            element.SetValue(SourceProperty, value);
        }

        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            "Source", typeof(Uri), typeof(RiveControl), new PropertyMetadata(null));
        #endregion

        #region Stretch property
        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
            "Stretch", typeof(Stretch), typeof(RiveControl), new PropertyMetadata(null));
        #endregion

        #region State property
        public string State { get; private set; }
        #endregion

        #region Inputs property
        public RiveInputCollection Inputs { get; } = new RiveInputCollection();
        #endregion

        protected override void OnRender(DrawingContext context)
        {
            SolidColorBrush brush = Brushes.CornflowerBlue.Clone();
            brush.Opacity = 0.8;
            context.DrawRectangle(brush, null, new Rect(RenderSize));
        }
    }

    public class RiveInput : Animatable
    {
        #region InputName property
        public string InputName
        {
            get { return (string)GetValue(InputNameProperty); }
            set { SetValue(InputNameProperty, value); }
        }

        public static readonly DependencyProperty InputNameProperty = DependencyProperty.Register(
            "InputName", typeof(string), typeof(RiveInput), new PropertyMetadata(string.Empty));
        #endregion

        #region InputValue property
        public object InputValue
        {
            get { return (object)GetValue(InputValueProperty); }
            set { SetValue(InputValueProperty, value); }
        }

        public static readonly DependencyProperty InputValueProperty = DependencyProperty.Register(
            "InputValue", typeof(object), typeof(RiveInput), new PropertyMetadata(null));
        #endregion

        protected override Freezable CreateInstanceCore()
        {
            return new RiveInput();
        }
    }

    public class RiveInputCollection : FreezableCollection<RiveInput>
    {
    }

    public class RiveTriggerAction : TargetedTriggerAction<RiveControl>
    {
        #region TriggerName property
        public string TriggerName
        {
            get { return (string)GetValue(TriggerNameProperty); }
            set { SetValue(TriggerNameProperty, value); }
        }

        public static readonly DependencyProperty TriggerNameProperty = DependencyProperty.Register(
            "TriggerName", typeof(string), typeof(RiveTriggerAction), new PropertyMetadata(string.Empty));
        #endregion

        protected override void Invoke(object parameter)
        {
        }
    }
}
