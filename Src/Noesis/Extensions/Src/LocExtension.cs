using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

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
            IProvideValueTarget valueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (valueTarget == null)
            {
                return this;
            }

            DependencyObject target = valueTarget.TargetObject as DependencyObject;
            Setter setterTarget = valueTarget.TargetObject as Setter;
            if (target == null && setterTarget == null)
            {
                return this;
            }

            DependencyProperty targetProperty = valueTarget.TargetProperty as DependencyProperty;
            if (target != null && targetProperty == null)
            {
                return null;
            }

            Binding binding = new Binding
            {
                Path = new PropertyPath("(0)[(1)]", ResourcesProperty, ResourceKey),
                RelativeSource = target is FrameworkElement ? RelativeSource.Self :
                    new RelativeSource(RelativeSourceMode.FindAncestor, typeof(FrameworkElement), 1),
                Converter = Converter,
                ConverterParameter = ConverterParameter
            };
            
            if (setterTarget != null)
            {
                return binding;
            }

            return binding.ProvideValue(serviceProvider);
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
                d.ClearValue(ResourcesProperty);
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

        public static readonly DependencyProperty ResourcesProperty =
            DependencyProperty.RegisterAttached(
                "Resources",
                typeof(ResourceDictionary),
                typeof(LocExtension),
                new FrameworkPropertyMetadata(null, flags: FrameworkPropertyMetadataOptions.Inherits)
            );

        public static ResourceDictionary GetResources(DependencyObject dependencyObject)
        {
            return (ResourceDictionary)dependencyObject.GetValue(ResourcesProperty);
        }

        public static void SetResources(DependencyObject dependencyObject, ResourceDictionary resources)
        {
            dependencyObject.SetValue(ResourcesProperty, resources);
        }

        #endregion
    }
}
