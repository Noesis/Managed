////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Media;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Adds stroke capabilities to text elements.
    ///
    /// Usage:
    ///
    ///     <Grid 
    ///       xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///       xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///       xmlns:noesis="clr-namespace:NoesisGUIExtensions">
    ///         <TextBlock noesis:Text.Stroke="Red" noesis:Text.StrokeThickness="1" Text="Hello"/>
    ///     </Grid>
    ///
    /// </summary>
    public class Text
    {
        public Text()
        {
        }

        /// <summary>
        /// Specifies the brush used to stroke the text
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
    }
}
