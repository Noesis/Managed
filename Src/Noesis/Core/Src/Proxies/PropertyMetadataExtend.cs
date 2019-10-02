using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Noesis
{
    /// <summary>
    /// Represents the callback that is invoked when the effective property value of a dependency
    /// property changes.
    /// </summary>
    public delegate void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e);

    /// <summary>
    /// Provides a template for a method that is called whenever a dependency property value is
    /// being re-evaluated, or coercion is specifically requested.
    /// </summary>
    public delegate object CoerceValueCallback(DependencyObject d, object baseValue);

    public partial class PropertyMetadata
    {
        public PropertyMetadata() :
            this(Noesis_PropertyMetadata_Create(), true)
        {
        }

        public PropertyMetadata(PropertyChangedCallback propertyChangedCallback) :
            this()
        {
            this.PropertyChangedCallback = propertyChangedCallback;
        }

        public PropertyMetadata(object defaultValue) :
            this()
        {
            this.DefaultValue = defaultValue;
        }

        public PropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback) :
            this(defaultValue)
        {
            this.PropertyChangedCallback = propertyChangedCallback;
        }

        public PropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback) :
            this(defaultValue, propertyChangedCallback)
        {
            this.CoerceValueCallback = coerceValueCallback;
        }

        /// <summary>
        /// Gets or sets a reference to a PropertyChangedCallback implementation specified
        /// in this metadata.
        /// </summary>
        public PropertyChangedCallback PropertyChangedCallback
        {
            get
            {
                PropertyChangedCallback changed = null;
                _PropertyChangedCallback.TryGetValue(swigCPtr.Handle, out changed);
                return changed;
            }
            set
            {
                if (_PropertyChangedCallback.ContainsKey(swigCPtr.Handle))
                {
                    _PropertyChangedCallback.Remove(swigCPtr.Handle);
                    Noesis_PropertyMetadata_UnbindPropertyChangedCallback(swigCPtr, _changed);
                }

                if (value != null)
                {
                    Noesis_PropertyMetadata_BindPropertyChangedCallback(swigCPtr, _changed);
                    _PropertyChangedCallback.Add(swigCPtr.Handle, value);
                }
            }
        }

        /// <summary>
        /// Gets or sets a reference to a CoerceValueCallback implementation specified in this
        /// metadata.
        /// </summary>
        public CoerceValueCallback CoerceValueCallback
        {
            get
            {
                CoerceValueCallback coerce = null;
                _CoerceValueCallback.TryGetValue(swigCPtr.Handle, out coerce);
                return coerce;
            }
            set
            {
                if (_CoerceValueCallback.ContainsKey(swigCPtr.Handle))
                {
                    _CoerceValueCallback.Remove(swigCPtr.Handle);
                    Noesis_PropertyMetadata_UnbindCoerceValueCallback(swigCPtr, _coerce);
                }

                if (value != null)
                {
                    Noesis_PropertyMetadata_BindCoerceValueCallback(swigCPtr, _coerce);
                    _CoerceValueCallback.Add(swigCPtr.Handle, value);
                }
            }
        }

        #region PropertyChangedCallback management
        protected delegate void ManagedPropertyChangedCallback(IntPtr cPtr, IntPtr sender, IntPtr e);
        private static ManagedPropertyChangedCallback _changed = OnPropertyChanged;

        [MonoPInvokeCallback(typeof(ManagedPropertyChangedCallback))]
        protected static void OnPropertyChanged(IntPtr cPtr, IntPtr d, IntPtr e)
        {
            try
            {
                if (!_PropertyChangedCallback.ContainsKey(cPtr))
                {
                    throw new Exception("PropertyChangedCallback not found");
                }
                if (d == IntPtr.Zero && e == IntPtr.Zero)
                {
                    _PropertyChangedCallback.Remove(cPtr);
                    return;
                }
                if (Noesis.Extend.Initialized)
                {
                    PropertyChangedCallback callback = _PropertyChangedCallback[cPtr];
                    if (callback != null)
                    {
                        callback((DependencyObject)Noesis.Extend.GetProxy(d, false),
                            new DependencyPropertyChangedEventArgs(e, false));
                    }
                }
            }
            catch (Exception exception)
            {
                Error.UnhandledException(exception);
            }
        }

        static protected Dictionary<IntPtr, PropertyChangedCallback> _PropertyChangedCallback =
            new Dictionary<IntPtr, PropertyChangedCallback>();

        #endregion

        #region CoerceValueCallback management
        protected delegate IntPtr ManagedCoerceValueCallback(IntPtr cPtr, IntPtr d, IntPtr baseValue);
        private static ManagedCoerceValueCallback _coerce = OnCoerceValue;

        [MonoPInvokeCallback(typeof(ManagedCoerceValueCallback))]
        protected static IntPtr OnCoerceValue(IntPtr cPtr, IntPtr d, IntPtr baseValue)
        {
            try
            {
                if (!_CoerceValueCallback.ContainsKey(cPtr))
                {
                    throw new Exception("CoerceValueCallback not found");
                }
                if (d == IntPtr.Zero && baseValue == IntPtr.Zero)
                {
                    _CoerceValueCallback.Remove(cPtr);
                    return IntPtr.Zero;
                }
                if (Noesis.Extend.Initialized)
                {
                    CoerceValueCallback callback = _CoerceValueCallback[cPtr];
                    if (callback != null)
                    {
                        object coercedValue = callback(
                            (DependencyObject)Noesis.Extend.GetProxy(d, false),
                            Noesis.Extend.GetProxy(baseValue, false));

                        HandleRef handle = Noesis.Extend.GetInstanceHandle(coercedValue);
                        BaseComponent.AddReference(handle.Handle); // released by native bindings

                        return handle.Handle;
                    }
                }
            }
            catch (Exception exception)
            {
                Error.UnhandledException(exception);
            }

            return IntPtr.Zero;
        }

        static protected Dictionary<IntPtr, CoerceValueCallback> _CoerceValueCallback =
            new Dictionary<IntPtr, CoerceValueCallback>();

        #endregion

        internal static void ClearCallbacks()
        {
            _PropertyChangedCallback.Clear();
            _CoerceValueCallback.Clear();
        }

        #region Imports

        static PropertyMetadata()
        {
            Noesis.GUI.Init();
        }

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_PropertyMetadata_Create();

        [DllImport(Library.Name)]
        private static extern void Noesis_PropertyMetadata_BindPropertyChangedCallback(
            HandleRef cPtr, ManagedPropertyChangedCallback callback);

        [DllImport(Library.Name)]
        private static extern void Noesis_PropertyMetadata_UnbindPropertyChangedCallback(
            HandleRef cPtr, ManagedPropertyChangedCallback callback);

        [DllImport(Library.Name)]
        private static extern void Noesis_PropertyMetadata_BindCoerceValueCallback(
            HandleRef cPtr, ManagedCoerceValueCallback callback);

        [DllImport(Library.Name)]
        private static extern void Noesis_PropertyMetadata_UnbindCoerceValueCallback(
            HandleRef cPtr, ManagedCoerceValueCallback callback);

        #endregion
    }

}
