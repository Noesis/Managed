////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NoesisGUIExtensions
{
    public enum PPAAMode
    {
        Default,
        Disabled
    }

    public enum BlendingMode
    {
        Normal,
        Multiply,
        Screen,
        Additive
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
        /// Adds support for XY Focus Navigation. It overrides default directional keyboard
        /// navigation by indicating the target element of the focus when user presses a direction
        /// on a keyboard or a controller.
        ///
        /// Usage:
        ///
        ///     <Grid
        ///       xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        ///       xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        ///       xmlns:noesis="clr-namespace:NoesisGUIExtensions;assembly=Noesis.GUI.Extensions">
        ///       <StackPanel Orientation="Horizontal">
        ///         <ContentControl Margin="20" noesis:Element.XYFocusRight="{Binding ElementName=Btn_3_A}">
        ///           <StackPanel>
        ///             <Button x:Name="Btn_1_A" Content="Button 1 A" Margin="0,5"/>
        ///             <Button x:Name="Btn_1_B" Content="Button 1 B" Margin="0,5"/>
        ///           </StackPanel>
        ///         </ContentControl>
        ///         <StackPanel Margin="20">
        ///           <Button x:Name="Btn_2_A" Content="Button 2 A" Margin="0,5"/>
        ///           <Button x:Name="Btn_2_B" Content="Button 2 B" Margin="0,5"
        ///                   noesis:Element.XYFocusDown="{Binding ElementName=Btn_3_A}"/>
        ///         </StackPanel>
        ///         <StackPanel Margin="20">
        ///           <Button x:Name="Btn_3_A" Content="Button 2 A" Margin="0,5"
        ///                   noesis:Element.XYFocusUp="{Binding ElementName=Btn_2_B}"/>
        ///           <Button x:Name="Btn_3_B" Content="Button 2 B" Margin="0,5"
        ///                   noesis:Element.XYFocusLeft="{Binding ElementName=Btn_1_A}"/>
        ///         </StackPanel>
        ///       </StackPanel>
        ///     </Grid>
        ///
        /// </summary>
        #region XY Focus Navigation attached properties

        public static UIElement GetXYFocusLeft(DependencyObject obj)
        {
            return (UIElement)obj.GetValue(XYFocusLeftProperty);
        }

        public static void SetXYFocusLeft(DependencyObject obj, UIElement value)
        {
            obj.SetValue(XYFocusLeftProperty, value);
        }

        public static readonly DependencyProperty XYFocusLeftProperty = DependencyProperty.RegisterAttached(
            "XYFocusLeft", typeof(UIElement), typeof(Element),
            new PropertyMetadata(null));

        public static UIElement GetXYFocusRight(DependencyObject obj)
        {
            return (UIElement)obj.GetValue(XYFocusRightProperty);
        }

        public static void SetXYFocusRight(DependencyObject obj, UIElement value)
        {
            obj.SetValue(XYFocusRightProperty, value);
        }

        public static readonly DependencyProperty XYFocusRightProperty = DependencyProperty.RegisterAttached(
            "XYFocusRight", typeof(UIElement), typeof(Element),
            new PropertyMetadata(null));

        public static UIElement GetXYFocusUp(DependencyObject obj)
        {
            return (UIElement)obj.GetValue(XYFocusUpProperty);
        }

        public static void SetXYFocusUp(DependencyObject obj, UIElement value)
        {
            obj.SetValue(XYFocusUpProperty, value);
        }

        public static readonly DependencyProperty XYFocusUpProperty = DependencyProperty.RegisterAttached(
            "XYFocusUp", typeof(UIElement), typeof(Element),
            new PropertyMetadata(null));

        public static UIElement GetXYFocusDown(DependencyObject obj)
        {
            return (UIElement)obj.GetValue(XYFocusDownProperty);
        }

        public static void SetXYFocusDown(DependencyObject obj, UIElement value)
        {
            obj.SetValue(XYFocusDownProperty, value);
        }

        public static readonly DependencyProperty XYFocusDownProperty = DependencyProperty.RegisterAttached(
            "XYFocusDown", typeof(UIElement), typeof(Element),
            new PropertyMetadata(null));

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

        /// <summary>
        /// Gets or sets a value indicating how the element's contents are mixed with the background.
        /// </summary>
        #region BlendingMode
        public static readonly DependencyProperty BlendingModeProperty = DependencyProperty.RegisterAttached(
            "BlendingMode", typeof(BlendingMode), typeof(Element), new PropertyMetadata(BlendingMode.Normal));

        public static BlendingMode GetBlendingMode(DependencyObject obj)
        {
            return (BlendingMode)obj.GetValue(BlendingModeProperty);
        }
        public static void SetBlendingMode(DependencyObject obj, BlendingMode value)
        {
            obj.SetValue(BlendingModeProperty, value);
        }
        #endregion

        /// <summary>
        /// Adds extra properties and events for high-level touch events:
        /// Tapped, DoubleTapped, Holding, RightTapped
        /// </summary>
        #region Touch extensions

        #region Properties
        public static readonly DependencyProperty IsTapEnabledProperty = DependencyProperty.RegisterAttached(
            "IsTapEnabled", typeof(bool), typeof(Element), new PropertyMetadata(true));

        public static bool GetIsTapEnabled(UIElement element)
        {
            return (bool)element.GetValue(IsTapEnabledProperty);
        }

        public static void SetIsTapEnabled(UIElement element, bool value)
        {
            element.SetValue(IsTapEnabledProperty, value);
        }

        public static readonly DependencyProperty IsDoubleTapEnabledProperty = DependencyProperty.RegisterAttached(
            "IsDoubleTapEnabled", typeof(bool), typeof(Element), new PropertyMetadata(true));

        public static bool GetIsDoubleTapEnabled(UIElement element)
        {
            return (bool)element.GetValue(IsDoubleTapEnabledProperty);
        }

        public static void SetIsDoubleTapEnabled(UIElement element, bool value)
        {
            element.SetValue(IsDoubleTapEnabledProperty, value);
        }

        public static readonly DependencyProperty IsHoldingEnabledProperty = DependencyProperty.RegisterAttached(
            "IsHoldingEnabled", typeof(bool), typeof(Element), new PropertyMetadata(true));

        public static bool GetIsHoldingEnabled(UIElement element)
        {
            return (bool)element.GetValue(IsHoldingEnabledProperty);
        }

        public static void SetIsHoldingEnabled(UIElement element, bool value)
        {
            element.SetValue(IsHoldingEnabledProperty, value);
        }

        public static readonly DependencyProperty IsRightTapEnabledProperty = DependencyProperty.RegisterAttached(
            "IsRightTapEnabled", typeof(bool), typeof(Element), new PropertyMetadata(true));

        public static bool GetIsRightTapEnabled(UIElement element)
        {
            return (bool)element.GetValue(IsRightTapEnabledProperty);
        }

        public static void SetIsRightTapEnabled(UIElement element, bool value)
        {
            element.SetValue(IsRightTapEnabledProperty, value);
        }
        #endregion

        #region Events
        public static readonly RoutedEvent TappedEvent = EventManager.RegisterRoutedEvent(
            "Tapped", RoutingStrategy.Bubble, typeof(TappedEventHandler), typeof(Element));

        public static void AddTappedHandler(DependencyObject d, TappedEventHandler handler)
        {
            UIElement element = d as UIElement;
            if (element != null)
            {
                element.AddHandler(TappedEvent, handler);
            }
        }

        public static void RemoveTappedHandler(DependencyObject d, TappedEventHandler handler)
        {
            UIElement element = d as UIElement;
            if (element != null)
            {
                element.RemoveHandler(TappedEvent, handler);
            }
        }

        public static readonly RoutedEvent DoubleTappedEvent = EventManager.RegisterRoutedEvent(
            "DoubleTapped", RoutingStrategy.Bubble, typeof(DoubleTappedEventHandler), typeof(Element));

        public static void AddDoubleTappedHandler(DependencyObject d, DoubleTappedEventHandler handler)
        {
            UIElement element = d as UIElement;
            if (element != null)
            {
                element.AddHandler(DoubleTappedEvent, handler);
            }
        }

        public static void RemoveDoubleTappedHandler(DependencyObject d, DoubleTappedEventHandler handler)
        {
            UIElement element = d as UIElement;
            if (element != null)
            {
                element.RemoveHandler(DoubleTappedEvent, handler);
            }
        }

        public static readonly RoutedEvent HoldingEvent = EventManager.RegisterRoutedEvent(
            "Holding", RoutingStrategy.Bubble, typeof(HoldingEventHandler), typeof(Element));

        public static void AddHoldingHandler(DependencyObject d, HoldingEventHandler handler)
        {
            UIElement element = d as UIElement;
            if (element != null)
            {
                element.AddHandler(HoldingEvent, handler);
            }
        }

        public static void RemoveHoldingHandler(DependencyObject d, HoldingEventHandler handler)
        {
            UIElement element = d as UIElement;
            if (element != null)
            {
                element.RemoveHandler(HoldingEvent, handler);
            }
        }

        public static readonly RoutedEvent RightTappedEvent = EventManager.RegisterRoutedEvent(
            "RightTapped", RoutingStrategy.Bubble, typeof(RightTappedEventHandler), typeof(Element));

        public static void AddRightTappedHandler(DependencyObject d, RightTappedEventHandler handler)
        {
            UIElement element = d as UIElement;
            if (element != null)
            {
                element.AddHandler(RightTappedEvent, handler);
            }
        }

        public static void RemoveTRightappedHandler(DependencyObject d, RightTappedEventHandler handler)
        {
            UIElement element = d as UIElement;
            if (element != null)
            {
                element.RemoveHandler(RightTappedEvent, handler);
            }
        }
        #endregion

        #endregion
    }
}

namespace System.Windows.Input
{
    public delegate void TappedEventHandler(object sender, TappedEventArgs e);
    public delegate void DoubleTappedEventHandler(object sender, DoubleTappedEventArgs e);
    public delegate void HoldingEventHandler(object sender, HoldingEventArgs e);
    public delegate void RightTappedEventHandler(object sender, RightTappedEventArgs e);

    public class TappedEventArgs : TouchEventArgs
    {
        public TappedEventArgs(TouchDevice touchDevice, int timestamp) : base(touchDevice, timestamp)
        {
        }
    }

    public class DoubleTappedEventArgs : TouchEventArgs
    {
        public DoubleTappedEventArgs(TouchDevice touchDevice, int timestamp) : base(touchDevice, timestamp)
        {
        }
    }

    public class HoldingEventArgs : TouchEventArgs
    {
        public HoldingEventArgs(TouchDevice touchDevice, int timestamp) : base(touchDevice, timestamp)
        {
        }

        public HoldingState HoldingState { get; }
    }

    public class RightTappedEventArgs : TouchEventArgs
    {
        public RightTappedEventArgs(TouchDevice touchDevice, int timestamp) : base(touchDevice, timestamp)
        {
        }
    }

    public enum HoldingState
    {
        Started,
        Completed,
        Canceled
    }
}

