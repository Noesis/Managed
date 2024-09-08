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
using System.Collections.Generic;

namespace Noesis
{

public class VisualStateGroup : DependencyObject {
  internal new static VisualStateGroup CreateProxy(IntPtr cPtr, bool cMemoryOwn) {
    return new VisualStateGroup(cPtr, cMemoryOwn);
  }

  internal VisualStateGroup(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) {
  }

  internal static HandleRef getCPtr(VisualStateGroup obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  #region Events
  #region CurrentStateChanging
  public delegate void CurrentStateChangingHandler(object sender, VisualStateChangedEventArgs e);
  public event CurrentStateChangingHandler CurrentStateChanging {
    add {
      long ptr = swigCPtr.Handle.ToInt64();
      if (!_CurrentStateChanging.ContainsKey(ptr)) {
        _CurrentStateChanging.Add(ptr, null);

        NoesisGUI_PINVOKE.BindEvent_VisualStateGroup_CurrentStateChanging(_raiseCurrentStateChanging, swigCPtr.Handle);
      }

      _CurrentStateChanging[ptr] += value;
    }
    remove {
      long ptr = swigCPtr.Handle.ToInt64();
      if (_CurrentStateChanging.ContainsKey(ptr)) {

        _CurrentStateChanging[ptr] -= value;

        if (_CurrentStateChanging[ptr] == null) {
          NoesisGUI_PINVOKE.UnbindEvent_VisualStateGroup_CurrentStateChanging(_raiseCurrentStateChanging, swigCPtr.Handle);

          _CurrentStateChanging.Remove(ptr);
        }
      }
    }
  }

  internal delegate void RaiseCurrentStateChangingCallback(IntPtr cPtr, IntPtr sender, IntPtr e);
  private static RaiseCurrentStateChangingCallback _raiseCurrentStateChanging = RaiseCurrentStateChanging;

  [MonoPInvokeCallback(typeof(RaiseCurrentStateChangingCallback))]
  private static void RaiseCurrentStateChanging(IntPtr cPtr, IntPtr sender, IntPtr e) {
    try {
      if (Noesis.Extend.Initialized) {
        long ptr = cPtr.ToInt64();
        if (sender == IntPtr.Zero && e == IntPtr.Zero) {
          _CurrentStateChanging.Remove(ptr);
          return;
        }
        CurrentStateChangingHandler handler = null;
        if (!_CurrentStateChanging.TryGetValue(ptr, out handler)) {
          throw new InvalidOperationException("Delegate not registered for CurrentStateChanging event");
        }
        handler?.Invoke(Noesis.Extend.GetProxy(sender, false), new VisualStateChangedEventArgs(e, false));
      }
    }
    catch (Exception exception) {
      Noesis.Error.UnhandledException(exception);
    }
  }

  internal static Dictionary<long, CurrentStateChangingHandler> _CurrentStateChanging =
      new Dictionary<long, CurrentStateChangingHandler>();
  #endregion

  #region CurrentStateChanged
  public delegate void CurrentStateChangedHandler(object sender, VisualStateChangedEventArgs e);
  public event CurrentStateChangedHandler CurrentStateChanged {
    add {
      long ptr = swigCPtr.Handle.ToInt64();
      if (!_CurrentStateChanged.ContainsKey(ptr)) {
        _CurrentStateChanged.Add(ptr, null);

        NoesisGUI_PINVOKE.BindEvent_VisualStateGroup_CurrentStateChanged(_raiseCurrentStateChanged, swigCPtr.Handle);
      }

      _CurrentStateChanged[ptr] += value;
    }
    remove {
      long ptr = swigCPtr.Handle.ToInt64();
      if (_CurrentStateChanged.ContainsKey(ptr)) {

        _CurrentStateChanged[ptr] -= value;

        if (_CurrentStateChanged[ptr] == null) {
          NoesisGUI_PINVOKE.UnbindEvent_VisualStateGroup_CurrentStateChanged(_raiseCurrentStateChanged, swigCPtr.Handle);

          _CurrentStateChanged.Remove(ptr);
        }
      }
    }
  }

  internal delegate void RaiseCurrentStateChangedCallback(IntPtr cPtr, IntPtr sender, IntPtr e);
  private static RaiseCurrentStateChangedCallback _raiseCurrentStateChanged = RaiseCurrentStateChanged;

  [MonoPInvokeCallback(typeof(RaiseCurrentStateChangedCallback))]
  private static void RaiseCurrentStateChanged(IntPtr cPtr, IntPtr sender, IntPtr e) {
    try {
      if (Noesis.Extend.Initialized) {
        long ptr = cPtr.ToInt64();
        if (sender == IntPtr.Zero && e == IntPtr.Zero) {
          _CurrentStateChanged.Remove(ptr);
          return;
        }
        CurrentStateChangedHandler handler = null;
        if (!_CurrentStateChanged.TryGetValue(ptr, out handler)) {
          throw new InvalidOperationException("Delegate not registered for CurrentStateChanged event");
        }
        handler?.Invoke(Noesis.Extend.GetProxy(sender, false), new VisualStateChangedEventArgs(e, false));
      }
    }
    catch (Exception exception) {
      Noesis.Error.UnhandledException(exception);
    }
  }

  internal static Dictionary<long, CurrentStateChangedHandler> _CurrentStateChanged =
      new Dictionary<long, CurrentStateChangedHandler>();
  #endregion

  internal static new void ResetEvents() {
    foreach (var kv in _CurrentStateChanging) {
      IntPtr cPtr = new IntPtr(kv.Key);
      NoesisGUI_PINVOKE.UnbindEvent_VisualStateGroup_CurrentStateChanging(_raiseCurrentStateChanging, cPtr);
    }
    _CurrentStateChanging.Clear();

    foreach (var kv in _CurrentStateChanged) {
      IntPtr cPtr = new IntPtr(kv.Key);
      NoesisGUI_PINVOKE.UnbindEvent_VisualStateGroup_CurrentStateChanged(_raiseCurrentStateChanged, cPtr);
    }
    _CurrentStateChanged.Clear();
  }

  #endregion

  public VisualStateGroup() {
  }

  protected override IntPtr CreateCPtr(Type type, out bool registerExtend) {
    registerExtend = false;
    return NoesisGUI_PINVOKE.new_VisualStateGroup();
  }

  public VisualState GetCurrentState(FrameworkElement fe) {
    IntPtr cPtr = NoesisGUI_PINVOKE.VisualStateGroup_GetCurrentState(swigCPtr, FrameworkElement.getCPtr(fe));
    return (VisualState)Noesis.Extend.GetProxy(cPtr, false);
  }

  public void SetCurrentState(FrameworkElement fe, VisualState state) {
    NoesisGUI_PINVOKE.VisualStateGroup_SetCurrentState(swigCPtr, FrameworkElement.getCPtr(fe), VisualState.getCPtr(state));
  }

  public VisualState FindState(string name) {
    IntPtr cPtr = NoesisGUI_PINVOKE.VisualStateGroup_FindState(swigCPtr, name != null ? name : string.Empty);
    return (VisualState)Noesis.Extend.GetProxy(cPtr, false);
  }

  public VisualTransition FindTransition(VisualState from, VisualState to) {
    IntPtr cPtr = NoesisGUI_PINVOKE.VisualStateGroup_FindTransition(swigCPtr, VisualState.getCPtr(from), VisualState.getCPtr(to));
    return (VisualTransition)Noesis.Extend.GetProxy(cPtr, false);
  }

  public void UpdateAnimations(FrameworkElement fe, Storyboard storyboard1, Storyboard storyboard2) {
    NoesisGUI_PINVOKE.VisualStateGroup_UpdateAnimations__SWIG_0(swigCPtr, FrameworkElement.getCPtr(fe), Storyboard.getCPtr(storyboard1), Storyboard.getCPtr(storyboard2));
  }

  public void UpdateAnimations(FrameworkElement fe, Storyboard storyboard1) {
    NoesisGUI_PINVOKE.VisualStateGroup_UpdateAnimations__SWIG_1(swigCPtr, FrameworkElement.getCPtr(fe), Storyboard.getCPtr(storyboard1));
  }

  public string Name {
    get {
      IntPtr strPtr = NoesisGUI_PINVOKE.VisualStateGroup_Name_get(swigCPtr);
      string str = Noesis.Extend.StringFromNativeUtf8(strPtr);
      return str;
    }
  }

  public VisualStateCollection States {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.VisualStateGroup_States_get(swigCPtr);
      return (VisualStateCollection)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public VisualTransitionCollection Transitions {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.VisualStateGroup_Transitions_get(swigCPtr);
      return (VisualTransitionCollection)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

}

}

