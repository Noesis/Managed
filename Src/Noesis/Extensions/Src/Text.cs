////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Controls;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Adds new features as attached properties to text elements.
    /// </summary>
    public static class Text
    {
        /// <summary>
        /// Gets or sets the uniform spacing between characters, in units of 1/1000 of an em
        ///
        /// Usage:
        ///
        ///     <Grid 
        ///       xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        ///       xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        ///       xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=Noesis.GUI.Extensions">
        ///         <TextBlock Text="Hello" noesis:Text.CharacterSpacing="10"/>
        ///     </Grid>
        ///
        /// </summary>
        #region CharacterSpacing attached property

        public static readonly DependencyProperty CharacterSpacingProperty =
            DependencyProperty.RegisterAttached("CharacterSpacing", typeof(int), typeof(Text),
            new FrameworkPropertyMetadata((int)0));

        public static void SetCharacterSpacing(DependencyObject element, int value)
        {
            element.SetValue(CharacterSpacingProperty, value);
        }

        public static int GetCharacterSpacing(DependencyObject element)
        {
            return (int)element.GetValue(CharacterSpacingProperty);
        }

        #endregion

        /// <summary>
        /// Specifies the brush used to stroke the text
        ///
        /// Usage:
        ///
        ///     <Grid 
        ///       xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        ///       xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        ///       xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=Noesis.GUI.Extensions">
        ///         <TextBlock Text="Hello" noesis:Text.Stroke="Red" noesis:Text.StrokeThickness="1"/>
        ///     </Grid>
        ///
        /// </summary>
        #region Stroke attached property

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.RegisterAttached("Stroke", typeof(System.Windows.Media.Brush), typeof(Text),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetStroke(DependencyObject element, System.Windows.Media.Brush value)
        {
            element.SetValue(StrokeProperty, value);
        }

        public static System.Windows.Media.Brush GetStroke(DependencyObject element)
        {
            return (System.Windows.Media.Brush)element.GetValue(StrokeProperty);
        }

        #endregion

        /// <summary>
        /// Specifies the thickness of the text stroke
        /// </summary>
        #region StrokeThickness attached property

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.RegisterAttached("StrokeThickness", typeof(double), typeof(Text),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetStrokeThickness(DependencyObject element, double value)
        {
            element.SetValue(StrokeThicknessProperty, value);
        }

        public static double GetStrokeThickness(DependencyObject element)
        {
            return (double)element.GetValue(StrokeThicknessProperty);
        }

        #endregion

        /// <summary>
        /// Specifies the placeholder text for TextBoxes and PasswordBoxes
        ///
        /// Usage:
        ///
        ///     <Grid 
        ///       xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        ///       xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        ///       xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=Noesis.GUI.Extensions">
        ///         <TextBox noesis:Text.Placeholder="Type your name"/>
        ///     </Grid>
        ///
        /// </summary>
        #region Placeholder attached property

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached("Placeholder", typeof(string), typeof(Text),
            new FrameworkPropertyMetadata(string.Empty, OnPlaceholderChanged));

        public static void SetPlaceholder(UIElement element, string value)
        {
            element.SetValue(PlaceholderProperty, value);
        }

        public static string GetPlaceholder(UIElement element)
        {
            return (string)element.GetValue(PlaceholderProperty);
        }

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = d as PasswordBox;
            if (passwordBox != null)
            {
                string oldPlaceholder = e.OldValue as string;
                if (!string.IsNullOrEmpty(oldPlaceholder))
                {
                    passwordBox.PasswordChanged -= OnPasswordChanged;
                }

                string newPlaceholder = e.NewValue as string;
                if (!string.IsNullOrEmpty(newPlaceholder))
                {
                    passwordBox.PasswordChanged += OnPasswordChanged;
                }
            }
        }

        private static void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            uint passwordLength = (uint)passwordBox.Password.Length;
            passwordBox.SetValue(PasswordLengthProperty, passwordLength);
        }

        public static readonly DependencyProperty PasswordLengthProperty =
            DependencyProperty.RegisterAttached("PasswordLength", typeof(uint), typeof(Text),
            new FrameworkPropertyMetadata(0u));

        public static uint GetPasswordLength(PasswordBox element)
        {
            return (uint)element.GetValue(PasswordLengthProperty);
        }

        #endregion

        /// <summary>
        /// Specifies the duration in milliseconds that the last character typed in a PasswordBox
        /// remains visible before it is converted into the corresponding *PasswordChar*
        ///
        /// Usage:
        /// 
        ///     <Grid 
        ///       xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        ///       xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        ///       xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=Noesis.GUI.Extensions">
        ///         <PasswordBox noesis:Text.ShowLastCharacterDuration="2000"/>
        ///     </Grid>
        ///
        /// </summary>
        #region ShowLastCharacterDuration attached property

        public static readonly DependencyProperty ShowLastCharacterDurationProperty =
            DependencyProperty.RegisterAttached("ShowLastCharacterDuration", typeof(int), typeof(Text),
            new PropertyMetadata(0));

        public static int GetShowLastCharacterDuration(PasswordBox obj)
        {
            return (int)obj.GetValue(ShowLastCharacterDurationProperty);
        }

        public static void SetShowLastCharacterDuration(PasswordBox obj, int value)
        {
            obj.SetValue(ShowLastCharacterDurationProperty, value);
        }

        #endregion
    }
}
