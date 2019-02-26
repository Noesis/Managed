using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// An action that will launch a process to open a file or uri.
    /// </summary>
    public class LaunchUriOrFileAction : TriggerAction<DependencyObject>
    {
        public new LaunchUriOrFileAction Clone()
        {
            return (LaunchUriOrFileAction)base.Clone();
        }

        public new LaunchUriOrFileAction CloneCurrentValue()
        {
            return (LaunchUriOrFileAction)base.CloneCurrentValue();
        }

        /// <summary>
        /// Gets or sets the file or uri to open
        /// </summary>
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
            "Path", typeof(string), typeof(LaunchUriOrFileAction),
            new PropertyMetadata(string.Empty));

        protected override void Invoke(object parameter)
        {
            string path = Path;
            if (AssociatedObject != null && !string.IsNullOrEmpty(path))
            {
                GUI.OpenUrl(path);
            }
        }
    }
}
