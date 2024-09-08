using Noesis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

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
    /// A Converter can also be specified, with an optional ConverterParameter.
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
    ///      <TextBlock noesis:RichText.Text="{noesis:Loc SoundLabel}"/>
    ///      <TextBlock noesis:RichText.Text="{noesis:Loc TitleLabel, Converter={StaticResource CaseConverter}, ConverterParameter=UpperCase}"/>
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
    ///      <sys:String x:Key="TitleLabel">[b]Localization Sample[/b]</sys:String>
    ///      <sys:String x:Key="SoundLabel">A [i]sound[/i] label</sys:String>
    ///    </ResourceDictionary>
    ///
    /// </summary>
    [ContentProperty("ResourceKey")]
    public class LocExtension : MarkupExtension
    {
        private string _resourceKey;
        private IValueConverter _converter;
        private object _converterParameter;

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

        /// <summary>
        /// Gets or sets a converter to use when finding a resource in the active localization ResourceDictionary.
        /// </summary>
        public IValueConverter Converter
        {
            get => this._converter;
            set => this._converter = value;
        }

        /// <summary>
        /// Gets or sets an optional converter parameter to use when finding a resource in the active localization ResourceDictionary.
        /// </summary>
        public object ConverterParameter
        {
            get => this._converterParameter;
            set => this._converterParameter = value;
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

            return monitor.AddDependencyProperty(targetProperty, _resourceKey, Converter, ConverterParameter);
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

            if (source == null)
            {
                d.SetValue(ResourcesProperty, null);
                return;
            }

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
                if (monitor.TargetObject != d)
                {
                    monitor = monitor.Clone(d);
                    d.SetValue(MonitorProperty, monitor);
                }
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

        internal class LocMonitor
        {
            internal class LocProperty
            {
                public DependencyProperty TargetProperty { get; set; }
                public string ResourceKey { get; set; }
                public IValueConverter Converter { get; set; }
                public object ConverterParameter { get; set; }

                public LocProperty(DependencyProperty targetProperty, string resourceKey, IValueConverter converter, object converterParameter)
                {
                    TargetProperty = targetProperty;
                    ResourceKey = resourceKey;
                    Converter = converter;
                    ConverterParameter = converterParameter;
                }
            }

            private readonly List<LocProperty> _monitoredDependencyProperties =
                new List<LocProperty>();

            public DependencyObject TargetObject { get; }

            public LocMonitor(DependencyObject targetObject)
            {
                TargetObject = targetObject;

                if (!(TargetObject is FrameworkElement))
                {
                    Binding binding = new Binding(ResourcesProperty, new RelativeSource(RelativeSourceMode.FindAncestor, typeof(FrameworkElement), 1));
                    BindingOperations.SetBinding(TargetObject, ResourcesProperty, binding);
                }
            }

            public object AddDependencyProperty(DependencyProperty targetProperty, string resourceKey, IValueConverter converter, object converterParameter)
            {
                ResourceDictionary resourceDictionary = LocExtension.GetResources(TargetObject);

                for (int i = 0; i < _monitoredDependencyProperties.Count; i++)
                {
                    if (_monitoredDependencyProperties[i].TargetProperty == targetProperty)
                    {
                        _monitoredDependencyProperties[i] =
                            new LocProperty(_monitoredDependencyProperties[i].TargetProperty, resourceKey, converter, converterParameter);

                        return Evaluate(targetProperty, resourceKey, converter, converterParameter, resourceDictionary);
                    }
                }

                _monitoredDependencyProperties.Add(new LocProperty(targetProperty, resourceKey, converter, converterParameter));

                return Evaluate(targetProperty, resourceKey, converter, converterParameter, resourceDictionary);
            }

            public void InvalidateResources(ResourceDictionary resourceDictionary)
            {
                foreach (LocProperty entry in _monitoredDependencyProperties)
                {
                    TargetObject.SetValue(entry.TargetProperty,
                        Evaluate(entry.TargetProperty, entry.ResourceKey, entry.Converter, entry.ConverterParameter, resourceDictionary));
                }
            }

            public LocMonitor Clone(DependencyObject targetObject)
            {
                LocMonitor clone = new LocMonitor(targetObject);
                clone._monitoredDependencyProperties.AddRange(_monitoredDependencyProperties);
                return clone;
            }

            private static object DynamicConvert(Type targetType, object value)
            {
                Type valueType = value.GetType();
                if (!targetType.IsAssignableFrom(valueType))
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(targetType);
                    if (converter.CanConvertFrom(valueType))
                    {
                        value = converter.ConvertFrom(value);
                    }
                    else
                    {
                        converter = TypeDescriptor.GetConverter(valueType);
                        if (converter.CanConvertTo(targetType))
                        {
                            value = converter.ConvertTo(value, targetType);
                        }
                    }
                }

                return value;
            }

            private static object Evaluate(DependencyProperty targetProperty, string resourceKey, IValueConverter converter,
                object converterParameter, ResourceDictionary resourceDictionary)
            {
                if (resourceDictionary != null && resourceDictionary.Contains(resourceKey))
                {
                    object resource = resourceDictionary[resourceKey];
                    if (resource != null)
                    {
                        if (converter != null)
                        {
                            resource = converter.Convert(resource, targetProperty.PropertyType, converterParameter,
                                CultureInfo.InvariantCulture);
                        }

                        resource = DynamicConvert(targetProperty.PropertyType, resource);

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
}
