////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Controls;

namespace NoesisGUIExtensions
{
    public enum PPAAMode
    {
        Default,
        Disabled
    }

    /// <summary>
    /// Extends UI elements with properties not supported by WPF but included in Noesis
    /// </summary>
    public static class Element
    {
        /// <summary>
        /// Adds 3D transform capabilities to UI elements.
        /// It Specifies the Transform3D object used to transform the UI element in 3D space.
        ///
        /// Usage:
        ///
        ///     <Grid
        ///       xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        ///       xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        ///       xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=Noesis.GUI.Extensions">
        ///         <Grid.Resources>
        ///             <Storyboard x:Key="AnimTransform3D" AutoReverse="True" RepeatBehavior="Forever">
        ///                 <DoubleAnimationUsingKeyFrames
        ///                   Storyboard.TargetProperty="(noesis:Element.Transform3D).(noesis:CompositeTransform3D.RotationY)"
        ///                   Storyboard.TargetName="Root">
        ///                     <EasingDoubleKeyFrame KeyTime="0:0:2" Value="-60">
        ///                         <EasingDoubleKeyFrame.EasingFunction>
        ///                             <SineEase EasingMode="EaseInOut"/>
        ///                         </EasingDoubleKeyFrame.EasingFunction>
        ///                     </EasingDoubleKeyFrame>
        ///                 </DoubleAnimationUsingKeyFrames>
        ///             </Storyboard>
        ///         </Grid.Resources>
        ///         <Grid.Triggers>
        ///             <EventTrigger RoutedEvent="FrameworkElement.Loaded">
        ///                 <BeginStoryboard Storyboard="{StaticResource AnimTransform3D}"/>
        ///             </EventTrigger>
        ///         </Grid.Triggers>
        ///         <Grid x:Name="Root">
        ///             <noesis:Element.Transform3D>
        ///                 <noesis:CompositeTransform3D RotationY="60"/>
        ///             </noesis:Element.Transform3D>
        ///             <Rectangle Width="500" Height="300" Fill="#80FF0000"/>
        ///             <TextBlock
        ///               Text="3D Transformations, wow!"
        ///               FontSize="40"
        ///               HorizontalAlignment="Center"
        ///               VerticalAlignment="Center"/>
        ///         </Grid>
        ///     </Grid>
        ///
        /// </summary>
        #region Transform3D attached property

        public static readonly DependencyProperty Transform3DProperty =
            DependencyProperty.RegisterAttached("Transform3D", typeof(Transform3D), typeof(Element),
            new PropertyMetadata(null));

        public static void SetTransform3D(UIElement element, Transform3D value)
        {
            element.SetValue(Transform3DProperty, value);
        }

        public static Transform3D GetTransform3D(UIElement element)
        {
            return (Transform3D)element.GetValue(Transform3DProperty);
        }

        #endregion

        /// <summary>
        /// Provides Focus Engagement properties: IsFocusEngagementEnabled and IsFocusEngaged.
        /// They can be used to style Control's focus state accordingly.
        /// </summary>
        #region Focus Engagement attached properties

        public static readonly DependencyProperty IsFocusEngagementEnabledProperty =
            DependencyProperty.RegisterAttached("IsFocusEngagementEnabled", typeof(bool), typeof(Element),
                new PropertyMetadata(false));

        public static void SetIsFocusEngagementEnabled(Control control, bool value)
        {
            control.SetValue(IsFocusEngagementEnabledProperty, value);
        }

        public static bool GetIsFocusEngagementEnabled(Control control)
        {
            return (bool)control.GetValue(IsFocusEngagementEnabledProperty);
        }

        public static readonly DependencyProperty IsFocusEngagedProperty =
            DependencyProperty.RegisterAttached("IsFocusEngaged", typeof(bool), typeof(Element),
                new PropertyMetadata(false));

        public static void SetIsFocusEngaged(Control control, bool value)
        {
            control.SetValue(IsFocusEngagedProperty, value);
        }

        public static bool GetIsFocusEngaged(Control control)
        {
            return (bool)control.GetValue(IsFocusEngagedProperty);
        }

        #endregion

        /// <summary>
        /// Determines whether antialiasing geometry is generated for this element. This property is
        /// inherited down the visual tree.
        /// </summary>
        #region PPAA attached properties

        public static readonly DependencyProperty PPAAModeProperty = DependencyProperty.RegisterAttached(
            "PPAAMode", typeof(PPAAMode), typeof(Element),
            new PropertyMetadata(PPAAMode.Default));

        public static void SetPPAAMode(FrameworkElement element, PPAAMode value)
        {
            element.SetValue(PPAAModeProperty, value);
        }

        public static PPAAMode GetPPAAMode(FrameworkElement element)
        {
            return (PPAAMode)element.GetValue(PPAAModeProperty);
        }

        public static readonly DependencyProperty PPAAInProperty = DependencyProperty.RegisterAttached(
            "PPAAIn", typeof(double), typeof(Element),
            new PropertyMetadata(0.25));

        public static void SetPPAAIn(FrameworkElement element, double value)
        {
            element.SetValue(PPAAInProperty, value);
        }

        public static double GetPPAAIn(FrameworkElement element)
        {
            return (double)element.GetValue(PPAAInProperty);
        }

        public static readonly DependencyProperty PPAAOutProperty = DependencyProperty.RegisterAttached(
            "PPAAOut", typeof(double), typeof(Element),
            new PropertyMetadata(0.5));

        public static void SetPPAAOut(FrameworkElement element, double value)
        {
            element.SetValue(PPAAOutProperty, value);
        }

        public static double GetPPAAOut(FrameworkElement element)
        {
            return (double)element.GetValue(PPAAOutProperty);
        }

        #endregion
    }
}
