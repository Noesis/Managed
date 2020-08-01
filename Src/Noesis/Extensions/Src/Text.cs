////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        public static void SetCharacterSpacing(UIElement element, int value)
        {
            element.SetValue(CharacterSpacingProperty, value);
        }

        public static int GetCharacterSpacing(UIElement element)
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
            DependencyProperty.RegisterAttached("Stroke", typeof(Brush), typeof(Text),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetStroke(UIElement element, Brush value)
        {
            element.SetValue(StrokeProperty, value);
        }

        public static Brush GetStroke(UIElement element)
        {
            return (Brush)element.GetValue(StrokeProperty);
        }

        #endregion

        /// <summary>
        /// Specifies the thickness of the text stroke
        /// </summary>
        #region StrokeThickness attached property

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.RegisterAttached("StrokeThickness", typeof(double), typeof(Text),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetStrokeThickness(UIElement element, double value)
        {
            element.SetValue(StrokeThicknessProperty, value);
        }

        public static double GetStrokeThickness(UIElement element)
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
    }
}
