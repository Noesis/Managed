//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.10
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


using System;
using System.Runtime.InteropServices;

namespace Noesis
{

public abstract class TextElement : FrameworkElement {
  internal TextElement(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) {
  }

  internal static HandleRef getCPtr(TextElement obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  protected TextElement() {
  }

  public Typography Typography {
    get { return new Typography(this); }
  }

  public static FontFamily GetFontFamily(DependencyObject element) {
    if (element == null) throw new ArgumentNullException("element");
    {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_GetFontFamily(DependencyObject.getCPtr(element));
      return (FontFamily)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static void SetFontFamily(DependencyObject element, FontFamily family) {
    if (element == null) throw new ArgumentNullException("element");
    {
      NoesisGUI_PINVOKE.TextElement_SetFontFamily(DependencyObject.getCPtr(element), FontFamily.getCPtr(family));
    }
  }

  public static float GetFontSize(DependencyObject element) {
    if (element == null) throw new ArgumentNullException("element");
    {
      float ret = NoesisGUI_PINVOKE.TextElement_GetFontSize(DependencyObject.getCPtr(element));
      return ret;
    }
  }

  public static void SetFontSize(DependencyObject element, float size) {
    if (element == null) throw new ArgumentNullException("element");
    {
      NoesisGUI_PINVOKE.TextElement_SetFontSize(DependencyObject.getCPtr(element), size);
    }
  }

  public static FontStretch GetFontStretch(DependencyObject element) {
    if (element == null) throw new ArgumentNullException("element");
    {
      FontStretch ret = (FontStretch)NoesisGUI_PINVOKE.TextElement_GetFontStretch(DependencyObject.getCPtr(element));
      return ret;
    }
  }

  public static void SetFontStretch(DependencyObject element, FontStretch stretch) {
    if (element == null) throw new ArgumentNullException("element");
    {
      NoesisGUI_PINVOKE.TextElement_SetFontStretch(DependencyObject.getCPtr(element), (int)stretch);
    }
  }

  public static FontStyle GetFontStyle(DependencyObject element) {
    if (element == null) throw new ArgumentNullException("element");
    {
      FontStyle ret = (FontStyle)NoesisGUI_PINVOKE.TextElement_GetFontStyle(DependencyObject.getCPtr(element));
      return ret;
    }
  }

  public static void SetFontStyle(DependencyObject element, FontStyle style) {
    if (element == null) throw new ArgumentNullException("element");
    {
      NoesisGUI_PINVOKE.TextElement_SetFontStyle(DependencyObject.getCPtr(element), (int)style);
    }
  }

  public static FontWeight GetFontWeight(DependencyObject element) {
    if (element == null) throw new ArgumentNullException("element");
    {
      FontWeight ret = (FontWeight)NoesisGUI_PINVOKE.TextElement_GetFontWeight(DependencyObject.getCPtr(element));
      return ret;
    }
  }

  public static void SetFontWeight(DependencyObject element, FontWeight weight) {
    if (element == null) throw new ArgumentNullException("element");
    {
      NoesisGUI_PINVOKE.TextElement_SetFontWeight(DependencyObject.getCPtr(element), (int)weight);
    }
  }

  public static Brush GetForeground(DependencyObject element) {
    if (element == null) throw new ArgumentNullException("element");
    {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_GetForeground(DependencyObject.getCPtr(element));
      return (Brush)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static void SetForeground(DependencyObject element, Brush foreground) {
    if (element == null) throw new ArgumentNullException("element");
    {
      NoesisGUI_PINVOKE.TextElement_SetForeground(DependencyObject.getCPtr(element), Brush.getCPtr(foreground));
    }
  }

  public static Brush GetStroke(DependencyObject element) {
    if (element == null) throw new ArgumentNullException("element");
    {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_GetStroke(DependencyObject.getCPtr(element));
      return (Brush)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static void SetStroke(DependencyObject element, Brush stroke) {
    if (element == null) throw new ArgumentNullException("element");
    {
      NoesisGUI_PINVOKE.TextElement_SetStroke(DependencyObject.getCPtr(element), Brush.getCPtr(stroke));
    }
  }

  public static float GetStrokeThickness(DependencyObject element) {
    if (element == null) throw new ArgumentNullException("element");
    {
      float ret = NoesisGUI_PINVOKE.TextElement_GetStrokeThickness(DependencyObject.getCPtr(element));
      return ret;
    }
  }

  public static void SetStrokeThickness(DependencyObject element, float strokeThickness) {
    if (element == null) throw new ArgumentNullException("element");
    {
      NoesisGUI_PINVOKE.TextElement_SetStrokeThickness(DependencyObject.getCPtr(element), strokeThickness);
    }
  }

  public static DependencyProperty BackgroundProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_BackgroundProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty CharacterSpacingProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_CharacterSpacingProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty FontFamilyProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_FontFamilyProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty FontSizeProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_FontSizeProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty FontStretchProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_FontStretchProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty FontStyleProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_FontStyleProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty FontWeightProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_FontWeightProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty ForegroundProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_ForegroundProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty StrokeProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_StrokeProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty StrokeThicknessProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_StrokeThicknessProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public Brush Background {
    set {
      NoesisGUI_PINVOKE.TextElement_Background_set(swigCPtr, Brush.getCPtr(value));
    } 
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_Background_get(swigCPtr);
      return (Brush)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public int CharacterSpacing {
    set {
      NoesisGUI_PINVOKE.TextElement_CharacterSpacing_set(swigCPtr, value);
    } 
    get {
      int ret = NoesisGUI_PINVOKE.TextElement_CharacterSpacing_get(swigCPtr);
      return ret;
    } 
  }

  public FontFamily FontFamily {
    set {
      NoesisGUI_PINVOKE.TextElement_FontFamily_set(swigCPtr, FontFamily.getCPtr(value));
    } 
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_FontFamily_get(swigCPtr);
      return (FontFamily)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public float FontSize {
    set {
      NoesisGUI_PINVOKE.TextElement_FontSize_set(swigCPtr, value);
    } 
    get {
      float ret = NoesisGUI_PINVOKE.TextElement_FontSize_get(swigCPtr);
      return ret;
    } 
  }

  public FontStretch FontStretch {
    set {
      NoesisGUI_PINVOKE.TextElement_FontStretch_set(swigCPtr, (int)value);
    } 
    get {
      FontStretch ret = (FontStretch)NoesisGUI_PINVOKE.TextElement_FontStretch_get(swigCPtr);
      return ret;
    } 
  }

  public FontStyle FontStyle {
    set {
      NoesisGUI_PINVOKE.TextElement_FontStyle_set(swigCPtr, (int)value);
    } 
    get {
      FontStyle ret = (FontStyle)NoesisGUI_PINVOKE.TextElement_FontStyle_get(swigCPtr);
      return ret;
    } 
  }

  public FontWeight FontWeight {
    set {
      NoesisGUI_PINVOKE.TextElement_FontWeight_set(swigCPtr, (int)value);
    } 
    get {
      FontWeight ret = (FontWeight)NoesisGUI_PINVOKE.TextElement_FontWeight_get(swigCPtr);
      return ret;
    } 
  }

  public Brush Foreground {
    set {
      NoesisGUI_PINVOKE.TextElement_Foreground_set(swigCPtr, Brush.getCPtr(value));
    } 
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_Foreground_get(swigCPtr);
      return (Brush)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public Brush Stroke {
    set {
      NoesisGUI_PINVOKE.TextElement_Stroke_set(swigCPtr, Brush.getCPtr(value));
    } 
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.TextElement_Stroke_get(swigCPtr);
      return (Brush)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public float StrokeThickness {
    set {
      NoesisGUI_PINVOKE.TextElement_StrokeThickness_set(swigCPtr, value);
    } 
    get {
      float ret = NoesisGUI_PINVOKE.TextElement_StrokeThickness_get(swigCPtr);
      return ret;
    } 
  }

}

}

