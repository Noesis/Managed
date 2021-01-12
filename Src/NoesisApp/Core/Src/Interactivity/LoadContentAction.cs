using System;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// An action that will set the Content of the target ContentControl by loading a xaml.
    /// </summary>
    public class LoadContentAction : NoesisApp.TargetedTriggerAction<Noesis.ContentControl>
    {
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly Noesis.DependencyProperty SourceProperty = Noesis.DependencyProperty.Register(
            "Source", typeof(Uri), typeof(LoadContentAction),
            new Noesis.PropertyMetadata(null));

        public new SetFocusAction Clone()
        {
            return (SetFocusAction)base.Clone();
        }

        public new SetFocusAction CloneCurrentValue()
        {
            return (SetFocusAction)base.CloneCurrentValue();
        }

        protected override void Invoke(object parameter)
        {
            Noesis.ContentControl control = Target;
            if (control != null)
            {
                Uri source = Source;
                if (source != null && !string.IsNullOrEmpty(source.OriginalString))
                {
                    object content = Noesis.GUI.LoadXaml(source.OriginalString);
                    control.Content = content;
                }
            }
        }
    }
}
