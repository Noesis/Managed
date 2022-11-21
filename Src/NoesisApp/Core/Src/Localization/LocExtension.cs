using Noesis;
using System;
using System.Collections.Generic;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Implements a markup extension that supports references to a localization ResourceDictionary.
    ///
    /// Provides a value for any XAML property attribute by looking up a reference in the
    /// ResourceDictionary defined by the Source attached property. Values will be re-evaluated when
    /// the Source attached property changes.
    ///
    /// If used with a string or object property, and the provided resource key is not found, the
    /// LocExtension will return a string in the format "<Loc !%s>" where %s is replaced with the key.
    ///
    /// This example shows the full setup for a LocExtension. It utilizes the RichText attached
    /// property to support BBCode markup in the localized strings. The Loc.Source property references
    /// the "Language_en-gb.xaml" ResourceDictionary below.
    ///
    /// Usage:
    ///
    ///    <StackPanel
    ///      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///      xmlns:noesis="clr-namespace:NoesisGUIExtensions"
    ///      noesis:Loc.Source="Language_en-gb.xaml">
    ///      <Image Source="{noesis:Loc Flag}"/>
    ///      <TextBlock noesis:RichText.Text="{noesis:Loc TitleLabel}"/>
    ///      <TextBlock noesis:RichText.Text="{noesis:Loc SoundLabel}"/>
    ///    </StackPanel>
    /// 
    /// This is the contents of a "Language_en-gb.xaml" localized ResourceDictionary:
    ///
    /// Usage:
    ///
    ///    <ResourceDictionary
    ///      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///      xmlns:sys="clr-namespace:System;assembly=mscorlib">
    ///      <ImageBrush x:Key="Flag" ImageSource="Flag_en-gb.png" Stretch="Fill"/>
    ///      <sys:String x:Key="TitleLabel">[b]LOCALIZATION SAMPLE[/b]</sys:String>
    ///      <sys:String x:Key="SoundLabel">A [i]sound[/i] label</sys:String>
    ///    </ResourceDictionary>
    ///
    /// </summary>
    [ContentProperty("ResourceKey")]
    public class LocExtension : MarkupExtension
    {
        private string _resourceKey;

        public LocExtension()
        {
        }

        public LocExtension(string resourceKey)
        {
            _resourceKey = resourceKey;
        }

        /// <summary>
        /// Gets or sets the key to use when finding a resource in the active localization ResourceDictionary. 
        ///
        /// Usage:
        ///
        ///    <Grid
        ///        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        ///        xmlns:noesis="clr-namespace:NoesisGUIExtensions"
        ///        noesis:Loc.Source="Locale_fr.xaml">
        ///        <TextBlock Text="{noesis:Loc Title}"/>
        ///        <Image Source="{noesis:Loc IntroBackground}"/>
        ///    </Grid>
        ///
        /// </summary>
        public string ResourceKey
        {
            get { return _resourceKey; }
            set { _resourceKey = value; }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget valueTarget = serviceProvider as IProvideValueTarget;
            if (valueTarget == null)
            {
                return null;
            }

            DependencyObject target = (DependencyObject)valueTarget.TargetObject;
            DependencyProperty targetProperty = (DependencyProperty)valueTarget.TargetProperty;

            LocMonitor monitor = (LocMonitor)target.GetValue(MonitorProperty);

            if (monitor == null)
            {
                monitor = new LocMonitor(target);
                target.SetValue(MonitorProperty, monitor);
            }

            return monitor.AddDependencyProperty(targetProperty, _resourceKey);
        }

        #region Source attached property

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached(
                "Source",
                typeof(Uri),
                typeof(LocExtension),
                new PropertyMetadata(null, SourceChangedCallback)
            );

        private static void SourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Uri source = (Uri)d.GetValue(SourceProperty);

            ResourceDictionary resourceDictionary = new ResourceDictionary
            {
                Source = source
            };

            d.SetValue(ResourcesProperty, resourceDictionary);
        }

        public static Uri GetSource(UIElement target)
        {
            return (Uri)target.GetValue(SourceProperty);
        }

        public static void SetSource(UIElement target, Uri value)
        {
            target.SetValue(SourceProperty, value);
        }

        #endregion

        #region Resources attached property

        private static readonly DependencyProperty ResourcesProperty =
            DependencyProperty.RegisterAttached(
                ".Resources",
                typeof(ResourceDictionary),
                typeof(LocExtension),
                new FrameworkPropertyMetadata(null, flags: FrameworkPropertyMetadataOptions.Inherits, ResourcesChangedCallback)
            );

        private static void ResourcesChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LocMonitor monitor = (LocMonitor)d.GetValue(MonitorProperty);

            if (monitor != null)
            {
                monitor.InvalidateResources((ResourceDictionary)e.NewValue);
            }
        }

        public static ResourceDictionary GetResources(DependencyObject dependencyObject)
        {
            return (ResourceDictionary)dependencyObject.GetValue(ResourcesProperty);
        }

        #endregion

        #region Monitor attached property

        private static readonly DependencyProperty MonitorProperty =
            DependencyProperty.RegisterAttached(
                ".Monitor",
                typeof(LocMonitor),
                typeof(LocExtension),
                new PropertyMetadata(null)
            );

        #endregion
    }

    internal class LocMonitor
    {
        private readonly DependencyObject _targetObject;

        private readonly List<Tuple<DependencyProperty, string>> _monitoredDependencyProperties =
            new List<Tuple<DependencyProperty, string>>();

        public LocMonitor(DependencyObject targetObject)
        {
            _targetObject = targetObject;
        }

        public object AddDependencyProperty(DependencyProperty targetProperty, string resourceKey)
        {
            ResourceDictionary resourceDictionary = LocExtension.GetResources(_targetObject);

            for (int i = 0; i < _monitoredDependencyProperties.Count; i++)
            {
                if (_monitoredDependencyProperties[i].Item1 == targetProperty)
                {
                    _monitoredDependencyProperties[i] =
                        new Tuple<DependencyProperty, string>(_monitoredDependencyProperties[i].Item1, resourceKey);

                    return Evaluate(targetProperty, resourceKey, resourceDictionary);
                }
            }

            _monitoredDependencyProperties.Add(new Tuple<DependencyProperty, string>(targetProperty, resourceKey));

            return Evaluate(targetProperty, resourceKey, resourceDictionary);
        }

        public void InvalidateResources(ResourceDictionary resourceDictionary)
        {
            foreach (Tuple<DependencyProperty, string> entry in _monitoredDependencyProperties)
            {
                _targetObject.SetValue(entry.Item1,
                    Evaluate(entry.Item1, entry.Item2, resourceDictionary));
            }
        }

        private static object Evaluate(DependencyProperty targetProperty, string resourceKey,
            ResourceDictionary resourceDictionary)
        {
            if (resourceDictionary != null && resourceDictionary.Contains(resourceKey))
            {
                object resource = resourceDictionary[resourceKey];
                if (resource != null)
                {
                    return resource;
                }

                Console.WriteLine($"[NOESIS/E] Resource key '{resourceKey}' not found in Loc Resources");
            }

            if (targetProperty.PropertyType == typeof(string))
            {
                return $"<Loc !{resourceKey}>";
            }

            return null;
        }
    }
}
