////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Markup;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Markup extension used to integrate Unreal Engine localization using String Tables into Noesis
    ///
    /// Usage:
    ///
    ///     <Grid 
    ///       xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///       xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///       xmlns:noesis="clr-namespace:NoesisGUIExtensions"
    ///       noesis:LocTable.Id="GameTable">
    ///         <StackPanel>
    ///           <TextBlock Text="{noesis:LocTable 'Hello World!', Key=HelloWorld}"/>
    ///           <TextBlock Text="{noesis:LocTable 'This uses a local table', Key=LocalText, Id=LocalTable}"/>
    ///         </StackPanel>
    ///     </Grid>
    ///
    /// </summary>
    public class LocTableExtension : MarkupExtension
    {
        public LocTableExtension(string source)
        {
            Source = source;
        }

        public string Id { get; set; }
        public string Key { get; set; }
        public string Source { get; set; }


        public static string GetId(UIElement obj)
        {
            return (string)obj.GetValue(IdProperty);
        }

        public static void SetId(UIElement obj, string value)
        {
            obj.SetValue(IdProperty, value);
        }

        public static readonly DependencyProperty IdProperty = DependencyProperty.RegisterAttached(
            "Id", typeof(string), typeof(LocTableExtension),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.Inherits));

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Source;
        }
    }

    /// <summary>
    /// Markup extension used to integrate Unreal Engine localization using Namespace Texts into Noesis
    ///
    /// Usage:
    ///
    ///     <Grid 
    ///       xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///       xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///       xmlns:noesis="clr-namespace:NoesisGUIExtensions"
    ///       noesis:LocText.Namespace="MainMenuNamespace">
    ///         <StackPanel>
    ///           <TextBlock Text="{noesis:LocText 'Hello World!', Key=HelloWorld}"/>
    ///           <TextBlock Text="{noesis:LocText 'This uses a local namespace', Key=LocalText, Id=LocalNamespace}"/>
    ///         </StackPanel>
    ///     </Grid>
    ///
    /// </summary>
    public class LocTextExtension : MarkupExtension
    {
        public LocTextExtension(string source)
        {
            Source = source;
        }

        public string Namespace { get; set; }
        public string Key { get; set; }
        public string Source { get; set; }


        public static string GetNamespace(UIElement obj)
        {
            return (string)obj.GetValue(NamespaceProperty);
        }

        public static void SetNamespace(UIElement obj, string value)
        {
            obj.SetValue(NamespaceProperty, value);
        }

        public static readonly DependencyProperty NamespaceProperty = DependencyProperty.RegisterAttached(
            "Namespace", typeof(string), typeof(LocTextExtension),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.Inherits));

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Source;
        }
    }
}
