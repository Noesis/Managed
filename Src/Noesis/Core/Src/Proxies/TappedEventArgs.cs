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

public class TappedEventArgs : TouchEventArgs {
  private HandleRef swigCPtr;

  internal TappedEventArgs(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) {
    swigCPtr = new HandleRef(this, cPtr);
  }

  internal static HandleRef getCPtr(TappedEventArgs obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  ~TappedEventArgs() {
    Dispose();
  }

  public override void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          NoesisGUI_PINVOKE.delete_TappedEventArgs(swigCPtr);
        }
        swigCPtr = new HandleRef(null, IntPtr.Zero);
      }
      GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

  internal static new void InvokeHandler(Delegate handler, IntPtr sender, IntPtr args) {
    TappedEventHandler handler_ = (TappedEventHandler)handler;
    if (handler_ != null) {
      handler_(Extend.GetProxy(sender, false), new TappedEventArgs(args, false));
    }
  }

  public TappedEventArgs(object source, RoutedEvent arg1, Point p, ulong device) : this(NoesisGUI_PINVOKE.new_TappedEventArgs(Noesis.Extend.GetInstanceHandle(source), RoutedEvent.getCPtr(arg1), ref p, device), true) {
  }

}

}

