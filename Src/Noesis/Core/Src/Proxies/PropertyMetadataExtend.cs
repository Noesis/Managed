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
                _PropertyChangedCallback.TryGetValue(swigCPtr.Handle.ToInt64(), out changed);
                return changed;
            }
            set
            {
                long ptr = swigCPtr.Handle.ToInt64();
                if (_PropertyChangedCallback.ContainsKey(ptr))
                {
                    _PropertyChangedCallback.Remove(ptr);
                    Noesis_PropertyMetadata_UnbindPropertyChangedCallback(swigCPtr, _changed);
                }

                if (value != null)
                {
                    Noesis_PropertyMetadata_BindPropertyChangedCallback(swigCPtr, _changed);
                    _PropertyChangedCallback.Add(ptr, value);
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
                _CoerceValueCallback.TryGetValue(swigCPtr.Handle.ToInt64(), out coerce);
                return coerce;
            }
            set
            {
                long ptr = swigCPtr.Handle.ToInt64();
                if (_CoerceValueCallback.ContainsKey(ptr))
                {
                    _CoerceValueCallback.Remove(ptr);
                    Noesis_PropertyMetadata_UnbindCoerceValueCallback(swigCPtr, _coerce);
                }

                if (value != null)
                {
                    Noesis_PropertyMetadata_BindCoerceValueCallback(swigCPtr, _coerce);
                    _CoerceValueCallback.Add(ptr, value);
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
                PropertyChangedCallback callback;
                long ptr = cPtr.ToInt64();
                if (!_PropertyChangedCallback.TryGetValue(ptr, out callback))
                {
                    throw new Exception("PropertyChangedCallback not found");
                }
                if (d == IntPtr.Zero && e == IntPtr.Zero)
                {
                    _PropertyChangedCallback.Remove(ptr);
                    return;
                }
                if (Noesis.Extend.Initialized && callback != null)
                {
                    DependencyObject sender = (DependencyObject)Noesis.Extend.GetProxy(d, false);
                    if (sender != null)
                    {
                        callback(sender, new DependencyPropertyChangedEventArgs(e, false));
                    }
                }
            }
            catch (Exception exception)
            {
                Error.UnhandledException(exception);
            }
        }

        static protected Dictionary<long, PropertyChangedCallback> _PropertyChangedCallback =
            new Dictionary<long, PropertyChangedCallback>();

        #endregion

        #region CoerceValueCallback management
        protected delegate IntPtr ManagedCoerceValueCallback(IntPtr cPtr, IntPtr d, IntPtr baseValue);
        private static ManagedCoerceValueCallback _coerce = OnCoerceValue;

        [MonoPInvokeCallback(typeof(ManagedCoerceValueCallback))]
        protected static IntPtr OnCoerceValue(IntPtr cPtr, IntPtr d, IntPtr baseValue)
        {
            try
            {
                CoerceValueCallback callback;
                long ptr = cPtr.ToInt64();
                if (!_CoerceValueCallback.TryGetValue(ptr, out callback))
                {
                    throw new Exception("CoerceValueCallback not found");
                }
                if (d == IntPtr.Zero && baseValue == IntPtr.Zero)
                {
                    _CoerceValueCallback.Remove(ptr);
                    return IntPtr.Zero;
                }
                if (Noesis.Extend.Initialized && callback != null)
                {
                    DependencyObject sender = (DependencyObject)Noesis.Extend.GetProxy(d, false);
                    if (sender != null)
                    {
                        object coercedValue = callback(sender, Noesis.Extend.GetProxy(baseValue, false));

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

            BaseComponent.AddReference(baseValue); // released by native bindings
            return baseValue;
        }

        static protected Dictionary<long, CoerceValueCallback> _CoerceValueCallback =
            new Dictionary<long, CoerceValueCallback>();

        #endregion

        internal static void ClearCallbacks()
        {
            _PropertyChangedCallback.Clear();
            _CoerceValueCallback.Clear();
        }

        #region Imports

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
