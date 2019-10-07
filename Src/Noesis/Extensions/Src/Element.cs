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
        /// Adds projection capabilities to UI elements.
        /// It Specifies the Projection object used to project the UI element.
        ///
        /// Usage:
        ///
        ///     <Grid
        ///       xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        ///       xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        ///       xmlns:noesis="clr-namespace:NoesisGUIExtensions">
        ///         <Grid.Resources>
        ///             <Storyboard x:Key="AnimProjection" AutoReverse="True" RepeatBehavior="Forever">
        ///                 <DoubleAnimationUsingKeyFrames
        ///                   Storyboard.TargetProperty="(noesis:Element.Projection).(noesis:PlaneProjection.RotationY)"
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
        ///                 <BeginStoryboard Storyboard="{StaticResource AnimProjection}"/>
        ///             </EventTrigger>
        ///         </Grid.Triggers>
        ///         <Grid x:Name="Root">
        ///             <noesis:Element.Projection>
        ///                 <noesis:PlaneProjection RotationY="60"/>
        ///             </noesis:Element.Projection>
        ///             <Rectangle Width="500" Height="300" Fill="#80FF0000"/>
        ///             <TextBlock
        ///               Text="3D Projection, wow!"
        ///               FontSize="40"
        ///               HorizontalAlignment="Center"
        ///               VerticalAlignment="Center"/>
        ///         </Grid>
        ///     </Grid>
        ///
        /// </summary>
        #region Projection attached property

        public static readonly DependencyProperty ProjectionProperty =
            DependencyProperty.RegisterAttached("Projection", typeof(Projection), typeof(Element),
            new PropertyMetadata(null));

        public static void SetProjection(UIElement element, Projection value)
        {
            element.SetValue(ProjectionProperty, value);
        }

        public static Projection GetProjection(UIElement element)
        {
            return (Projection)element.GetValue(ProjectionProperty);
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

        public static readonly DependencyProperty SupportsFocusEngagementProperty =
            DependencyProperty.RegisterAttached("SupportsFocusEngagement", typeof(bool), typeof(Element),
                new PropertyMetadata(false));

        public static bool GetSupportsFocusEngagement(DependencyObject obj)
        {
            return (bool)obj.GetValue(SupportsFocusEngagementProperty);
        }

        public static void SetSupportsFocusEngagement(DependencyObject obj, bool value)
        {
            obj.SetValue(SupportsFocusEngagementProperty, value);
        }

        #endregion

        /// <summary>
        /// Determines whether antialiasing geometry is generated for this element. This property is
        /// inherited down the visual tree.
        /// </summary>
        #region PPAA attached property

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

        #endregion
    }
}
