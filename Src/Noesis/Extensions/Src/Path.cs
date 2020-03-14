////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NoesisGUIExtensions
{
    /// <summary>
    /// Extends Shape elements with path trimming.
    ///
    /// Usage:
    ///
    ///     <Grid 
    ///       xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///       xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///       xmlns:noesis="clr-namespace:NoesisGUIExtensions">
    ///         <Ellipse noesis:Path.TrimStart="0" noesis:Path.TrimEnd="0.6" Stroke="Black"/>
    ///     </Grid>
    ///
    /// </summary>
    public static class Path
    {
        /// <summary>
        /// Gets or sets the amount to trim the start of the geometry path.
        /// </summary>
        #region TrimStart attached property

        public static readonly DependencyProperty TrimStartProperty =
            DependencyProperty.RegisterAttached("TrimStart", typeof(double), typeof(Path),
            new FrameworkPropertyMetadata(0.0));

        public static void SetTrimStart(Shape element, double value)
        {
            element.SetValue(TrimStartProperty, value);
        }

        public static double GetTrimStart(Shape element)
        {
            return (double)element.GetValue(TrimStartProperty);
        }

        #endregion

        /// <summary>
        /// Gets or sets the amount to trim the end of the geometry path.
        /// </summary>
        #region TrimEnd attached property

        public static readonly DependencyProperty TrimEndProperty =
            DependencyProperty.RegisterAttached("TrimEnd", typeof(double), typeof(Path),
            new FrameworkPropertyMetadata(0.0));

        public static void SetTrimEnd(Shape element, double value)
        {
            element.SetValue(TrimEndProperty, value);
        }

        public static double GetTrimEnd(Shape element)
        {
            return (double)element.GetValue(TrimEndProperty);
        }

        #endregion

        /// <summary>
        /// Gets or sets the amount to offset trimming the geometry path.
        /// </summary>
        #region TrimOffset attached property

        public static readonly DependencyProperty TrimOffsetProperty =
            DependencyProperty.RegisterAttached("TrimOffset", typeof(double), typeof(Path),
            new FrameworkPropertyMetadata(0.0));

        public static void SetTrimOffset(Shape element, double value)
        {
            element.SetValue(TrimOffsetProperty, value);
        }

        public static double GetTrimOffset(Shape element)
        {
            return (double)element.GetValue(TrimOffsetProperty);
        }

        #endregion
    }
}
