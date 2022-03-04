////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Xaml.Behaviors;

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


        public static string GetId(object obj)
        {
            if (obj is DependencyObject dob) return (string)dob.GetValue(IdProperty);
            if (obj is LocTableExtension ext) return ext.Id;
            return string.Empty;
        }

        public static void SetId(object obj, string value)
        {
            if (obj is DependencyObject dob) dob.SetValue(IdProperty, value);
            else if (obj is LocTableExtension ext) ext.Id = value;
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
    ///           <TextBlock Text="{noesis:LocText 'This uses a local namespace', Key=LocalText, Namespace=LocalNamespace}"/>
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


        public static string GetNamespace(object obj)
        {
            if (obj is DependencyObject dob) return (string)dob.GetValue(NamespaceProperty);
            if (obj is LocTextExtension ext) return ext.Namespace;
            return string.Empty;
        }

        public static void SetNamespace(object obj, string value)
        {
            if (obj is DependencyObject dob) dob.SetValue(NamespaceProperty, value);
            else if (obj is LocTextExtension ext) ext.Namespace = value;
        }

        public static readonly DependencyProperty NamespaceProperty = DependencyProperty.RegisterAttached(
            "Namespace", typeof(string), typeof(LocTextExtension),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.Inherits));

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Source;
        }
    }


    /// <summary>
    /// Type of event that triggers the action
    /// </summary>
    public enum InputActionType
    {
        Pressed,
        Released,
        Repeat,
        DoubleClick,
        Axis
    }

    /// <summary>
    /// Interactivity trigger that listens to an Unreal's Input Action
    /// 
    /// Usage:
    /// 
    ///     <Grid 
    ///       xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///       xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///       xmlns:b="http://schemas.microsoft.com/xaml/behaviors"
    ///       xmlns:noesis="clr-namespace:NoesisGUIExtensions">
    ///         <b:Interaction.Triggers>
    ///           <noesis:InputActionTrigger Action="Fire" Type="Pressed" Consume="False">
    ///             <b:ChangePropertyAction TargetName="rect" PropertyName="Fill" Value="Red"/>
    ///           </noesis:InputActionTrigger>
    ///         </b:Interaction.Triggers>
    ///         <Rectangle x:Name="rect" Fill="Transparent"/>
    ///     </Grid>
    ///     
    /// </summary>
    public class InputActionTrigger : TriggerBase<UIElement>
    {
        /// <summary>
        /// Unreal's input action name
        /// </summary>
        public string Action
        {
            get { return (string)GetValue(ActionProperty); }
            set { SetValue(ActionProperty, value); }
        }

        public static readonly DependencyProperty ActionProperty = DependencyProperty.Register(
            "Action", typeof(string), typeof(InputActionTrigger), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Event type that fires the trigger
        /// </summary>
        public InputActionType Type
        {
            get { return (InputActionType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            "Type", typeof(InputActionType), typeof(InputActionTrigger), new PropertyMetadata(InputActionType.Pressed));

        /// <summary>
        /// Indicates if input event is consumed by this action trigger
        /// </summary>
        public bool Consume
        {
            get { return (bool)GetValue(ConsumeProperty); }
            set { SetValue(ConsumeProperty, value); }
        }

        public static readonly DependencyProperty ConsumeProperty = DependencyProperty.Register(
            "Consume", typeof(bool), typeof(InputActionTrigger), new PropertyMetadata(false));
    }
}
