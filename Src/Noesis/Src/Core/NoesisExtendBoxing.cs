using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Globalization;
using System.Linq;

namespace Noesis
{
    ////////////////////////////////////////////////////////////////////////////////////////////////
    // Manages Noesis Extensibility
    ////////////////////////////////////////////////////////////////////////////////////////////////
    internal partial class Extend
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr BoxDelegate(object val);
        private static Dictionary<Type, BoxDelegate> _boxFunctions = CreateBoxFunctions();

        private static Dictionary<Type, BoxDelegate> CreateBoxFunctions()
        {
            Dictionary<Type, BoxDelegate> boxFunctions = new Dictionary<Type, BoxDelegate>(100);

            boxFunctions[typeof(string)] = (val) => NoesisGUI_.Box_String((string)val);
            boxFunctions[typeof(bool)] = (val) => NoesisGUI_.Box_Bool((bool)val);
            boxFunctions[typeof(float)] = (val) => NoesisGUI_.Box_Float((float)val);
            boxFunctions[typeof(double)] = (val) => NoesisGUI_.Box_Double((double)val);
            boxFunctions[typeof(decimal)] = (val) => NoesisGUI_.Box_Double((double)(decimal)val);
            boxFunctions[typeof(int)] = (val) => NoesisGUI_.Box_Int((int)val);
            boxFunctions[typeof(uint)] = (val) => NoesisGUI_.Box_UInt((uint)val);
            boxFunctions[typeof(long)] = (val) => NoesisGUI_.Box_Long((long)val);
            boxFunctions[typeof(ulong)] = (val) => NoesisGUI_.Box_ULong((ulong)val);
            boxFunctions[typeof(char)] = (val) => NoesisGUI_.Box_UInt((uint)(char)val);
            boxFunctions[typeof(short)] = (val) => NoesisGUI_.Box_Short((short)val);
            boxFunctions[typeof(sbyte)] = (val) => NoesisGUI_.Box_Short((short)(sbyte)val);
            boxFunctions[typeof(ushort)] = (val) => NoesisGUI_.Box_UShort((ushort)val);
            boxFunctions[typeof(byte)] = (val) => NoesisGUI_.Box_UShort((ushort)(byte)val);
            boxFunctions[typeof(Noesis.Color)] = (val) => NoesisGUI_.Box_Color((Noesis.Color)val);
            boxFunctions[typeof(Noesis.Point)] = (val) => NoesisGUI_.Box_Point((Noesis.Point)val);
            boxFunctions[typeof(Noesis.Rect)] = (val) => NoesisGUI_.Box_Rect((Noesis.Rect)val);
            boxFunctions[typeof(Noesis.Size)] = (val) => NoesisGUI_.Box_Size((Noesis.Size)val);
            boxFunctions[typeof(Noesis.Thickness)] = (val) => NoesisGUI_.Box_Thickness((Noesis.Thickness)val);
            boxFunctions[typeof(Noesis.CornerRadius)] = (val) => NoesisGUI_.Box_CornerRadius((Noesis.CornerRadius)val);
            boxFunctions[typeof(Noesis.GridLength)] = (val) => NoesisGUI_.Box_GridLength((Noesis.GridLength)val);
            boxFunctions[typeof(Noesis.Duration)] = (val) => NoesisGUI_.Box_Duration((Noesis.Duration)val);
            boxFunctions[typeof(Noesis.KeyTime)] = (val) => NoesisGUI_.Box_KeyTime((Noesis.KeyTime)val);
            boxFunctions[typeof(System.TimeSpan)] = (val) => NoesisGUI_.Box_TimeSpan((System.TimeSpan)val);
            boxFunctions[typeof(Noesis.VirtualizationCacheLength)] = (val) => NoesisGUI_.Box_VirtualizationCacheLength((Noesis.VirtualizationCacheLength)val);
            boxFunctions[typeof(Noesis.AlignmentX)] = (val) => NoesisGUI_.Box_AlignmentX((Noesis.AlignmentX)val);
            boxFunctions[typeof(Noesis.AlignmentY)] = (val) => NoesisGUI_.Box_AlignmentY((Noesis.AlignmentY)val);
            boxFunctions[typeof(Noesis.AutoToolTipPlacement)] = (val) => NoesisGUI_.Box_AutoToolTipPlacement((Noesis.AutoToolTipPlacement)val);
            boxFunctions[typeof(Noesis.BindingMode)] = (val) => NoesisGUI_.Box_BindingMode((Noesis.BindingMode)val);
            boxFunctions[typeof(Noesis.BitmapScalingMode)] = (val) => NoesisGUI_.Box_BitmapScalingMode((Noesis.BitmapScalingMode)val);
            boxFunctions[typeof(Noesis.BrushMappingMode)] = (val) => NoesisGUI_.Box_BrushMappingMode((Noesis.BrushMappingMode)val);
            boxFunctions[typeof(Noesis.CharacterCasing)] = (val) => NoesisGUI_.Box_CharacterCasing((Noesis.CharacterCasing)val);
            boxFunctions[typeof(Noesis.ClickMode)] = (val) => NoesisGUI_.Box_ClickMode((Noesis.ClickMode)val);
            boxFunctions[typeof(Noesis.ColorInterpolationMode)] = (val) => NoesisGUI_.Box_ColorInterpolationMode((Noesis.ColorInterpolationMode)val);
            boxFunctions[typeof(Noesis.Dock)] = (val) => NoesisGUI_.Box_Dock((Noesis.Dock)val);
            boxFunctions[typeof(Noesis.ExpandDirection)] = (val) => NoesisGUI_.Box_ExpandDirection((Noesis.ExpandDirection)val);
            boxFunctions[typeof(Noesis.FillRule)] = (val) => NoesisGUI_.Box_FillRule((Noesis.FillRule)val);
            boxFunctions[typeof(Noesis.FlowDirection)] = (val) => NoesisGUI_.Box_FlowDirection((Noesis.FlowDirection)val);
            boxFunctions[typeof(Noesis.FontStretch)] = (val) => NoesisGUI_.Box_FontStretch((Noesis.FontStretch)val);
            boxFunctions[typeof(Noesis.FontStyle)] = (val) => NoesisGUI_.Box_FontStyle((Noesis.FontStyle)val);
            boxFunctions[typeof(Noesis.FontWeight)] = (val) => NoesisGUI_.Box_FontWeight((Noesis.FontWeight)val);
            boxFunctions[typeof(Noesis.GeometryCombineMode)] = (val) => NoesisGUI_.Box_GeometryCombineMode((Noesis.GeometryCombineMode)val);
            boxFunctions[typeof(Noesis.GradientSpreadMethod)] = (val) => NoesisGUI_.Box_GradientSpreadMethod((Noesis.GradientSpreadMethod)val);
            boxFunctions[typeof(Noesis.HorizontalAlignment)] = (val) => NoesisGUI_.Box_HorizontalAlignment((Noesis.HorizontalAlignment)val);
            boxFunctions[typeof(Noesis.KeyboardNavigationMode)] = (val) => NoesisGUI_.Box_KeyboardNavigationMode((Noesis.KeyboardNavigationMode)val);
            boxFunctions[typeof(Noesis.LineStackingStrategy)] = (val) => NoesisGUI_.Box_LineStackingStrategy((Noesis.LineStackingStrategy)val);
            boxFunctions[typeof(Noesis.ListSortDirection)] = (val) => NoesisGUI_.Box_ListSortDirection((Noesis.ListSortDirection)val);
            boxFunctions[typeof(Noesis.MenuItemRole)] = (val) => NoesisGUI_.Box_MenuItemRole((Noesis.MenuItemRole)val);
            boxFunctions[typeof(Noesis.Orientation)] = (val) => NoesisGUI_.Box_Orientation((Noesis.Orientation)val);
            boxFunctions[typeof(Noesis.OverflowMode)] = (val) => NoesisGUI_.Box_OverflowMode((Noesis.OverflowMode)val);
            boxFunctions[typeof(Noesis.PenLineCap)] = (val) => NoesisGUI_.Box_PenLineCap((Noesis.PenLineCap)val);
            boxFunctions[typeof(Noesis.PenLineJoin)] = (val) => NoesisGUI_.Box_PenLineJoin((Noesis.PenLineJoin)val);
            boxFunctions[typeof(Noesis.PlacementMode)] = (val) => NoesisGUI_.Box_PlacementMode((Noesis.PlacementMode)val);
            boxFunctions[typeof(Noesis.PopupAnimation)] = (val) => NoesisGUI_.Box_PopupAnimation((Noesis.PopupAnimation)val);
            boxFunctions[typeof(Noesis.RelativeSourceMode)] = (val) => NoesisGUI_.Box_RelativeSourceMode((Noesis.RelativeSourceMode)val);
            boxFunctions[typeof(Noesis.SelectionMode)] = (val) => NoesisGUI_.Box_SelectionMode((Noesis.SelectionMode)val);
            boxFunctions[typeof(Noesis.CornerRadius)] = (val) => NoesisGUI_.Box_CornerRadius((Noesis.CornerRadius)val);
            boxFunctions[typeof(Noesis.Stretch)] = (val) => NoesisGUI_.Box_Stretch((Noesis.Stretch)val);
            boxFunctions[typeof(Noesis.StretchDirection)] = (val) => NoesisGUI_.Box_StretchDirection((Noesis.StretchDirection)val);
            boxFunctions[typeof(Noesis.TextAlignment)] = (val) => NoesisGUI_.Box_TextAlignment((Noesis.TextAlignment)val);
            boxFunctions[typeof(Noesis.TextTrimming)] = (val) => NoesisGUI_.Box_TextTrimming((Noesis.TextTrimming)val);
            boxFunctions[typeof(Noesis.TextWrapping)] = (val) => NoesisGUI_.Box_TextWrapping((Noesis.TextWrapping)val);
            boxFunctions[typeof(Noesis.TickBarPlacement)] = (val) => NoesisGUI_.Box_TickBarPlacement((Noesis.TickBarPlacement)val);
            boxFunctions[typeof(Noesis.TickPlacement)] = (val) => NoesisGUI_.Box_TickPlacement((Noesis.TickPlacement)val);
            boxFunctions[typeof(Noesis.TileMode)] = (val) => NoesisGUI_.Box_TileMode((Noesis.TileMode)val);
            boxFunctions[typeof(Noesis.VerticalAlignment)] = (val) => NoesisGUI_.Box_VerticalAlignment((Noesis.VerticalAlignment)val);
            boxFunctions[typeof(Noesis.Visibility)] = (val) => NoesisGUI_.Box_Visibility((Noesis.Visibility)val);
            boxFunctions[typeof(Noesis.ClockState)] = (val) => NoesisGUI_.Box_ClockState((Noesis.ClockState)val);
            boxFunctions[typeof(Noesis.EasingMode)] = (val) => NoesisGUI_.Box_EasingMode((Noesis.EasingMode)val);
            boxFunctions[typeof(Noesis.SlipBehavior)] = (val) => NoesisGUI_.Box_SlipBehavior((Noesis.SlipBehavior)val);
            boxFunctions[typeof(Noesis.FillBehavior)] = (val) => NoesisGUI_.Box_FillBehavior((Noesis.FillBehavior)val);
            boxFunctions[typeof(Noesis.GridViewColumnHeaderRole)] = (val) => NoesisGUI_.Box_GridViewColumnHeaderRole((Noesis.GridViewColumnHeaderRole)val);
            boxFunctions[typeof(Noesis.HandoffBehavior)] = (val) => NoesisGUI_.Box_HandoffBehavior((Noesis.HandoffBehavior)val);
            boxFunctions[typeof(Noesis.PanningMode)] = (val) => NoesisGUI_.Box_PanningMode((Noesis.PanningMode)val);
            boxFunctions[typeof(Noesis.UpdateSourceTrigger)] = (val) => NoesisGUI_.Box_UpdateSourceTrigger((Noesis.UpdateSourceTrigger)val);
            boxFunctions[typeof(Noesis.ScrollUnit)] = (val) => NoesisGUI_.Box_ScrollUnit((Noesis.ScrollUnit)val);
            boxFunctions[typeof(Noesis.VirtualizationMode)] = (val) => NoesisGUI_.Box_VirtualizationMode((Noesis.VirtualizationMode)val);
            boxFunctions[typeof(Noesis.VirtualizationCacheLengthUnit)] = (val) => NoesisGUI_.Box_VirtualizationCacheLengthUnit((Noesis.VirtualizationCacheLengthUnit)val);

            return boxFunctions;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static IntPtr Box(object val)
        {
            BoxDelegate boxFunction;
            if (_boxFunctions.TryGetValue(val.GetType(), out boxFunction))
            {
                return RegisterPendingRelease(boxFunction(val));
            }
            else if (val.GetType().GetTypeInfo().IsEnum)
            {
                _boxFunctions.TryGetValue(typeof(int), out boxFunction);
                return RegisterPendingRelease(boxFunction((int)Convert.ToInt64(val)));
            }
            else if (val is Type)
            {
                ResourceKeyType resourceKey = GetResourceKeyType((Type)val);
                HandleRef handle = BaseComponent.getCPtr(resourceKey);
                BaseComponent.AddReference(handle.Handle);
                return RegisterPendingRelease(handle.Handle);
            }
            else
            {
                return IntPtr.Zero;
            }
        }

        private static IntPtr RegisterPendingRelease(IntPtr cPtr)
        {
            AddPendingRelease(cPtr);
            return cPtr;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private class Boxed<T> { }
        private delegate object UnboxDelegate(IntPtr cPtr);
        private static Dictionary<Type, UnboxDelegate> _unboxFunctions = CreateUnboxFunctions();

        private static Dictionary<Type, UnboxDelegate> CreateUnboxFunctions()
        {
            Dictionary<Type, UnboxDelegate> unboxFunctions = new Dictionary<Type, UnboxDelegate>(100);

            unboxFunctions[typeof(Boxed<string>)] = (cPtr) => NoesisGUI_.Unbox_String(cPtr);
            unboxFunctions[typeof(Boxed<bool>)] = (cPtr) => NoesisGUI_.Unbox_Bool(cPtr);
            unboxFunctions[typeof(Boxed<float>)] = (cPtr) => NoesisGUI_.Unbox_Float(cPtr);
            unboxFunctions[typeof(Boxed<double>)] = (cPtr) => NoesisGUI_.Unbox_Double(cPtr);
            unboxFunctions[typeof(Boxed<int>)] = (cPtr) => NoesisGUI_.Unbox_Int(cPtr);
            unboxFunctions[typeof(Boxed<uint>)] = (cPtr) => NoesisGUI_.Unbox_UInt(cPtr);
            unboxFunctions[typeof(Boxed<long>)] = (cPtr) => NoesisGUI_.Unbox_Long(cPtr);
            unboxFunctions[typeof(Boxed<ulong>)] = (cPtr) => NoesisGUI_.Unbox_ULong(cPtr);
            unboxFunctions[typeof(Boxed<short>)] = (cPtr) => NoesisGUI_.Unbox_Short(cPtr);
            unboxFunctions[typeof(Boxed<ushort>)] = (cPtr) => NoesisGUI_.Unbox_UShort(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.Color>)] = (cPtr) => NoesisGUI_.Unbox_Color(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.Point>)] = (cPtr) => NoesisGUI_.Unbox_Point(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.Rect>)] = (cPtr) => NoesisGUI_.Unbox_Rect(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.Size>)] = (cPtr) => NoesisGUI_.Unbox_Size(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.Thickness>)] = (cPtr) => NoesisGUI_.Unbox_Thickness(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.CornerRadius>)] = (cPtr) => NoesisGUI_.Unbox_CornerRadius(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.GridLength>)] = (cPtr) => NoesisGUI_.Unbox_GridLength(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.Duration>)] = (cPtr) => NoesisGUI_.Unbox_Duration(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.KeyTime>)] = (cPtr) => NoesisGUI_.Unbox_KeyTime(cPtr);
            unboxFunctions[typeof(Boxed<System.TimeSpan>)] = (cPtr) => NoesisGUI_.Unbox_TimeSpan(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.VirtualizationCacheLength>)] = (cPtr) => NoesisGUI_.Unbox_VirtualizationCacheLength(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.AlignmentX>)] = (cPtr) => NoesisGUI_.Unbox_AlignmentX(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.AlignmentY>)] = (cPtr) => NoesisGUI_.Unbox_AlignmentY(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.AutoToolTipPlacement>)] = (cPtr) => NoesisGUI_.Unbox_AutoToolTipPlacement(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.BindingMode>)] = (cPtr) => NoesisGUI_.Unbox_BindingMode(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.BitmapScalingMode>)] = (cPtr) => NoesisGUI_.Unbox_BitmapScalingMode(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.BrushMappingMode>)] = (cPtr) => NoesisGUI_.Unbox_BrushMappingMode(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.CharacterCasing>)] = (cPtr) => NoesisGUI_.Unbox_CharacterCasing(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.ClickMode>)] = (cPtr) => NoesisGUI_.Unbox_ClickMode(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.ColorInterpolationMode>)] = (cPtr) => NoesisGUI_.Unbox_ColorInterpolationMode(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.Dock>)] = (cPtr) => NoesisGUI_.Unbox_Dock(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.ExpandDirection>)] = (cPtr) => NoesisGUI_.Unbox_ExpandDirection(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.FillRule>)] = (cPtr) => NoesisGUI_.Unbox_FillRule(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.FlowDirection>)] = (cPtr) => NoesisGUI_.Unbox_FlowDirection(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.FontStretch>)] = (cPtr) => NoesisGUI_.Unbox_FontStretch(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.FontStyle>)] = (cPtr) => NoesisGUI_.Unbox_FontStyle(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.FontWeight>)] = (cPtr) => NoesisGUI_.Unbox_FontWeight(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.GeometryCombineMode>)] = (cPtr) => NoesisGUI_.Unbox_GeometryCombineMode(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.GradientSpreadMethod>)] = (cPtr) => NoesisGUI_.Unbox_GradientSpreadMethod(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.HorizontalAlignment>)] = (cPtr) => NoesisGUI_.Unbox_HorizontalAlignment(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.KeyboardNavigationMode>)] = (cPtr) => NoesisGUI_.Unbox_KeyboardNavigationMode(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.LineStackingStrategy>)] = (cPtr) => NoesisGUI_.Unbox_LineStackingStrategy(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.ListSortDirection>)] = (cPtr) => NoesisGUI_.Unbox_ListSortDirection(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.MenuItemRole>)] = (cPtr) => NoesisGUI_.Unbox_MenuItemRole(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.Orientation>)] = (cPtr) => NoesisGUI_.Unbox_Orientation(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.OverflowMode>)] = (cPtr) => NoesisGUI_.Unbox_OverflowMode(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.PenLineCap>)] = (cPtr) => NoesisGUI_.Unbox_PenLineCap(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.PenLineJoin>)] = (cPtr) => NoesisGUI_.Unbox_PenLineJoin(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.PlacementMode>)] = (cPtr) => NoesisGUI_.Unbox_PlacementMode(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.PopupAnimation>)] = (cPtr) => NoesisGUI_.Unbox_PopupAnimation(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.RelativeSourceMode>)] = (cPtr) => NoesisGUI_.Unbox_RelativeSourceMode(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.SelectionMode>)] = (cPtr) => NoesisGUI_.Unbox_SelectionMode(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.CornerRadius>)] = (cPtr) => NoesisGUI_.Unbox_CornerRadius(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.Stretch>)] = (cPtr) => NoesisGUI_.Unbox_Stretch(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.StretchDirection>)] = (cPtr) => NoesisGUI_.Unbox_StretchDirection(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.TextAlignment>)] = (cPtr) => NoesisGUI_.Unbox_TextAlignment(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.TextTrimming>)] = (cPtr) => NoesisGUI_.Unbox_TextTrimming(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.TextWrapping>)] = (cPtr) => NoesisGUI_.Unbox_TextWrapping(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.TickBarPlacement>)] = (cPtr) => NoesisGUI_.Unbox_TickBarPlacement(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.TickPlacement>)] = (cPtr) => NoesisGUI_.Unbox_TickPlacement(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.TileMode>)] = (cPtr) => NoesisGUI_.Unbox_TileMode(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.VerticalAlignment>)] = (cPtr) => NoesisGUI_.Unbox_VerticalAlignment(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.Visibility>)] = (cPtr) => NoesisGUI_.Unbox_Visibility(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.ClockState>)] = (cPtr) => NoesisGUI_.Unbox_ClockState(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.EasingMode>)] = (cPtr) => NoesisGUI_.Unbox_EasingMode(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.SlipBehavior>)] = (cPtr) => NoesisGUI_.Unbox_SlipBehavior(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.FillBehavior>)] = (cPtr) => NoesisGUI_.Unbox_FillBehavior(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.GridViewColumnHeaderRole>)] = (cPtr) => NoesisGUI_.Unbox_GridViewColumnHeaderRole(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.HandoffBehavior>)] = (cPtr) => NoesisGUI_.Unbox_HandoffBehavior(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.PanningMode>)] = (cPtr) => NoesisGUI_.Unbox_PanningMode(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.UpdateSourceTrigger>)] = (cPtr) => NoesisGUI_.Unbox_UpdateSourceTrigger(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.ScrollUnit>)] = (cPtr) => NoesisGUI_.Unbox_ScrollUnit(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.VirtualizationMode>)] = (cPtr) => NoesisGUI_.Unbox_VirtualizationMode(cPtr);
            unboxFunctions[typeof(Boxed<Noesis.VirtualizationCacheLengthUnit>)] = (cPtr) => NoesisGUI_.Unbox_VirtualizationCacheLengthUnit(cPtr);

            return unboxFunctions;
        }

        public static object Unbox(IntPtr cPtr, NativeTypeInfo info)
        {
            UnboxDelegate unboxFunction;
            if (_unboxFunctions.TryGetValue(info.Type, out unboxFunction))
            {
                return unboxFunction(cPtr);
            }

            throw new InvalidOperationException("Can't unbox native pointer");
        }
    }
}
