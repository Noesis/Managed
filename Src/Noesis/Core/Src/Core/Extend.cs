using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace Noesis
{
    ////////////////////////////////////////////////////////////////////////////////////////////////
    // Manages Noesis Extensibility
    ////////////////////////////////////////////////////////////////////////////////////////////////
    internal partial class Extend
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool Initialized { get; internal set; }

        public static void Init()
        {
            Noesis_EnableExtend(true);

            Initialized = true;

            RegisterNativeTypes();

#if UNITY_EDITOR
            AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
#endif
        }

#if UNITY_EDITOR
        private static void OnDomainUnload(object sender, System.EventArgs e)
        {
            Shutdown();
        }
#endif

        public static void Shutdown()
        {
            if (Initialized)
            {
#if UNITY_EDITOR
                AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
#endif

                try
                {
                    Initialized = false;

                    EventHandlerStore.Clear();

                    Noesis.GUI.SetApplicationResources(null);

                    var pendingReleases = new List<KeyValuePair<long, WeakReference>>();

                    pendingReleases.AddRange(_proxies);

                    foreach (var kv in _extends)
                    {
                        pendingReleases.Add(new KeyValuePair<long, WeakReference>(kv.Key, kv.Value.weak));

                        kv.Value.instance = null;
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // NOTE: During release of pending objects new proxies could be created and those
                    // references will show as leaks when Noesis shuts down.
                    // TODO: Find a way to track those objects and correctly release them.

                    foreach (var kv in pendingReleases)
                    {
                        object instance = kv.Value.Target;
                        if (instance != null)
                        {
                            BaseComponent.ForceRelease(instance, new IntPtr(kv.Key));
                        }
                    }

                    ReleasePending();

                    Noesis_ClearExtendTypes();
                    Noesis_EnableExtend(false);
                }
                catch (Exception)
                {
                    // clear errors generated releasing all C# proxies, throwing during
                    // assembly unload will close Unity without notifying
                }

                ClearTables();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void RegisterCallbacks()
        {
            // register callbacks
            Noesis_RegisterReflectionCallbacks(

                _freeString,

                _registerType,

                _toString,
                _equals,

                _visualChildrenCount,
                _visualGetChild,

                _uiElementRender,

                _frameworkElementConnectEvent,
                _frameworkElementConnectField,
                _frameworkElementMeasure,
                _frameworkElementArrange,
                _frameworkElementApplyTemplate,

                _itemsControlGetContainer,
                _itemsControlIsContainer,

                _adornerGetTransform,

                _freezableClone,

                _shapeGetGeometry,

                _commandCanExecute,
                _commandExecute,

                _converterConvert,
                _converterConvertBack,
                _multiConverterConvert,
                _multiConverterConvertBack,

                _listCount,
                _listGet,
                _listSet,
                _listAdd,
                _listInsert,
                _listIndexOf,
                _listRemoveAt,
                _listClear,

                _dictionaryFind,
                _dictionarySet,
                _dictionaryAdd,
                _dictionaryRemove,

                _listIndexerTryGet,
                _listIndexerTrySet,

                _dictionaryIndexerTryGet,
                _dictionaryIndexerTrySet,

                _selectTemplate,

                _streamSetPosition,
                _streamGetPosition,
                _streamGetLength,
                _streamRead,
                _streamClose,

                _providerLoadXaml,
                _providerTextureInfo,
                _providerTextureLoad,
                _providerTextureOpen,
                _providerMatchFont,
                _providerFamilyExists,
                _providerScanFolder,
                _providerOpenFont,

                _scrollInfoBringIntoView,
                _scrollInfoGetCanHorizontalScroll,
                _scrollInfoSetCanHorizontalScroll,
                _scrollInfoGetCanVerticalScroll,
                _scrollInfoSetCanVerticalScroll,
                _scrollInfoGetExtentWidth,
                _scrollInfoGetExtentHeight,
                _scrollInfoGetViewportWidth,
                _scrollInfoGetViewportHeight,
                _scrollInfoGetHorizontalOffset,
                _scrollInfoGetVerticalOffset,
                _scrollInfoGetScrollOwner,
                _scrollInfoSetScrollOwner,
                _scrollInfoLineLeft,
                _scrollInfoLineRight,
                _scrollInfoLineUp,
                _scrollInfoLineDown,
                _scrollInfoPageLeft,
                _scrollInfoPageRight,
                _scrollInfoPageUp,
                _scrollInfoPageDown,
                _scrollInfoMouseWheelLeft,
                _scrollInfoMouseWheelRight,
                _scrollInfoMouseWheelUp,
                _scrollInfoMouseWheelDown,
                _scrollInfoSetHorizontalOffset,
                _scrollInfoSetVerticalOffset,
                _scrollInfoMakeVisible,

                _markupExtensionProvideValue,

                _animationTimelineGetType,
                _animationTimelineGetValue,
                _booleanAnimationGetValue,
                _int16AnimationGetValue,
                _int32AnimationGetValue,
                _int64AnimationGetValue,
                _doubleAnimationGetValue,
                _colorAnimationGetValue,
                _pointAnimationGetValue,
                _rectAnimationGetValue,
                _sizeAnimationGetValue,
                _thicknessAnimationGetValue,
                _objectAnimationGetValue,
                _stringAnimationGetValue,
                _matrixAnimationGetValue,

                _getPropertyValue_Bool,
                _getPropertyValue_Float,
                _getPropertyValue_Double,
                _getPropertyValue_Int64,
                _getPropertyValue_UInt64,
                _getPropertyValue_Int,
                _getPropertyValue_UInt,
                _getPropertyValue_Short,
                _getPropertyValue_UShort,
                _getPropertyValue_String,
                _getPropertyValue_Uri,
                _getPropertyValue_Color,
                _getPropertyValue_Point,
                _getPropertyValue_Rect,
                _getPropertyValue_Int32Rect,
                _getPropertyValue_Size,
                _getPropertyValue_Thickness,
                _getPropertyValue_CornerRadius,
                _getPropertyValue_TimeSpan,
                _getPropertyValue_Duration,
                _getPropertyValue_KeyTime,
                _getPropertyValue_Type,
                _getPropertyValue_BaseComponent,

                _setPropertyValue_Bool,
                _setPropertyValue_Float,
                _setPropertyValue_Double,
                _setPropertyValue_Int64,
                _setPropertyValue_UInt64,
                _setPropertyValue_Int,
                _setPropertyValue_UInt,
                _setPropertyValue_Short,
                _setPropertyValue_UShort,
                _setPropertyValue_String,
                _setPropertyValue_Uri,
                _setPropertyValue_Color,
                _setPropertyValue_Point,
                _setPropertyValue_Rect,
                _setPropertyValue_Int32Rect,
                _setPropertyValue_Size,
                _setPropertyValue_Thickness,
                _setPropertyValue_CornerRadius,
                _setPropertyValue_TimeSpan,
                _setPropertyValue_Duration,
                _setPropertyValue_KeyTime,
                _setPropertyValue_Type,
                _setPropertyValue_BaseComponent,

                _createInstance,
                _deleteInstance,
                _grabInstance);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void UnregisterCallbacks()
        {
            // unregister callbacks
            Noesis_RegisterReflectionCallbacks(
                null,
                null,
                null, null,
                null, null,
                null,
                null, null, null, null, null,
                null, null,
                null,
                null,
                null,
                null, null,
                null, null, null, null,
                null, null, null, null, null, null, null, null,
                null, null, null, null,
                null, null,
                null, null,
                null,
                null, null, null, null, null,
                null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static void ClearTables()
        {
            _nativeTypes.Clear();
            _managedTypes.Clear();
            _extends.Clear();
            _proxies.Clear();
            _pendingRelease.Clear();

            PropertyMetadata.ClearCallbacks();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        #region Native type info

        public enum NativeTypeKind
        {
            Basic,
            Boxed,
            Component,
            Extended
        }

        public class NativeTypeInfo
        {
            public NativeTypeKind Kind { get; private set; }
            public System.Type Type { get; private set; }

            public NativeTypeInfo(NativeTypeKind kind, System.Type type)
            {
                Kind = kind;
                Type = type;
            }
        }

        public class NativeTypeComponentInfo: NativeTypeInfo
        {
            public Func<IntPtr, bool, BaseComponent> Creator { get; private set; }

            public NativeTypeComponentInfo(NativeTypeKind kind, System.Type type, Func<IntPtr, bool, BaseComponent> creator)
                : base(kind, type)
            {
                Creator = creator;
            }
        }

        public interface INativeTypeExtended
        {
        }

        public class NativeTypeEnumInfo : NativeTypeInfo, INativeTypeExtended
        {
            public NativeTypeEnumInfo(NativeTypeKind kind, Type type) : base(kind, type)
            {
            }
        }

        public class NativeTypeExtendedInfo: NativeTypeInfo, INativeTypeExtended
        {
            public Func<object> Creator { get; private set; }

            public NativeTypeExtendedInfo(NativeTypeKind kind, System.Type type, Func<object> creator)
                : base(kind, type)
            {
                Creator = creator;
            }
        }

        public class NativeTypePropsInfo: NativeTypeExtendedInfo
        {
            public List<PropertyAccessor> Properties { get; private set; }

            public NativeTypePropsInfo(NativeTypeKind kind, System.Type type, Func<object> creator)
                : base(kind, type, creator)
            {
                Properties = new List<PropertyAccessor>();
            }
        }

        public class NativeTypeIndexerInfo: NativeTypePropsInfo
        {
            public IndexerAccessor Indexer { get; private set; }

            public NativeTypeIndexerInfo(NativeTypeKind kind, System.Type type, Func<object> creator, IndexerAccessor indexer)
                : base(kind, type, creator)
            {
                Indexer = indexer;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        const int TypesCapacity = 650;
        static Dictionary<long, NativeTypeInfo> _nativeTypes = new Dictionary<long, NativeTypeInfo>(TypesCapacity);
        static Dictionary<Type, IntPtr> _managedTypes = new Dictionary<Type, IntPtr>(TypesCapacity);

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static void AddNativeType(IntPtr nativeType, NativeTypeInfo info)
        {
            // Check we are calling GetStaticType on the correct native type
            NativeTypeInfo currentInfo;
            if (_nativeTypes.TryGetValue(nativeType.ToInt64(), out currentInfo))
            {
                Log.Error(string.Format("Native type already registered for type {0}, registering {1}",
                    currentInfo.Type.FullName, info.Type.FullName));
            }

            _nativeTypes[nativeType.ToInt64()] = info;

            IntPtr currentType;
            if (_managedTypes.TryGetValue(info.Type, out currentType))
            {
                Log.Error(string.Format("Native type already registered for type {0}",
                    info.Type.FullName));
            }

            _managedTypes[info.Type] = nativeType;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static IntPtr TryGetNativeType(Type type)
        {
            IntPtr nativeType;
            lock (_managedTypes)
            {
                if (_managedTypes.TryGetValue(type, out nativeType))
                {
                    return nativeType;
                }
            }

            return IntPtr.Zero;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static IntPtr GetNativeType(Type type)
        {
            IntPtr nativeType;
            if (!_managedTypes.TryGetValue(type, out nativeType))
            {
                throw new InvalidOperationException($"Native type is not registered for '{type.Name}'");
            }
            return nativeType;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static NativeTypeInfo GetNativeTypeInfo(IntPtr nativeType)
        {
            NativeTypeInfo info;
            if (!_nativeTypes.TryGetValue(nativeType.ToInt64(), out info))
            {
                // Use nativeType.BaseType until we find a registered type we can use
                IntPtr baseType = Noesis.BaseComponent.GetBaseType(nativeType);
                if (baseType != IntPtr.Zero)
                {
                    return GetNativeTypeInfo(baseType);
                }

                string typeName = StringFromNativeUtf8(Noesis_GetTypeName(nativeType));
                throw new InvalidOperationException($"Native type '{typeName}' is not registered");
            }
            return info;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        internal static object GetProxy(IntPtr nativeType, IntPtr cPtr, bool ownMemory)
        {
            if (cPtr != IntPtr.Zero)
            {
                NativeTypeInfo info = GetNativeTypeInfo(nativeType);

                switch (info.Kind)
                {
                    default:
                    case NativeTypeKind.Basic:
                        throw new InvalidOperationException(
                            string.Format("Can't get a proxy for basic type {0}",
                            info.Type.Name));

                    case NativeTypeKind.Boxed:
                        return Unbox(cPtr, ownMemory, info);

                    case NativeTypeKind.Component:
                        return GetProxyInstance(cPtr, ownMemory, info);

                    case NativeTypeKind.Extended:
                        return GetExtendInstance(cPtr, ownMemory);
                }
            }

            return null;
        }

        public static object GetProxy(IntPtr cPtr, bool ownMemory)
        {
            if (cPtr != IntPtr.Zero)
            {
                IntPtr nativeType = Noesis.BaseComponent.GetDynamicType(cPtr);
                NativeTypeInfo info = GetNativeTypeInfo(nativeType);

                switch (info.Kind)
                {
                    default:
                    case NativeTypeKind.Basic:
                        throw new InvalidOperationException(
                            string.Format("Can't get a proxy for basic type {0}",
                            info.Type.Name));

                    case NativeTypeKind.Boxed:
                        return Unbox(cPtr, ownMemory, info);

                    case NativeTypeKind.Component:
                        return GetProxyInstance(cPtr, ownMemory, info);

                    case NativeTypeKind.Extended:
                        return GetExtendInstance(cPtr, ownMemory);
                }
            }

            return null;
        }

        public static object Initialize(object instance)
        {
            DependencyObject dob = instance as DependencyObject;
            if (dob != null && !(dob is FrameworkElement))
            {
                dob.InitObject();
            }

            return instance;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void RegisterNativeTypes()
        {
            IntPtr[] types = new IntPtr[TypesCapacity];
            for (int t = 0; t < TypesCapacity; ++t) types[t] = IntPtr.Zero;

            int numTypes = Noesis_GetNativeTypes(types, TypesCapacity);

            int i = 0;

            // NOTE: Must keep in sync with NoesisNativeTypes.i in SWIGNoesis tool

            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Type)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(string)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(bool)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(float)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(double)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(int)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(uint)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(short)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(ushort)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(long)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(ulong)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Color)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Point)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Rect)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Int32Rect)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Size)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Thickness)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(CornerRadius)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(GridLength)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Duration)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(KeyTime)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(TimeSpan)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(VirtualizationCacheLength)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Matrix)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Matrix3D)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Matrix4)));

            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(bool?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(float?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(double?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(int?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(uint?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(short?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(ushort?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(long?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(ulong?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Color?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Point?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Rect?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Int32Rect?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Size?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Thickness?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(CornerRadius?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Duration?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(KeyTime?)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(TimeSpan?)));

            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(AlignmentX)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(AlignmentY)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(AutoToolTipPlacement)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(BindingMode)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(BitmapScalingMode)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(BrushMappingMode)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(CharacterCasing)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(ClickMode)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(ColorInterpolationMode)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Dock)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(ExpandDirection)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(FillRule)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(FlowDirection)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(FontStretch)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(FontStyle)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(FontWeight)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(GeometryCombineMode)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(GradientSpreadMethod)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(HorizontalAlignment)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(MouseAction)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Key)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(ModifierKeys)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(KeyboardNavigationMode)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(LineStackingStrategy)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(ListSortDirection)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(MenuItemRole)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Orientation)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(OverflowMode)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(PenLineCap)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(PenLineJoin)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(PlacementMode)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(PopupAnimation)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(RelativeSourceMode)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(SelectionMode)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(ScrollBarVisibility)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Stretch)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(StretchDirection)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(TextDecorations)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(TextAlignment)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(TextTrimming)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(TextWrapping)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(TickBarPlacement)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(TickPlacement)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(TileMode)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(VerticalAlignment)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Visibility)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(ClockState)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(EasingMode)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(SlipBehavior)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(FillBehavior)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(GridViewColumnHeaderRole)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(HandoffBehavior)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(PanningMode)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(UpdateSourceTrigger)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(ScrollUnit)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(VirtualizationMode)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(VirtualizationCacheLengthUnit)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(PPAAMode)));

            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Type>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<string>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<bool>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<float>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<double>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<int>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<uint>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<short>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<ushort>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<long>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<ulong>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Color>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Point>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Rect>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Int32Rect>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Size>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Thickness>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<CornerRadius>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<GridLength>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Duration>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<KeyTime>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<System.TimeSpan>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<VirtualizationCacheLength>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Boxed<Matrix>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Boxed<Matrix3D>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Basic, typeof(Boxed<Matrix4>)));

            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Enum>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<AlignmentX>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<AlignmentY>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<AutoToolTipPlacement>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<BindingMode>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<BitmapScalingMode>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<BrushMappingMode>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<CharacterCasing>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<ClickMode>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<ColorInterpolationMode>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Dock>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<ExpandDirection>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<FillRule>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<FlowDirection>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<FontStretch>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<FontStyle>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<FontWeight>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<GeometryCombineMode>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<GradientSpreadMethod>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<HorizontalAlignment>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<MouseAction>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Key>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<ModifierKeys>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<KeyboardNavigationMode>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<LineStackingStrategy>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<ListSortDirection>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<MenuItemRole>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Orientation>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<OverflowMode>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<PenLineCap>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<PenLineJoin>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<PlacementMode>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<PopupAnimation>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<RelativeSourceMode>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<SelectionMode>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<ScrollBarVisibility>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Stretch>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<StretchDirection>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<TextDecorations>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<TextAlignment>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<TextTrimming>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<TextWrapping>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<TickBarPlacement>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<TickPlacement>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<TileMode>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<VerticalAlignment>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Visibility>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<ClockState>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<EasingMode>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<SlipBehavior>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<FillBehavior>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<GridViewColumnHeaderRole>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<HandoffBehavior>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<PanningMode>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<UpdateSourceTrigger>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<ScrollUnit>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<VirtualizationMode>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<VirtualizationCacheLengthUnit>)));
            AddNativeType(types[i++], new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<PPAAMode>)));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BaseComponent), BaseComponent.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DispatcherObject), DispatcherObject.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DependencyObject), DependencyObject.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DependencyProperty), DependencyProperty.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Freezable), Freezable.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PropertyMetadata), PropertyMetadata.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Expression), Expression.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(View), View.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Renderer), Renderer.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Adorner), Adorner.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(AdornerLayer), AdornerLayer.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(AdornerDecorator), AdornerDecorator.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Animatable), Animatable.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ArcSegment), ArcSegment.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BindingBase), null));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BindingExpressionBase), BindingExpressionBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BindingExpression), BindingExpression.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Bold), Bold.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ButtonBase), ButtonBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DefinitionBase), DefinitionBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(MenuBase), MenuBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SetterBase), SetterBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SetterBaseCollection), SetterBaseCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TextBoxBase), TextBoxBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TriggerBase), TriggerBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BezierSegment), BezierSegment.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Binding), Binding.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BindingCollection), BindingCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BitmapImage), BitmapImage.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BitmapSource), BitmapSource.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Border), Border.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Brush), Brush.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BrushShader), null));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BulletDecorator), BulletDecorator.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Button), Button.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Canvas), Canvas.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(CheckBox), CheckBox.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BaseUICollection), BaseUICollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(CollectionView), CollectionView.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(CollectionViewSource), CollectionViewSource.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ColumnDefinition), ColumnDefinition.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ColumnDefinitionCollection), ColumnDefinitionCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(CombinedGeometry), CombinedGeometry.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ComboBox), ComboBox.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ComboBoxItem), ComboBoxItem.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(CommandBinding), CommandBinding.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(CommandBindingCollection), CommandBindingCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(CompositeTransform), CompositeTransform.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(CompositeTransform3D), CompositeTransform3D.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Condition), Condition.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ConditionCollection), ConditionCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ContentControl), ContentControl.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ContentPresenter), ContentPresenter.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ContextMenu), ContextMenu.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ContextMenuService), null));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Control), Control.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ControlTemplate), ControlTemplate.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(CroppedBitmap), CroppedBitmap.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Cursor), Cursor.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DashStyle), DashStyle.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DataTemplate), DataTemplate.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DataTemplateSelector), DataTemplateSelector.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DataTrigger), DataTrigger.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(HierarchicalDataTemplate), HierarchicalDataTemplate.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Decorator), Decorator.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DockPanel), DockPanel.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DrawingContext), DrawingContext.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Effect), Effect.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BlurEffect), BlurEffect.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DropShadowEffect), DropShadowEffect.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ShaderEffect), null));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Ellipse), Ellipse.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(EllipseGeometry), EllipseGeometry.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(EventTrigger), EventTrigger.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Expander), Expander.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(FontFamily), FontFamily.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(FormattedText), FormattedText.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(FrameworkElement), FrameworkElement.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(FrameworkTemplate), FrameworkTemplate.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(FrameworkPropertyMetadata), FrameworkPropertyMetadata.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BaseFreezableCollection), BaseFreezableCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Geometry), Geometry.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(GeometryCollection), GeometryCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(GeometryGroup), GeometryGroup.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(GradientBrush), GradientBrush.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(GradientStop), GradientStop.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(GradientStopCollection), GradientStopCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Grid), Grid.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(GridSplitter), GridSplitter.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(GroupBox), GroupBox.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(HeaderedContentControl), HeaderedContentControl.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(HeaderedItemsControl), HeaderedItemsControl.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Hyperlink), Hyperlink.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Image), Image.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ImageBrush), ImageBrush.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ImageSource), ImageSource.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Inline), Inline.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(InlineCollection), InlineCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(InlineUIContainer), InlineUIContainer.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(InputBinding), InputBinding.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(InputBindingCollection), InputBindingCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(InputGesture), InputGesture.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(InputGestureCollection), InputGestureCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Italic), Italic.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ItemCollection), ItemCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ItemContainerGenerator), ItemContainerGenerator.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ItemsControl), ItemsControl.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ItemsPanelTemplate), ItemsPanelTemplate.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ItemsPresenter), ItemsPresenter.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(KeyBinding), KeyBinding.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Keyboard), Keyboard.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(KeyboardNavigation), KeyboardNavigation.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(KeyGesture), KeyGesture.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Label), Label.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Line), Line.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(LinearGradientBrush), LinearGradientBrush.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(LineBreak), LineBreak.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(LineGeometry), LineGeometry.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(LineSegment), LineSegment.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ListBox), ListBox.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ListBoxItem), ListBoxItem.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(MarkupExtension), null));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(MatrixTransform), MatrixTransform.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(MatrixTransform3D), MatrixTransform3D.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Menu), Menu.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(MenuItem), MenuItem.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(MouseBinding), MouseBinding.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(MouseGesture), MouseGesture.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(MultiBinding), MultiBinding.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(MultiBindingExpression), MultiBindingExpression.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(MultiDataTrigger), MultiDataTrigger.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(MultiTrigger), MultiTrigger.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(NameScope), NameScope.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Page), Page.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Panel), Panel.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PasswordBox), PasswordBox.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Path), Path.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PathFigure), PathFigure.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PathFigureCollection), PathFigureCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PathGeometry), PathGeometry.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PathSegment), PathSegment.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PathSegmentCollection), PathSegmentCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Pen), Pen.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PointCollection), PointCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PolyBezierSegment), PolyBezierSegment.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PolyLineSegment), PolyLineSegment.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PolyQuadraticBezierSegment), PolyQuadraticBezierSegment.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Popup), Popup.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ProgressBar), ProgressBar.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PropertyPath), PropertyPath.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(QuadraticBezierSegment), QuadraticBezierSegment.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RadialGradientBrush), RadialGradientBrush.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RadioButton), RadioButton.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RangeBase), RangeBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Rectangle), Rectangle.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RectangleGeometry), RectangleGeometry.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RelativeSource), RelativeSource.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RenderOptions), null));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RepeatButton), RepeatButton.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ResourceDictionary), ResourceDictionary.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ResourceDictionaryCollection), ResourceDictionaryCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RotateTransform), RotateTransform.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RoutedCommand), RoutedCommand.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RoutedEvent), RoutedEvent.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RoutedUICommand), RoutedUICommand.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RowDefinition), RowDefinition.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RowDefinitionCollection), RowDefinitionCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Run), Run.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ScaleTransform), ScaleTransform.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ScrollBar), ScrollBar.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ScrollViewer), ScrollViewer.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ScrollContentPresenter), ScrollContentPresenter.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Selector), Selector.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Separator), Separator.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Setter), Setter.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Shape), Shape.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SkewTransform), SkewTransform.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Slider), Slider.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SolidColorBrush), SolidColorBrush.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Span), Span.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(StackPanel), StackPanel.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(StatusBar), StatusBar.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(StatusBarItem), StatusBarItem.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(StreamGeometry), StreamGeometry.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Style), Style.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TabControl), TabControl.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TabItem), TabItem.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TabPanel), TabPanel.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TemplateBindingExpression), TemplateBindingExpression.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TemplateBindingExtension), TemplateBindingExtension.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TextBlock), TextBlock.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TextBox), TextBox.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TextElement), TextElement.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TextureSource), TextureSource.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DynamicTextureSource), DynamicTextureSource.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Thumb), Thumb.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TickBar), TickBar.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TileBrush), TileBrush.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ToggleButton), ToggleButton.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ToolBar), ToolBar.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ToolBarOverflowPanel), ToolBarOverflowPanel.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ToolBarPanel), ToolBarPanel.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ToolBarTray), ToolBarTray.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ToolTip), ToolTip.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ToolTipService), null));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Track), Track.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TransformGroup), TransformGroup.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TranslateTransform), TranslateTransform.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Transform), Transform.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Transform3D), Transform3D.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TransformCollection), TransformCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TreeView), TreeView.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TreeViewItem), TreeViewItem.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TriggerAction), TriggerAction.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TriggerActionCollection), TriggerActionCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Trigger), Trigger.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TriggerCollection), TriggerCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(UIElement), UIElement.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(UIElementCollection), UIElementCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(UIPropertyMetadata), UIPropertyMetadata.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Underline), Underline.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(UniformGrid), UniformGrid.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(UserControl), UserControl.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Viewbox), Viewbox.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(VirtualizingPanel), VirtualizingPanel.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(VirtualizingStackPanel), VirtualizingStackPanel.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Visual), Visual.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(VisualBrush), VisualBrush.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(VisualCollection), VisualCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(WrapPanel), WrapPanel.CreateProxy));


            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Clock), Clock.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ClockGroup), ClockGroup.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(AnimationClock), AnimationClock.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Timeline), Timeline.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TimelineCollection), TimelineCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(TimelineGroup), TimelineGroup.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ParallelTimeline), ParallelTimeline.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(AnimationTimeline), AnimationTimeline.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Storyboard), Storyboard.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BooleanAnimationBase), BooleanAnimationBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Int16AnimationBase), Int16AnimationBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Int32AnimationBase), Int32AnimationBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Int64AnimationBase), Int64AnimationBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DoubleAnimationBase), DoubleAnimationBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ColorAnimationBase), ColorAnimationBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PointAnimationBase), PointAnimationBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RectAnimationBase), RectAnimationBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SizeAnimationBase), SizeAnimationBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ThicknessAnimationBase), ThicknessAnimationBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ObjectAnimationBase), ObjectAnimationBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(StringAnimationBase), StringAnimationBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(MatrixAnimationBase), MatrixAnimationBase.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Int16Animation), Int16Animation.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Int32Animation), Int32Animation.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Int64Animation), Int64Animation.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DoubleAnimation), DoubleAnimation.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ColorAnimation), ColorAnimation.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PointAnimation), PointAnimation.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RectAnimation), RectAnimation.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SizeAnimation), SizeAnimation.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ThicknessAnimation), ThicknessAnimation.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BooleanAnimationUsingKeyFrames), BooleanAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Int16AnimationUsingKeyFrames), Int16AnimationUsingKeyFrames.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Int32AnimationUsingKeyFrames), Int32AnimationUsingKeyFrames.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Int64AnimationUsingKeyFrames), Int64AnimationUsingKeyFrames.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DoubleAnimationUsingKeyFrames), DoubleAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ColorAnimationUsingKeyFrames), ColorAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PointAnimationUsingKeyFrames), PointAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RectAnimationUsingKeyFrames), RectAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SizeAnimationUsingKeyFrames), SizeAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ThicknessAnimationUsingKeyFrames), ThicknessAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ObjectAnimationUsingKeyFrames), ObjectAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(StringAnimationUsingKeyFrames), StringAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(MatrixAnimationUsingKeyFrames), MatrixAnimationUsingKeyFrames.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BooleanKeyFrameCollection), BooleanKeyFrameCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Int16KeyFrameCollection), Int16KeyFrameCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Int32KeyFrameCollection), Int32KeyFrameCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Int64KeyFrameCollection), Int64KeyFrameCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DoubleKeyFrameCollection), DoubleKeyFrameCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ColorKeyFrameCollection), ColorKeyFrameCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PointKeyFrameCollection), PointKeyFrameCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RectKeyFrameCollection), RectKeyFrameCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SizeKeyFrameCollection), SizeKeyFrameCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ThicknessKeyFrameCollection), ThicknessKeyFrameCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ObjectKeyFrameCollection), ObjectKeyFrameCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(StringKeyFrameCollection), StringKeyFrameCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(MatrixKeyFrameCollection), MatrixKeyFrameCollection.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BooleanKeyFrame), BooleanKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Int16KeyFrame), Int16KeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Int32KeyFrame), Int32KeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Int64KeyFrame), Int64KeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DoubleKeyFrame), DoubleKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ColorKeyFrame), ColorKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PointKeyFrame), PointKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RectKeyFrame), RectKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SizeKeyFrame), SizeKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ThicknessKeyFrame), ThicknessKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(StringKeyFrame), StringKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(MatrixKeyFrame), MatrixKeyFrame.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DiscreteBooleanKeyFrame), DiscreteBooleanKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DiscreteInt16KeyFrame), DiscreteInt16KeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DiscreteInt32KeyFrame), DiscreteInt32KeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DiscreteInt64KeyFrame), DiscreteInt64KeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DiscreteDoubleKeyFrame), DiscreteDoubleKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DiscreteColorKeyFrame), DiscreteColorKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DiscretePointKeyFrame), DiscretePointKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DiscreteRectKeyFrame), DiscreteRectKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DiscreteSizeKeyFrame), DiscreteSizeKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DiscreteThicknessKeyFrame), DiscreteThicknessKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DiscreteObjectKeyFrame), DiscreteObjectKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DiscreteStringKeyFrame), DiscreteStringKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(DiscreteMatrixKeyFrame), DiscreteMatrixKeyFrame.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(LinearInt16KeyFrame), LinearInt16KeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(LinearInt32KeyFrame), LinearInt32KeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(LinearInt64KeyFrame), LinearInt64KeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(LinearDoubleKeyFrame), LinearDoubleKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(LinearColorKeyFrame), LinearColorKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(LinearPointKeyFrame), LinearPointKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(LinearRectKeyFrame), LinearRectKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(LinearSizeKeyFrame), LinearSizeKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(LinearThicknessKeyFrame), LinearThicknessKeyFrame.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SplineInt16KeyFrame), SplineInt16KeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SplineInt32KeyFrame), SplineInt32KeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SplineInt64KeyFrame), SplineInt64KeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SplineDoubleKeyFrame), SplineDoubleKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SplineColorKeyFrame), SplineColorKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SplinePointKeyFrame), SplinePointKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SplineRectKeyFrame), SplineRectKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SplineSizeKeyFrame), SplineSizeKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SplineThicknessKeyFrame), SplineThicknessKeyFrame.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(KeySpline), KeySpline.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(EasingInt16KeyFrame), EasingInt16KeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(EasingInt32KeyFrame), EasingInt32KeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(EasingInt64KeyFrame), EasingInt64KeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(EasingDoubleKeyFrame), EasingDoubleKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(EasingColorKeyFrame), EasingColorKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(EasingPointKeyFrame), EasingPointKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(EasingRectKeyFrame), EasingRectKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(EasingSizeKeyFrame), EasingSizeKeyFrame.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(EasingThicknessKeyFrame), EasingThicknessKeyFrame.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(EasingFunctionBase), EasingFunctionBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BackEase), BackEase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BounceEase), BounceEase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(CircleEase), CircleEase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(CubicEase), CubicEase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ElasticEase), ElasticEase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ExponentialEase), ExponentialEase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PowerEase), PowerEase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(QuadraticEase), QuadraticEase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(QuarticEase), QuarticEase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(QuinticEase), QuinticEase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(SineEase), SineEase.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ControllableStoryboardAction), ControllableStoryboardAction.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(BeginStoryboard), BeginStoryboard.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(PauseStoryboard), PauseStoryboard.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ResumeStoryboard), ResumeStoryboard.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(StopStoryboard), StopStoryboard.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(VisualStateManager), VisualStateManager.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(VisualStateGroup), VisualStateGroup.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(VisualStateGroupCollection), VisualStateGroupCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(VisualTransition), VisualTransition.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(VisualTransitionCollection), VisualTransitionCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(VisualState), VisualState.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(VisualStateCollection), VisualStateCollection.CreateProxy));


            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ViewBase), ViewBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(GridView), GridView.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(GridViewColumn), GridViewColumn.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(GridViewColumnCollection), GridViewColumnCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(GridViewColumnHeader), GridViewColumnHeader.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(GridViewRowPresenterBase), GridViewRowPresenterBase.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(GridViewRowPresenter), GridViewRowPresenter.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(GridViewHeaderRowPresenter), GridViewHeaderRowPresenter.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ListView), ListView.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(ListViewItem), ListViewItem.CreateProxy));

            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RiveInput), RiveInput.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RiveInputCollection), RiveInputCollection.CreateProxy));
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RiveControl), RiveControl.CreateProxy));

            _managedTypes[typeof(NativeRenderDevice)] = types[i];
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RenderDevice), NativeRenderDevice.CreateProxy));
            _managedTypes[typeof(NativeRenderTarget)] = types[i];
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(RenderTarget), NativeRenderTarget.CreateProxy));
            _managedTypes[typeof(NativeTexture)] = types[i];
            AddNativeType(types[i++], new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Texture), NativeTexture.CreateProxy));

            _managedTypes[typeof(object)] = BaseComponent.GetStaticType();

            if (i != numTypes)
            {
                throw new InvalidOperationException("Invalid registered native types count");
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void UnregisterNativeTypes()
        {
            Shutdown();

            if (_assemblies != null)
            {
                AppDomain.CurrentDomain.AssemblyLoad -= AddLoadedAssembly;
                _assemblies = null;
            }

            Init();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static PropertyInfo[] GetPublicProperties(Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().DeclaredProperties.Where(p => p.GetMethod.IsPublic && !p.GetMethod.IsStatic).ToArray();
#else
            return type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
#endif
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static EventInfo[] GetPublicEvents(Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().DeclaredProperties.Where(p => p.GetMethod.IsPublic && !p.GetMethod.IsStatic).ToArray();
#else
            return type.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
#endif
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static PropertyInfo FindIndexer(Type type, Type paramType)
        {
#if NETFX_CORE
            var props = type.GetRuntimeProperties().Where(p => p.GetMethod.IsPublic && !p.GetMethod.IsStatic);
#else
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
#endif

            foreach (var p in props)
            {
                ParameterInfo[] indexParams = p.GetIndexParameters();
                if (indexParams.Length == 1 && indexParams[0].ParameterType.Equals(paramType))
                {
                    return p;
                }
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static PropertyInfo FindListIndexer(Type type)
        {
            return FindIndexer(type, typeof(int));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static PropertyInfo FindDictIndexer(Type type)
        {
            return FindIndexer(type, typeof(string));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [StructLayout(LayoutKind.Sequential)]
        private struct ExtendTypeData
        {
            [MarshalAs(UnmanagedType.U8)]
            public long type;
            [MarshalAs(UnmanagedType.U8)]
            public long baseType;
            [MarshalAs(UnmanagedType.U8)]
            public long typeConverter;
            [MarshalAs(UnmanagedType.U8)]
            public long contentProperty;
            [MarshalAs(UnmanagedType.U4)]
            public int overrides;
        }

        [Flags]
        private enum ExtendTypeOverrides
        {
            None                            = 0,
            Object_ToString                 = 1,
            Object_Equals                   = 2,
            Visual_GetChildrenCount         = 4,
            Visual_GetChild                 = 8,
            UIElement_OnRender              = 16,
            FrameworkElement_ConnectEvent   = 32,
            FrameworkElement_ConnectField   = 64,
            FrameworkElement_Measure        = 128,
            FrameworkElement_Arrange        = 256,
            FrameworkElement_ApplyTemplate  = 512,
            ItemsControl_GetContainer       = 1024,
            ItemsControl_IsContainer        = 2048,
            Adorner_GetTransform            = 4096,
            Freezable_Clone                 = 8192,
            Animation_GetValueCore          = 16384
        }

        private struct ExtendPropertyData
        {
            public long name;
            public long type;
            public long typeConverter;
            public int extendType;
            public int readOnly;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static IntPtr RegisterNativeType(Type type)
        {
            return RegisterNativeType(type, true);
        }

        public static IntPtr RegisterNativeType(Type type, bool registerDP)
        {
            IntPtr nativeType = IntPtr.Zero;

            if (type == null)
            {
                return IntPtr.Zero;
            }

            PropertyInfo[] props;
            NativeTypeInfo info;

            lock (_managedTypes)
            {
                if (_managedTypes.TryGetValue(type, out nativeType))
                {
                    // already registered by other thread
                    return nativeType;
                }

                if (type.GetTypeInfo().IsInterface)
                {
                    nativeType = Noesis.BaseComponent.GetStaticType();
                    _managedTypes[type] = nativeType;
                    return nativeType;
                }

                if (type.GetTypeInfo().IsEnum)
                {
                    int numEnums;
                    IntPtr enumsData = CreateNativeEnumsData(type, out numEnums);
                    try
                    {
                        nativeType = Noesis_RegisterEnumType(TypeFullName(type), numEnums, enumsData);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(enumsData);
                    }

                    AddNativeType(nativeType, new NativeTypeEnumInfo(NativeTypeKind.Basic, type));

                    return nativeType;
                }

                PropertyInfo indexerInfo = null;
                IndexerAccessor indexer = null;

                #if UNITY_5_3_OR_NEWER
                if (typeof(UnityEngine.Texture).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    nativeType = Noesis.TextureSource.Extend(TypeFullName(type));
                }
                else if (typeof(UnityEngine.Sprite).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    nativeType = Noesis.CroppedBitmap.Extend(TypeFullName(type));
                }
                else
                #endif
                if (typeof(Noesis.IScrollInfo).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()) &&
                    typeof(Noesis.VirtualizingPanel).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    nativeType = Noesis.ExtendVirtualScrollInfo.Extend(TypeFullName(type));
                }
                else if (typeof(Noesis.IScrollInfo).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()) &&
                    typeof(Noesis.Panel).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    nativeType = Noesis.ExtendScrollInfo.Extend(TypeFullName(type));
                }
                else if (type.GetTypeInfo().IsSubclassOf(typeof(Noesis.BaseComponent)))
                {
                    System.Reflection.MethodInfo extend = FindExtendMethod(type);
                    if (extend != null)
                    {
                        object[] typeName = { TypeFullName(type) };
                        nativeType = (IntPtr)extend.Invoke(null, typeName);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "Can't find Extend method in any base class for " + type.FullName);
                    }
                }
                else if (typeof(System.IO.Stream).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    nativeType = Noesis.ExtendStream.Extend(TypeFullName(type));
                }
                else if (typeof(ICommand).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    nativeType = Noesis.ExtendCommand.Extend(TypeFullName(type));
                }
                else if (typeof(Noesis.IValueConverter).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    nativeType = Noesis.ExtendConverter.Extend(TypeFullName(type));
                }
                else if (typeof(Noesis.IMultiValueConverter).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    nativeType = Noesis.ExtendMultiConverter.Extend(TypeFullName(type));
                }
                else if (typeof(System.Collections.IList).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    if (typeof(System.Collections.Specialized.INotifyCollectionChanged).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                    {
                        nativeType = Noesis.ExtendObservableList.Extend(TypeFullName(type));
                    }
                    else
                    {
                        nativeType = Noesis.ExtendList.Extend(TypeFullName(type));
                    }
                }
                else if (typeof(System.Collections.IDictionary).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    nativeType = Noesis.ExtendDictionary.Extend(TypeFullName(type));
                }
                else if ((indexerInfo = FindListIndexer(type)) != null)
                {
                    indexer = CreateIndexerAccessor<int>(indexerInfo);
                    nativeType = Noesis.ExtendListIndexer.Extend(TypeFullName(type));
                }
                else if ((indexerInfo = FindDictIndexer(type)) != null)
                {
                    indexer = CreateIndexerAccessor<string>(indexerInfo);
                    nativeType = Noesis.ExtendDictionaryIndexer.Extend(TypeFullName(type));
                }
                else
                {
                    nativeType = Noesis.BaseComponent.Extend(TypeFullName(type));
                }

                // Register native type
                props = GetPublicProperties(type);
                info = CreateNativeTypeInfo(type, indexer, props);
                AddNativeType(nativeType, info);
            }

            // Fill native type with C# public properties
            ExtendTypeData typeData = CreateNativeTypeData(type, nativeType);
            int numProps;
            IntPtr propsData = CreateNativePropsData(type, props, info, out numProps);
            try
            {
                Noesis_FillExtendType(ref typeData, numProps, propsData);
            }
            finally
            {
                Marshal.FreeHGlobal(propsData);
            }

            if (registerDP)
            {
                RegisterDependencyProperties(type);
            }

            return nativeType;
        }

        // FullName returned by mono AOT implementation is a bit different than non-AOT implementation.
        // They must be the same because those names are stored in the serialization file
        private static string TypeFullName(System.Type type)
        {
            return type.FullName.Replace("Culture=,", "Culture=neutral,");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static Func<object> TypeCreator(System.Type type)
        {
#if !ENABLE_IL2CPP && !UNITY_IOS
            if (!type.GetTypeInfo().IsValueType && Platform.ID != PlatformID.iPhone)
            {
                #if NETFX_CORE
                var ctor = type.GetTypeInfo().DeclaredConstructors.Where(c =>
                    c.GetParameters().Count() == 0 && c.IsStatic == false).FirstOrDefault();
                #else
                var ctor = type.GetTypeInfo().GetConstructor(Type.EmptyTypes);
                #endif

                if (ctor != null && !type.GetTypeInfo().IsAbstract)
                {
                    return System.Linq.Expressions.Expression.Lambda<Func<object>>(
                        System.Linq.Expressions.Expression.New(ctor)).Compile();
                }
                else
                {
                    return null;
                }
            }
            else
#endif
            {
                return () => Activator.CreateInstance(type);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static NativeTypeInfo CreateNativeTypeInfo(System.Type type, IndexerAccessor indexer,
            PropertyInfo[] props)
        {
            if (indexer != null)
            {
                return new NativeTypeIndexerInfo(NativeTypeKind.Extended, type, TypeCreator(type), indexer);
            }
            else if (props.Length > 0)
            {
                return new NativeTypePropsInfo(NativeTypeKind.Extended, type, TypeCreator(type));
            }
            else
            {
                return new NativeTypeExtendedInfo(NativeTypeKind.Extended, type, TypeCreator(type));
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static bool IsOverride(MethodInfo m)
        {
            return m != null && m.GetBaseDefinition().DeclaringType != m.DeclaringType;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static bool ParametersMatch(MethodInfo m, Type[] types)
        {
            ParameterInfo[] parameters = m.GetParameters();
            if (parameters.Length == types.Length)
            {
                for (int i = 0; i < parameters.Length; ++i)
                {
                    if (parameters[i].ParameterType != types[i]) return false;
                }
            }

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static MethodInfo FindMethod(Type type, string name, Type[] types)
        {
            #if NETFX_CORE
            return type.GetRuntimeMethods()
                .Where(m => m.Name == name && !m.IsStatic && ParametersMatch(m, types)).FirstOrDefault();
            #else
            return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.Name == name && ParametersMatch(m, types)).FirstOrDefault();
            #endif
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static PropertyInfo FindProperty(Type type, string name)
        {
            #if NETFX_CORE
            return type.GetRuntimeProperties()
                .Where(p => p.Name == name && !p.GetMethod.IsStatic).FirstOrDefault;
            #else
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.Name == name).FirstOrDefault();
            #endif
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static ExtendTypeData CreateNativeTypeData(System.Type type, IntPtr nativeType)
        {
            ExtendTypeData typeData = new ExtendTypeData();
            typeData.type = nativeType.ToInt64();
            #if UNITY_5_3_OR_NEWER
            if (typeof(UnityEngine.Texture).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                typeData.baseType = TryGetNativeType(typeof(TextureSource)).ToInt64();
            }
            else if (typeof(UnityEngine.Sprite).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                typeData.baseType = TryGetNativeType(typeof(CroppedBitmap)).ToInt64();
            }
            else
            {
                typeData.baseType = EnsureNativeType(type.GetTypeInfo().BaseType).ToInt64();
            }
            #else
                typeData.baseType = EnsureNativeType(type.GetTypeInfo().BaseType).ToInt64();
            #endif

            var typeConverter = type.GetTypeInfo().GetCustomAttribute<System.ComponentModel.TypeConverterAttribute>();
            if (typeConverter != null)
            {
                typeData.typeConverter = Marshal.StringToHGlobalAnsi(typeConverter.ConverterTypeName).ToInt64();
            }

            var contentProperty = type.GetTypeInfo().GetCustomAttribute<ContentPropertyAttribute>();
            if (contentProperty != null)
            {
                typeData.contentProperty = Marshal.StringToHGlobalAnsi(contentProperty.Name).ToInt64();
            }

            // overrides
            ExtendTypeOverrides overrides = ExtendTypeOverrides.None;

            MethodInfo toStringMethod = FindMethod(type, "ToString", new Type[] { });
            if (IsOverride(toStringMethod)) overrides |= ExtendTypeOverrides.Object_ToString;

            MethodInfo equalsMethod = FindMethod(type, "Equals", new Type[] { typeof(object) });
            if (IsOverride(equalsMethod)) overrides |= ExtendTypeOverrides.Object_Equals;

            if (typeof(Visual).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                PropertyInfo childrenCountProp = FindProperty(type, "VisualChildrenCount");
                if (IsOverride(childrenCountProp?.GetMethod)) overrides |= ExtendTypeOverrides.Visual_GetChildrenCount;

                MethodInfo getChildMethod = FindMethod(type, "GetVisualChild", new Type[] { typeof(int) });
                if (IsOverride(getChildMethod)) overrides |= ExtendTypeOverrides.Visual_GetChild;
            }

            if (typeof(UIElement).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                MethodInfo renderMethod = FindMethod(type, "OnRender", new Type[] { typeof(DrawingContext) });
                if (IsOverride(renderMethod)) overrides |= ExtendTypeOverrides.UIElement_OnRender;
            }

            if (typeof(FrameworkElement).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                MethodInfo eventMethod = FindMethod(type, "ConnectEvent", new Type[] { typeof(object), typeof(string), typeof(string) });
                if (IsOverride(eventMethod)) overrides |= ExtendTypeOverrides.FrameworkElement_ConnectEvent;

                MethodInfo fieldMethod = FindMethod(type, "ConnectField", new Type[] { typeof(object), typeof(string) });
                if (IsOverride(fieldMethod)) overrides |= ExtendTypeOverrides.FrameworkElement_ConnectField;

                MethodInfo measureMethod = FindMethod(type, "MeasureOverride", new Type[] { typeof(Size) });
                if (IsOverride(measureMethod)) overrides |= ExtendTypeOverrides.FrameworkElement_Measure;

                MethodInfo arrangeMethod = FindMethod(type, "ArrangeOverride", new Type[] { typeof(Size) });
                if (IsOverride(arrangeMethod)) overrides |= ExtendTypeOverrides.FrameworkElement_Arrange;

                MethodInfo templateMethod = FindMethod(type, "OnApplyTemplate", new Type[] { });
                if (IsOverride(templateMethod)) overrides |= ExtendTypeOverrides.FrameworkElement_ApplyTemplate;
            }

            if (typeof(ItemsControl).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                MethodInfo getContainerMethod = FindMethod(type, "GetContainerForItemOverride", new Type[] { });
                if (IsOverride(getContainerMethod)) overrides |= ExtendTypeOverrides.ItemsControl_GetContainer;

                MethodInfo isContainerMethod = FindMethod(type, "IsItemItsOwnContainerOverride", new Type[] { typeof(object) });
                if (IsOverride(isContainerMethod)) overrides |= ExtendTypeOverrides.ItemsControl_IsContainer;
            }

            if (typeof(Adorner).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                MethodInfo getTransformMethod = FindMethod(type, "GetDesiredTransform", new Type[] { typeof(Matrix4) });
                if (IsOverride(getTransformMethod)) overrides |= ExtendTypeOverrides.Adorner_GetTransform;
            }

            if (typeof(Freezable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                MethodInfo cloneMethod = FindMethod(type, "CloneCommonCore", new Type[] { typeof(Freezable) });
                if (IsOverride(cloneMethod)) overrides |= ExtendTypeOverrides.Freezable_Clone;
            }

            if (typeof(AnimationTimeline).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                if (typeof(Int16Animation).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    MethodInfo getValueMethod = FindMethod(type, "GetCurrentValueCore", new Type[] { typeof(short), typeof(short), typeof(AnimationClock) });
                    if (IsOverride(getValueMethod) && getValueMethod.DeclaringType != typeof(Int16Animation))
                        overrides |= ExtendTypeOverrides.Animation_GetValueCore;
                }
                if (typeof(Int32Animation).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    MethodInfo getValueMethod = FindMethod(type, "GetCurrentValueCore", new Type[] { typeof(int), typeof(int), typeof(AnimationClock) });
                    if (IsOverride(getValueMethod) && getValueMethod.DeclaringType != typeof(Int32Animation))
                        overrides |= ExtendTypeOverrides.Animation_GetValueCore;
                }
                if (typeof(Int64Animation).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    MethodInfo getValueMethod = FindMethod(type, "GetCurrentValueCore", new Type[] { typeof(long), typeof(long), typeof(AnimationClock) });
                    if (IsOverride(getValueMethod) && getValueMethod.DeclaringType != typeof(Int64Animation))
                        overrides |= ExtendTypeOverrides.Animation_GetValueCore;
                }
                if (typeof(DoubleAnimation).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    MethodInfo getValueMethod = FindMethod(type, "GetCurrentValueCore", new Type[] { typeof(float), typeof(float), typeof(AnimationClock) });
                    if (IsOverride(getValueMethod) && getValueMethod.DeclaringType != typeof(DoubleAnimation))
                        overrides |= ExtendTypeOverrides.Animation_GetValueCore;
                }
                if (typeof(ColorAnimation).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    MethodInfo getValueMethod = FindMethod(type, "GetCurrentValueCore", new Type[] { typeof(Color), typeof(Color), typeof(AnimationClock) });
                    if (IsOverride(getValueMethod) && getValueMethod.DeclaringType != typeof(ColorAnimation))
                        overrides |= ExtendTypeOverrides.Animation_GetValueCore;
                }
                if (typeof(PointAnimation).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    MethodInfo getValueMethod = FindMethod(type, "GetCurrentValueCore", new Type[] { typeof(Point), typeof(Point), typeof(AnimationClock) });
                    if (IsOverride(getValueMethod) && getValueMethod.DeclaringType != typeof(PointAnimation))
                        overrides |= ExtendTypeOverrides.Animation_GetValueCore;
                }
                if (typeof(RectAnimation).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    MethodInfo getValueMethod = FindMethod(type, "GetCurrentValueCore", new Type[] { typeof(Rect), typeof(Rect), typeof(AnimationClock) });
                    if (IsOverride(getValueMethod) && getValueMethod.DeclaringType != typeof(RectAnimation))
                        overrides |= ExtendTypeOverrides.Animation_GetValueCore;
                }
                if (typeof(SizeAnimation).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    MethodInfo getValueMethod = FindMethod(type, "GetCurrentValueCore", new Type[] { typeof(Size), typeof(Size), typeof(AnimationClock) });
                    if (IsOverride(getValueMethod) && getValueMethod.DeclaringType != typeof(SizeAnimation))
                        overrides |= ExtendTypeOverrides.Animation_GetValueCore;
                }
                if (typeof(ThicknessAnimation).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                {
                    MethodInfo getValueMethod = FindMethod(type, "GetCurrentValueCore", new Type[] { typeof(Thickness), typeof(Thickness), typeof(AnimationClock) });
                    if (IsOverride(getValueMethod) && getValueMethod.DeclaringType != typeof(ThicknessAnimation))
                        overrides |= ExtendTypeOverrides.Animation_GetValueCore;
                }
            }

            typeData.overrides = (int)overrides;

            return typeData;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static bool IsIndexerProperty(PropertyInfo p)
        {
            return p.GetIndexParameters().Length > 0;
        }

        private static bool HasTypeConverter(PropertyInfo p)
        {
            return p.GetCustomAttribute<System.ComponentModel.TypeConverterAttribute>() != null;
        }

        private static bool IsDependencyProperty(Type type, PropertyInfo prop)
        {
            string name = prop.Name + "Property";

            #if NETFX_CORE
            FieldInfo field = type.GetTypeInfo().DeclaredFields.Where(f => f.IsPublic && f.IsStatic && f.Name == name).FirstOrDefault();
            #else
            FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            #endif

            return field != null && field.FieldType.Equals(typeof(Noesis.DependencyProperty));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static IntPtr CreateNativePropsData(System.Type type, PropertyInfo[] props,
            NativeTypeInfo info, out int numProps)
        {
            EventInfo[] events = GetPublicEvents(type);
            int eventsLen = events.Length;

            int propSize = Marshal.SizeOf<ExtendPropertyData>();
            int propsLen = props.Length;
            IntPtr propsData = Marshal.AllocHGlobal((propsLen + eventsLen) * propSize);
            numProps = 0;

            if (propsLen > 0)
            {
#if ENABLE_IL2CPP || UNITY_IOS
                bool usePropertyInfo = true;
#else
                bool usePropertyInfo = type.GetTypeInfo().IsValueType || Platform.ID == PlatformID.iPhone;
#endif

                NativeTypePropsInfo propsInfo = (NativeTypePropsInfo)info;
                for (int i = 0; i < propsLen; ++i)
                {
                    var p = props[i];
                    if (p.GetGetMethod() != null && !p.PropertyType.IsPointer && !p.PropertyType.IsByRef &&
                        (HasTypeConverter(p) || (!IsIndexerProperty(p) && !IsDependencyProperty(type, p))))
                    {
                        ExtendPropertyData propData = AddProperty(propsInfo, p, usePropertyInfo);

                        int offset = numProps * propSize;

                        Marshal.WriteInt64(propsData, offset + 0, propData.name);
                        Marshal.WriteInt64(propsData, offset + 8, propData.type);
                        Marshal.WriteInt64(propsData, offset + 16, propData.typeConverter);
                        Marshal.WriteInt32(propsData, offset + 24, propData.extendType);
                        Marshal.WriteInt32(propsData, offset + 28, propData.readOnly);

                        ++numProps;
                    }
                }
            }

            if (eventsLen > 0)
            {
                for (int i = 0; i < eventsLen; ++i)
                {
                    var e = events[i];

                    int offset = numProps * propSize;

                    ExtendPropertyData eventData = new ExtendPropertyData();
                    eventData.name = Marshal.StringToHGlobalAnsi(e.Name).ToInt64();
                    eventData.extendType = (int)NativePropertyType.Event;

                    Marshal.WriteInt64(propsData, offset + 0, eventData.name);
                    Marshal.WriteInt64(propsData, offset + 8, eventData.type);
                    Marshal.WriteInt64(propsData, offset + 16, eventData.typeConverter);
                    Marshal.WriteInt32(propsData, offset + 24, eventData.extendType);
                    Marshal.WriteInt32(propsData, offset + 28, eventData.readOnly);

                    ++numProps;
                }
            }

            return propsData;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static IntPtr CreateNativeEnumsData(System.Type type, out int numEnums)
        {
            var names = Enum.GetNames(type);
            var values = Enum.GetValues(type);

            numEnums = values.Length;

            int enumSize = Marshal.SizeOf<long>() * 2;
            IntPtr enumsData = Marshal.AllocHGlobal(numEnums * enumSize);

            for (int i = 0; i < numEnums; ++i)
            {
                object val = values.GetValue(i);
                string name = (string)names.GetValue(i);

                int offset = i * enumSize;

                Marshal.WriteInt64(enumsData, offset + 0, Marshal.StringToHGlobalAnsi(name).ToInt64());
                Marshal.WriteInt64(enumsData, offset + 8, Convert.ToInt64(val));
            }

            return enumsData;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static IntPtr EnsureNativeType(System.Type type)
        {
            return EnsureNativeType(type, true);
        }

        public static IntPtr EnsureNativeType(System.Type type, bool registerDP)
        {
            IntPtr nativeType = TryGetNativeType(type);
            if (nativeType == IntPtr.Zero)
            {
                nativeType = RegisterNativeType(type, registerDP);
            }

            if (nativeType == IntPtr.Zero)
            {
                throw new Exception(string.Format("Unable to register native type for '%s'",
                    type.FullName));
            }

            return nativeType;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static void RegisterDependencyProperties(Type type)
        {
            // Skip types from Base Class Library assembly
            if (type.Assembly == BclAssembly)
            {
                return;
            }

            // Ensure static constructor is called for DependencyObjects or types defining DependencyProperties
            if (typeof(DependencyObject).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()) || HasDependencyProperties(type))
            {
                RunClassConstructor(type);
            }
        }

        private static bool HasDependencyProperties(Type type)
        {
#if NETFX_CORE
            var fields = type.GetTypeInfo().DeclaredFields.Where(p => p.IsStatic);
            if (fields.Any())
            {
#else
            var fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (fields.Length > 0)
            {
#endif
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType == typeof(DependencyProperty))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void RunClassConstructor(Type type)
        {
            // Ensure that static constructor is executed, so dependency properties are correctly
            // registered or overridden in Noesis reflection

            if (_constructedTypes.Add(type))
            {
                // New type added to the HashSet - ensure the static constructor was invoked.
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            }
            else
            {
                // We've already ensured the static class constructor was called sometime.
                // It's no longer possible to invoke it again, but we have everything in registry.
                DependencyPropertyRegistry.Restore(type);
            }
        }

        private static readonly Assembly BclAssembly = typeof(object).Assembly;
        private static HashSet<Type> _constructedTypes = new HashSet<Type>();

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static System.Reflection.MethodInfo FindExtendMethod(System.Type type)
        {
            System.Type baseType = type;
            while (baseType != null)
            {
                System.Reflection.MethodInfo extend = GetExtendMethod(baseType);
                if (extend != null)
                {
                    return extend;
                }

                baseType = baseType.GetTypeInfo().BaseType;
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static System.Reflection.MethodInfo GetExtendMethod(System.Type type)
        {
#if NETFX_CORE
            System.Reflection.MethodInfo extend = type.GetTypeInfo().GetDeclaredMethods("Extend").Where(m => !m.IsPublic && m.IsStatic).FirstOrDefault();
#else
            System.Reflection.MethodInfo extend = type.GetMethod("Extend", BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Static);
#endif
            if (extend != null && extend.GetParameters().Length == 1)
            {
                return extend;
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_FreeString(IntPtr strPtr);
        private static Callback_FreeString _freeString = FreeString;

        [MonoPInvokeCallback(typeof(Callback_FreeString))]
        private static void FreeString(IntPtr strPtr)
        {
            try
            {
                Marshal.FreeHGlobal(strPtr);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static string StringFromNativeUtf8(IntPtr nativeUtf8)
        {
            if (nativeUtf8 == IntPtr.Zero)
            {
                return string.Empty;
            }

#if __MonoCS__
            // Mono on all platforms currently uses UTF-8 encoding 
            return Marshal.PtrToStringAnsi(nativeUtf8);
#else
            // Waiting for PtrToStringUtf8 implementation in C#
            // https://github.com/dotnet/corefx/issues/9605
            int len = 0;
            while (Marshal.ReadByte(nativeUtf8, len) != 0) len++;
            byte[] buffer = new byte[len];
            Marshal.Copy(nativeUtf8, buffer, 0, len);
            return System.Text.Encoding.UTF8.GetString(buffer, 0, len);
#endif
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_RegisterType(string typeName);
        private static Callback_RegisterType _registerType = RegisterType;
        private static List<Assembly> _assemblies = null;

        [MonoPInvokeCallback(typeof(Callback_RegisterType))]
        private static void RegisterType(string typeName)
        {
            try
            {
#if UNITY_5_3_OR_NEWER
                if (typeName == "NoesisApp.Application")
                {
                    UnityEngine.Debug.LogError("Application not supported in Unity, please set Application Resources in NoesisGUI settings.");
                    return;
                }
                if (typeName == "NoesisApp.Window")
                {
                    UnityEngine.Debug.LogError("Window not supported in Unity, please use Page or UserControl instead.");
                    return;
                }
#endif
                Type type = FindType(typeName);

                if (type != null)
                {
                    RegisterNativeType(type);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static Type FindType(string name)
        {
            // First, look for in the currently executing assembly and in Mscorlib.dll
            // Note that this step is mandatory in WINRT because in that platform only the
            // assemblies found in InstalledLocation are manually loaded
            Type type = Type.GetType(name);

            if (type == null)
            {
                if (_assemblies == null)
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    _assemblies = new List<Assembly>(assemblies.Length);
                    _assemblies.AddRange(assemblies);

                    AppDomain.CurrentDomain.AssemblyLoad += AddLoadedAssembly;
                }

                // We want to use the types from latest loaded assemblies, so we reverse the lookup
                for (int i = _assemblies.Count - 1; i >= 0; --i)
                {
                    type = _assemblies[i].GetType(name);
                    if (type != null)
                    {
                        break;
                    }
                }
            }

            return type;
        }

        private static void AddLoadedAssembly(object sender, AssemblyLoadEventArgs e)
        {
            _assemblies.Add(e.LoadedAssembly);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_ToString(IntPtr cPtr);
        private static Callback_ToString _toString = ToStringEx;

        [MonoPInvokeCallback(typeof(Callback_ToString))]
        private static IntPtr ToStringEx(IntPtr cPtr)
        {
            try
            {
                object proxy = GetExtendInstance(cPtr);
                string str = proxy != null ? proxy.ToString() : string.Empty;
                return Marshal.StringToHGlobalUni(str != null ? str : string.Empty);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return Marshal.StringToHGlobalUni(string.Empty);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_Equals(IntPtr cPtr,
            IntPtr cPtrOtherType, IntPtr cPtrOther);
        private static Callback_Equals _equals = EqualsEx;

        [MonoPInvokeCallback(typeof(Callback_Equals))]
        private static bool EqualsEx(IntPtr cPtr, IntPtr cPtrOtherType, IntPtr cPtrOther)
        {
            try
            {
                object proxy = GetExtendInstance(cPtr);
                object proxyOther = GetProxy(cPtrOtherType, cPtrOther, false);
                return object.Equals(proxy, proxyOther);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return false;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate int Callback_VisualChildrenCount(IntPtr cPtr,
            Visual.ChildrenCountBaseCallback callback);
        private static Callback_VisualChildrenCount _visualChildrenCount = VisualChildrenCount;

        [MonoPInvokeCallback(typeof(Callback_VisualChildrenCount))]
        private static int VisualChildrenCount(IntPtr cPtr,
            Visual.ChildrenCountBaseCallback callback)
        {
            try
            {
                Visual visual = (Visual)GetExtendInstance(cPtr);
                if (visual != null)
                {
                    visual.ChildrenCountBase = callback;
                    int count = visual.VisualChildrenCount;
                    visual.ChildrenCountBase = null;

                    return count;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return 0;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_VisualGetChild(IntPtr cPtr, int index,
            Visual.ChildrenCountBaseCallback countCallback, Visual.GetChildBaseCallback childCallback);
        private static Callback_VisualGetChild _visualGetChild  = VisualGetChild;

        [MonoPInvokeCallback(typeof(Callback_VisualGetChild))]
        private static IntPtr VisualGetChild(IntPtr cPtr, int index,
            Visual.ChildrenCountBaseCallback countCallback, Visual.GetChildBaseCallback childCallback)
        {
            try
            {
                Visual visual = (Visual)GetExtendInstance(cPtr);
                if (visual != null)
                {
                    visual.ChildrenCountBase = countCallback;
                    visual.GetChildBase = childCallback;
                    Visual child = visual.GetVisualChild(index);
                    visual.GetChildBase = null;
                    visual.ChildrenCountBase = null;

                    HandleRef childPtr = GetInstanceHandle(child);
                    BaseComponent.AddReference(childPtr.Handle); // released by native bindings
                    return childPtr.Handle;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_UIElementRender(IntPtr cPtr, IntPtr contextType, IntPtr context,
            UIElement.RenderBaseCallback callback);
        private static Callback_UIElementRender _uiElementRender = UIElementRender;

        [MonoPInvokeCallback(typeof(Callback_UIElementRender))]
        private static void UIElementRender(IntPtr cPtr, IntPtr contextType, IntPtr context,
            UIElement.RenderBaseCallback callback)
        {
            try
            {
                UIElement element = (UIElement)GetExtendInstance(cPtr);
                if (element != null)
                {
                    element.OnRenderBase = callback;
                    element.OnRender((DrawingContext)GetProxy(contextType, context, false));
                    element.OnRenderBase = null;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_FrameworkElementConnectEvent(IntPtr cPtr,
            IntPtr cPtrSourceType, IntPtr cPtrSource, string eventName, string handlerName);
        private static Callback_FrameworkElementConnectEvent _frameworkElementConnectEvent = FrameworkElementConnectEvent;

        [MonoPInvokeCallback(typeof(Callback_FrameworkElementConnectEvent))]
        private static bool FrameworkElementConnectEvent(IntPtr cPtr,
            IntPtr cPtrSourceType, IntPtr cPtrSource, string eventName, string handlerName)
        {
            try
            {
                FrameworkElement element = (FrameworkElement)GetExtendInstance(cPtr);
                if (element != null)
                {
                    object source = GetProxy(cPtrSourceType, cPtrSource, false);
                    return element.ConnectEvent(source, eventName, handlerName);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_FrameworkElementConnectField(IntPtr cPtr,
            IntPtr cPtrSourceType, IntPtr cPtrSource, string fieldName);
        private static Callback_FrameworkElementConnectField _frameworkElementConnectField = FrameworkElementConnectField;

        [MonoPInvokeCallback(typeof(Callback_FrameworkElementConnectField))]
        private static void FrameworkElementConnectField(IntPtr cPtr,
            IntPtr cPtrSourceType, IntPtr cPtrSource, string fieldName)
        {
            try
            {
                FrameworkElement element = (FrameworkElement)GetExtendInstance(cPtr);
                if (element != null)
                {
                    object source = GetProxy(cPtrSourceType, cPtrSource, false);
                    element.ConnectField(source, fieldName);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_FrameworkElementMeasure(IntPtr cPtr,
            ref Size availableSize, ref Size desiredSize, FrameworkElement.LayoutBaseCallback callback);
        private static Callback_FrameworkElementMeasure _frameworkElementMeasure = FrameworkElementMeasure;

        [MonoPInvokeCallback(typeof(Callback_FrameworkElementMeasure))]
        private static void FrameworkElementMeasure(IntPtr cPtr,
            ref Size availableSize, ref Size desiredSize, FrameworkElement.LayoutBaseCallback callback)
        {
            try
            {
                FrameworkElement element = (FrameworkElement)GetExtendInstance(cPtr);
                if (element != null)
                {
                    element.MeasureBase = callback;
                    desiredSize = element.MeasureOverride(availableSize);
                    element.MeasureBase = null;

                    if (desiredSize.IsEmpty) desiredSize = new Size(0, 0);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_FrameworkElementArrange(IntPtr cPtr,
            ref Size finalSize, ref Size renderSize, FrameworkElement.LayoutBaseCallback callback);
        private static Callback_FrameworkElementArrange _frameworkElementArrange = FrameworkElementArrange;

        [MonoPInvokeCallback(typeof(Callback_FrameworkElementArrange))]
        private static void FrameworkElementArrange(IntPtr cPtr,
            ref Size finalSize, ref Size renderSize, FrameworkElement.LayoutBaseCallback callback)
        {
            try
            {
                FrameworkElement element = (FrameworkElement)GetExtendInstance(cPtr);
                if (element != null)
                {
                    element.ArrangeBase = callback;
                    renderSize = element.ArrangeOverride(finalSize);
                    element.ArrangeBase = callback;

                    if (renderSize.IsEmpty) renderSize = new Size(0, 0);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_FrameworkElementApplyTemplate(IntPtr cPtr);
        private static Callback_FrameworkElementApplyTemplate _frameworkElementApplyTemplate = FrameworkElementApplyTemplate;

        [MonoPInvokeCallback(typeof(Callback_FrameworkElementApplyTemplate))]
        private static void FrameworkElementApplyTemplate(IntPtr cPtr)
        {
            try
            {
                FrameworkElement element = (FrameworkElement)GetExtendInstance(cPtr);
                if (element != null)
                {
                    element.OnApplyTemplate();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_ItemsControlGetContainer(IntPtr cPtr,
            ItemsControl.GetContainerForItemBaseCallback callback);
        private static Callback_ItemsControlGetContainer _itemsControlGetContainer = ItemsControlGetContainer;

        [MonoPInvokeCallback(typeof(Callback_ItemsControlGetContainer))]
        private static IntPtr ItemsControlGetContainer(IntPtr cPtr,
            ItemsControl.GetContainerForItemBaseCallback callback)
        {
            try
            {
                ItemsControl itemsControl = (ItemsControl)GetExtendInstance(cPtr);
                if (itemsControl != null)
                {
                    itemsControl.GetContainerForItemBase = callback;
                    DependencyObject container = itemsControl.GetContainerForItemOverride();
                    itemsControl.GetContainerForItemBase = null;

                    HandleRef containerPtr = GetInstanceHandle(container);
                    BaseComponent.AddReference(containerPtr.Handle); // released by native bindings
                    return containerPtr.Handle;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_ItemsControlIsContainer(IntPtr cPtr, IntPtr itemTypePtr,
            IntPtr itemPtr, ItemsControl.IsItemItsOwnContainerBaseCallback callback);
        private static Callback_ItemsControlIsContainer _itemsControlIsContainer = ItemsControlIsContainer;

        [MonoPInvokeCallback(typeof(Callback_ItemsControlIsContainer))]
        private static bool ItemsControlIsContainer(IntPtr cPtr, IntPtr itemTypePtr, IntPtr itemPtr,
            ItemsControl.IsItemItsOwnContainerBaseCallback callback)
        {
            try
            {
                ItemsControl itemsControl = (ItemsControl)GetExtendInstance(cPtr);
                if (itemsControl != null)
                {
                    itemsControl.IsItemItsOwnContainerBase = callback;
                    bool isContainer = itemsControl.IsItemItsOwnContainerOverride(GetProxy(itemTypePtr, itemPtr, false));
                    itemsControl.IsItemItsOwnContainerBase = null;

                    return isContainer;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_AdornerGetTransform(IntPtr cPtr,
            ref Matrix4 transform, ref Matrix4 desiredTransform);
        private static Callback_AdornerGetTransform _adornerGetTransform = AdornerGetTransform;

        [MonoPInvokeCallback(typeof(Callback_AdornerGetTransform))]
        private static void AdornerGetTransform(IntPtr cPtr,
            ref Matrix4 transform, ref Matrix4 desiredTransform)
        {
            try
            {
                Adorner adorner = (Adorner)GetExtendInstance(cPtr);
                if (adorner != null)
                {
                    desiredTransform = adorner.GetDesiredTransform(transform);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_FreezableClone(IntPtr cPtrType, IntPtr cPtrClone,
            IntPtr cPtrSource);
        private static Callback_FreezableClone _freezableClone = FreezableClone;

        [MonoPInvokeCallback(typeof(Callback_FreezableClone))]
        private static void FreezableClone(IntPtr cPtrType, IntPtr cPtrClone, IntPtr cPtrSource)
        {
            try
            {
                Freezable clone = (Freezable)GetExtendInstance(cPtrClone);
                Freezable source = (Freezable)GetExtendInstance(cPtrSource);
                if (clone != null && source != null)
                {
                    clone.CallCloneCommonCore(source);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_ShapeGetGeometry(IntPtr cPtr);
        private static Callback_ShapeGetGeometry _shapeGetGeometry = ShapeGetGeometry;

        [MonoPInvokeCallback(typeof(Callback_ShapeGetGeometry))]
        private static IntPtr ShapeGetGeometry(IntPtr cPtr)
        {
            try
            {
                var shape = (Shape)GetExtendInstance(cPtr);
                Geometry geometry = shape.DefiningGeometry;
                HandleRef geometryPtr = GetInstanceHandle(geometry);
                return geometryPtr.Handle;
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return IntPtr.Zero;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_CommandCanExecute(IntPtr cPtr,
            IntPtr paramType, IntPtr paramPtr);
        private static Callback_CommandCanExecute _commandCanExecute = CommandCanExecute;

        [MonoPInvokeCallback(typeof(Callback_CommandCanExecute))]
        private static bool CommandCanExecute(IntPtr cPtr, IntPtr paramType, IntPtr paramPtr)
        {
            try
            {
                var command = (ICommand)GetExtendInstance(cPtr);
                return command != null ? command.CanExecute(GetProxy(paramType, paramPtr, false)) : false;
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return false;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_CommandExecute(IntPtr cPtr,
            IntPtr paramType, IntPtr paramPtr);
        private static Callback_CommandExecute _commandExecute = CommandExecute;

        [MonoPInvokeCallback(typeof(Callback_CommandExecute))]
        private static void CommandExecute(IntPtr cPtr, IntPtr paramType, IntPtr paramPtr)
        {
            try
            {
                var command = (ICommand)GetExtendInstance(cPtr);
                if (command != null)
                {
                    command.Execute(GetProxy(paramType, paramPtr, false));
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static bool IsNullableType(Type type)
        {
            return type.GetTypeInfo().IsGenericType &&
                type.GetTypeInfo().GetGenericTypeDefinition().Equals(
                    typeof(Nullable<>));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static bool AreCompatibleTypes(object source, Type targetType)
        {
            if (source == DependencyProperty.UnsetValue || source == Binding.DoNothing)
            {
                return true;
            }

            if (source == null)
            {
                // TODO: Store IsNullable in NativeTypeInfo
                return !targetType.GetTypeInfo().IsValueType || IsNullableType(targetType);
            }

            Type sourceType = source.GetType();
            if (targetType.GetTypeInfo().IsAssignableFrom(sourceType.GetTypeInfo()))
            {
                return true;
            }

            if (targetType.Equals(typeof(Noesis.BaseComponent)))
            {
                return true;
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_ConverterConvert(IntPtr cPtr,
            IntPtr valType, IntPtr valPtr, IntPtr targetTypePtr,
            IntPtr paramType, IntPtr paramPtr, out IntPtr result);
        private static Callback_ConverterConvert _converterConvert = ConverterConvert;

        [MonoPInvokeCallback(typeof(Callback_ConverterConvert))]
        private static bool ConverterConvert(IntPtr cPtr,
            IntPtr valType, IntPtr valPtr, IntPtr targetTypePtr,
            IntPtr paramType, IntPtr paramPtr, out IntPtr result)
        {
            try
            {
                var converter = (IValueConverter)GetExtendInstance(cPtr);
                if (converter != null)
                {
                    Type targetType = GetNativeTypeInfo(targetTypePtr).Type;
                    object val = GetProxy(valType, valPtr, false);
                    object param = GetProxy(paramType, paramPtr, false);

                    object obj = converter.Convert(val, targetType, param, CultureInfo.CurrentCulture);

                    if (AreCompatibleTypes(obj, targetType))
                    {
                        HandleRef res = GetInstanceHandle(obj);
                        BaseComponent.AddReference(res.Handle); // released by native bindings
                        result = res.Handle;
                        return true;
                    }
                    else
                    {
                        Log.Error(string.Format("{0} Convert() expects {1} and {2} is returned",
                            converter.GetType().FullName, targetType.FullName,
                            obj != null ? obj.GetType().FullName : "null"));
                    }
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            result = IntPtr.Zero;
            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_ConverterConvertBack(IntPtr cPtr,
            IntPtr valType, IntPtr valPtr, IntPtr targetTypePtr,
            IntPtr paramType, IntPtr paramPtr, out IntPtr result);
        private static Callback_ConverterConvertBack _converterConvertBack = ConverterConvertBack;

        [MonoPInvokeCallback(typeof(Callback_ConverterConvertBack))]
        private static bool ConverterConvertBack(IntPtr cPtr,
            IntPtr valType, IntPtr valPtr, IntPtr targetTypePtr,
            IntPtr paramType, IntPtr paramPtr, out IntPtr result)
        {
            try
            {
                var converter = (IValueConverter)GetExtendInstance(cPtr);
                if (converter != null)
                {
                    Type targetType = GetNativeTypeInfo(targetTypePtr).Type;
                    object val = GetProxy(valType, valPtr, false);
                    object param = GetProxy(paramType, paramPtr, false);

                    object obj = converter.ConvertBack(val, targetType, param, CultureInfo.CurrentCulture);

                    if (AreCompatibleTypes(obj, targetType))
                    {
                        HandleRef res = GetInstanceHandle(obj);
                        BaseComponent.AddReference(res.Handle); // released by native bindings
                        result = res.Handle;
                        return true;
                    }
                    else
                    {
                        Log.Error(string.Format("{0} ConvertBack() expects {1} and {2} is returned",
                            converter.GetType().FullName, targetType.FullName,
                            obj != null ? obj.GetType().FullName : "null"));
                    }
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            result = IntPtr.Zero;
            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_MultiConverterConvert(IntPtr cPtr,
            int numSources, IntPtr valTypes, IntPtr valPtrs, IntPtr targetTypePtr,
            IntPtr paramType, IntPtr paramPtr, out IntPtr result);
        private static Callback_MultiConverterConvert _multiConverterConvert = MultiConverterConvert;

        [MonoPInvokeCallback(typeof(Callback_MultiConverterConvert))]
        private static bool MultiConverterConvert(IntPtr cPtr,
            int numSources, IntPtr valTypes, IntPtr valPtrs, IntPtr targetTypePtr,
            IntPtr paramType, IntPtr paramPtr, out IntPtr result)
        {
            try
            {
                var converter = (IMultiValueConverter)GetExtendInstance(cPtr);
                if (converter != null)
                {
                    int elementSize = Marshal.SizeOf<IntPtr>();
                    object[] values = new object[numSources];
                    for (int i = 0; i < numSources; ++i)
                    {
                        IntPtr valType = Marshal.ReadIntPtr(valTypes, i * elementSize);
                        IntPtr val = Marshal.ReadIntPtr(valPtrs, i * elementSize);
                        values[i] = GetProxy(valType, val, false);
                    }

                    Type targetType = GetNativeTypeInfo(targetTypePtr).Type;
                    object param = GetProxy(paramType, paramPtr, false);

                    object obj = converter.Convert(values, targetType, param, CultureInfo.CurrentCulture);

                    if (AreCompatibleTypes(obj, targetType))
                    {
                        HandleRef res = GetInstanceHandle(obj);
                        BaseComponent.AddReference(res.Handle); // released by native bindings
                        result = res.Handle;
                        return true;
                    }
                    else
                    {
                        Log.Error(string.Format("{0} Convert() expects {1} and {2} is returned",
                            converter.GetType().FullName, targetType.FullName,
                            obj != null ? obj.GetType().FullName : "null"));
                    }
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            result = IntPtr.Zero;
            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_MultiConverterConvertBack(IntPtr cPtr,
            int numSources, IntPtr valType, IntPtr valPtr, IntPtr targetTypePtrs,
            IntPtr paramType, IntPtr paramPtr, IntPtr results);
        private static Callback_MultiConverterConvertBack _multiConverterConvertBack = MultiConverterConvertBack;

        [MonoPInvokeCallback(typeof(Callback_MultiConverterConvertBack))]
        private static bool MultiConverterConvertBack(IntPtr cPtr,
            int numSources, IntPtr valType, IntPtr valPtr, IntPtr targetTypePtrs,
            IntPtr paramType, IntPtr paramPtr, IntPtr results)
        {
            try
            {
                var converter = (IMultiValueConverter)GetExtendInstance(cPtr);
                if (converter != null)
                {
                    int elementSize = Marshal.SizeOf<IntPtr>();
                    Type[] types = new Type[numSources];
                    for (int i = 0; i < numSources; ++i)
                    {
                        IntPtr targetType = Marshal.ReadIntPtr(targetTypePtrs, i * elementSize);
                        types[i] = GetNativeTypeInfo(targetType).Type;
                    }

                    object val = GetProxy(valType, valPtr, false);
                    object param = GetProxy(paramType, paramPtr, false);

                    object[] objs = converter.ConvertBack(val, types, param, CultureInfo.CurrentCulture);

                    if (objs != null)
                    {
                        for (int i = 0; i < numSources; ++i)
                        {
                            object obj = i < objs.Length ? objs[i] : null;
                            HandleRef res = GetInstanceHandle(obj);
                            BaseComponent.AddReference(res.Handle); // released by native bindings
                            Marshal.WriteIntPtr(results, i * elementSize, res.Handle);
                        }

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate uint Callback_ListCount(IntPtr cPtr);
        private static Callback_ListCount _listCount = ListCount;

        [MonoPInvokeCallback(typeof(Callback_ListCount))]
        private static uint ListCount(IntPtr cPtr)
        {
            try
            {
                var list = (System.Collections.IList)GetExtendInstance(cPtr);
                return list != null && Initialized ? (uint)list.Count : 0;
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_ListGet(IntPtr cPtr, uint index);
        private static Callback_ListGet _listGet = ListGet;

        [MonoPInvokeCallback(typeof(Callback_ListGet))]
        private static IntPtr ListGet(IntPtr cPtr, uint index)
        {
            try
            {
                var list = (System.Collections.IList)GetExtendInstance(cPtr);
                object item = list != null ? list[(int)index] : null;
                HandleRef itemPtr = GetInstanceHandle(item);
                BaseComponent.AddReference(itemPtr.Handle); // released by native bindings
                return itemPtr.Handle;
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return IntPtr.Zero;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ListSet(IntPtr cPtr, uint index, IntPtr itemType, IntPtr item);
        private static Callback_ListSet _listSet = ListSet;

        [MonoPInvokeCallback(typeof(Callback_ListSet))]
        private static void ListSet(IntPtr cPtr, uint index, IntPtr itemType, IntPtr item)
        {
            try
            {
                var list = (System.Collections.IList)GetExtendInstance(cPtr);
                if (list != null)
                {
                    list[(int)index] = GetProxy(itemType, item, false);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate uint Callback_ListAdd(IntPtr cPtr, IntPtr itemType, IntPtr item);
        private static Callback_ListAdd _listAdd = ListAdd;

        [MonoPInvokeCallback(typeof(Callback_ListAdd))]
        private static uint ListAdd(IntPtr cPtr, IntPtr itemType, IntPtr item)
        {
            try
            {
                var list = (System.Collections.IList)GetExtendInstance(cPtr);
                return list != null ? (uint)list.Add(GetProxy(itemType, item, false)) : 0;
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ListInsert(IntPtr cPtr, uint index, IntPtr itemType, IntPtr item);
        private static Callback_ListInsert _listInsert = ListInsert;

        [MonoPInvokeCallback(typeof(Callback_ListInsert))]
        private static void ListInsert(IntPtr cPtr, uint index, IntPtr itemType, IntPtr item)
        {
            try
            {
                var list = (System.Collections.IList)GetExtendInstance(cPtr);
                if (list != null)
                {
                    list.Insert((int)index, GetProxy(itemType, item, false));
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate int Callback_ListIndexOf(IntPtr cPtr, IntPtr itemType, IntPtr item);
        private static Callback_ListIndexOf _listIndexOf = ListIndexOf;

        [MonoPInvokeCallback(typeof(Callback_ListIndexOf))]
        private static int ListIndexOf(IntPtr cPtr, IntPtr itemType, IntPtr item)
        {
            try
            {
                var list = (System.Collections.IList)GetExtendInstance(cPtr);
                return list != null ? list.IndexOf(GetProxy(itemType, item, false)) : -1;
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return -1;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ListRemoveAt(IntPtr cPtr, uint index);
        private static Callback_ListRemoveAt _listRemoveAt = ListRemoveAt;

        [MonoPInvokeCallback(typeof(Callback_ListRemoveAt))]
        private static void ListRemoveAt(IntPtr cPtr, uint index)
        {
            try
            {
                var list = (System.Collections.IList)GetExtendInstance(cPtr);
                if (list != null)
                {
                    list.RemoveAt((int)index);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ListClear(IntPtr cPtr);
        private static Callback_ListClear _listClear = ListClear;

        [MonoPInvokeCallback(typeof(Callback_ListClear))]
        private static void ListClear(IntPtr cPtr)
        {
            try
            {
                var list = (System.Collections.IList)GetExtendInstance(cPtr);
                if (list != null)
                {
                    list.Clear();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_DictionaryFind(IntPtr cPtr, string key, ref IntPtr item);
        private static Callback_DictionaryFind _dictionaryFind = DictionaryFind;

        [MonoPInvokeCallback(typeof(Callback_DictionaryFind))]
        private static bool DictionaryFind(IntPtr cPtr, string key, ref IntPtr item)
        {
            try
            {
                var dictionary = (System.Collections.IDictionary)GetExtendInstance(cPtr);
                if (dictionary != null && dictionary.Contains(key))
                {
                    HandleRef itemPtr = GetInstanceHandle(dictionary[key]);
                    BaseComponent.AddReference(itemPtr.Handle); // released by native bindings
                    item = itemPtr.Handle;
                    return true;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            item = IntPtr.Zero;
            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_DictionarySet(IntPtr cPtr, string key, IntPtr itemType, IntPtr item);
        private static Callback_DictionarySet _dictionarySet = DictionarySet;

        [MonoPInvokeCallback(typeof(Callback_DictionarySet))]
        private static void DictionarySet(IntPtr cPtr, string key, IntPtr itemType, IntPtr item)
        {
            try
            {
                var dictionary = (System.Collections.IDictionary)GetExtendInstance(cPtr);
                if (dictionary != null)
                {
                    dictionary[key] = GetProxy(itemType, item, false);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_DictionaryAdd(IntPtr cPtr, string key, IntPtr itemType, IntPtr item);
        private static Callback_DictionaryAdd _dictionaryAdd = DictionaryAdd;

        [MonoPInvokeCallback(typeof(Callback_DictionaryAdd))]
        private static void DictionaryAdd(IntPtr cPtr, string key, IntPtr itemType, IntPtr item)
        {
            try
            {
                var dictionary = (System.Collections.IDictionary)GetExtendInstance(cPtr);
                if (dictionary != null)
                {
                    dictionary.Add(key, GetProxy(itemType, item, false));
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_DictionaryRemove(IntPtr cPtr, string key);
        private static Callback_DictionaryRemove _dictionaryRemove = DictionaryRemove;

        [MonoPInvokeCallback(typeof(Callback_DictionaryRemove))]
        private static void DictionaryRemove(IntPtr cPtr, string key)
        {
            try
            {
                var dictionary = (System.Collections.IDictionary)GetExtendInstance(cPtr);
                if (dictionary != null)
                {
                    dictionary.Remove(key);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_ListIndexerTryGet(IntPtr cPtrType, IntPtr cPtr, uint index, ref IntPtr item);
        private static Callback_ListIndexerTryGet _listIndexerTryGet = ListIndexerTryGet;

        [MonoPInvokeCallback(typeof(Callback_ListIndexerTryGet))]
        [return: MarshalAs(UnmanagedType.U1)]
        private static bool ListIndexerTryGet(IntPtr cPtrType, IntPtr cPtr, uint index, ref IntPtr item)
        {
            try
            {
                object proxy = GetExtendInstance(cPtr);
                if (proxy != null)
                {
                    NativeTypeIndexerInfo info = (NativeTypeIndexerInfo)GetNativeTypeInfo(cPtrType);

                    try
                    {
                        IndexerAccessorT<int> indexer = (IndexerAccessorT<int>)info.Indexer;
                        HandleRef itemPtr = GetInstanceHandle(indexer.Get(proxy, (int)index));
                        BaseComponent.AddReference(itemPtr.Handle); // released by native bindings
                        item = itemPtr.Handle;
                        return true;
                    }
                    catch { }
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            item = IntPtr.Zero;
            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_ListIndexerTrySet(IntPtr cPtrType, IntPtr cPtr, uint index, IntPtr itemType, IntPtr item);
        private static Callback_ListIndexerTrySet _listIndexerTrySet = ListIndexerTrySet;

        [MonoPInvokeCallback(typeof(Callback_ListIndexerTrySet))]
        [return: MarshalAs(UnmanagedType.U1)]
        private static bool ListIndexerTrySet(IntPtr cPtrType, IntPtr cPtr, uint index, IntPtr itemType, IntPtr item)
        {
            try
            {
                object proxy = GetExtendInstance(cPtr);
                if (proxy != null)
                {
                    object itemObj = GetProxy(itemType, item, false);
                    NativeTypeIndexerInfo info = (NativeTypeIndexerInfo)GetNativeTypeInfo(cPtrType);

                    try
                    {
                        IndexerAccessorT<int> indexer = (IndexerAccessorT<int>)info.Indexer;
                        indexer.Set(proxy, (int)index, itemObj);
                        return true;
                    }
                    catch { }
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_DictionaryIndexerTryGet(IntPtr cPtrType, IntPtr cPtr, string key, ref IntPtr item);
        private static Callback_DictionaryIndexerTryGet _dictionaryIndexerTryGet = DictionaryIndexerTryGet;

        [MonoPInvokeCallback(typeof(Callback_DictionaryIndexerTryGet))]
        [return: MarshalAs(UnmanagedType.U1)]
        private static bool DictionaryIndexerTryGet(IntPtr cPtrType, IntPtr cPtr, string key, ref IntPtr item)
        {
            try
            {
                object proxy = GetExtendInstance(cPtr);
                if (proxy != null)
                {
                    NativeTypeIndexerInfo info = (NativeTypeIndexerInfo)GetNativeTypeInfo(cPtrType);

                    try
                    {
                        IndexerAccessorT<string> indexer = (IndexerAccessorT<string>)info.Indexer;
                        HandleRef itemPtr = GetInstanceHandle(indexer.Get(proxy, key));
                        BaseComponent.AddReference(itemPtr.Handle); // released by native bindings
                        item = itemPtr.Handle;
                        return true;
                    }
                    catch { }
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            item = IntPtr.Zero;
            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_DictionaryIndexerTrySet(IntPtr cPtrType, IntPtr cPtr, string key, IntPtr itemType, IntPtr item);
        private static Callback_DictionaryIndexerTrySet _dictionaryIndexerTrySet = DictionaryIndexerTrySet;

        [MonoPInvokeCallback(typeof(Callback_DictionaryIndexerTrySet))]
        [return: MarshalAs(UnmanagedType.U1)]
        private static bool DictionaryIndexerTrySet(IntPtr cPtrType, IntPtr cPtr, string key, IntPtr itemType, IntPtr item)
        {
            try
            {
                object proxy = GetExtendInstance(cPtr);
                if (proxy != null)
                {
                    object itemObj = GetProxy(itemType, item, false);
                    NativeTypeIndexerInfo info = (NativeTypeIndexerInfo)GetNativeTypeInfo(cPtrType);

                    try
                    {
                        IndexerAccessorT<string> indexer = (IndexerAccessorT<string>)info.Indexer;
                        indexer.Set(proxy, key, itemObj);
                        return true;
                    }
                    catch { }
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_SelectTemplate(IntPtr cPtr,
            IntPtr itemType, IntPtr item, IntPtr containerType, IntPtr container);
        private static Callback_SelectTemplate _selectTemplate = SelectTemplate;

        [MonoPInvokeCallback(typeof(Callback_SelectTemplate))]
        private static IntPtr SelectTemplate(IntPtr cPtr,
            IntPtr itemType, IntPtr item, IntPtr containerType, IntPtr container)
        {
            try
            {
                DataTemplateSelector selector = (DataTemplateSelector)GetExtendInstance(cPtr);
                if (selector != null)
                {
                    DataTemplate template = selector.SelectTemplate(GetProxy(itemType, item, false),
                        (DependencyObject)GetProxy(containerType, container, false));
                    return GetInstanceHandle(template).Handle;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_StreamSetPosition(IntPtr cPtr, uint pos);
        private static Callback_StreamSetPosition _streamSetPosition = StreamSetPosition;

        [MonoPInvokeCallback(typeof(Callback_StreamSetPosition))]
        private static void StreamSetPosition(IntPtr cPtr, uint pos)
        {
            try
            {
                var stream = (System.IO.Stream)GetExtendInstance(cPtr);
                if (stream != null)
                {
                    stream.Position = pos;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate uint Callback_StreamGetPosition(IntPtr cPtr);
        private static Callback_StreamGetPosition _streamGetPosition = StreamGetPosition;

        [MonoPInvokeCallback(typeof(Callback_StreamGetPosition))]
        private static uint StreamGetPosition(IntPtr cPtr)
        {
            try
            {
                var stream = (System.IO.Stream)GetExtendInstance(cPtr);
                return stream != null ? (uint)stream.Position : 0;
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate uint Callback_StreamGetLength(IntPtr cPtr);
        private static Callback_StreamGetLength _streamGetLength = StreamGetLength;

        [MonoPInvokeCallback(typeof(Callback_StreamGetLength))]
        private static uint StreamGetLength(IntPtr cPtr)
        {
            try
            {
                var stream = (System.IO.Stream)GetExtendInstance(cPtr);
                return stream != null ? (uint)stream.Length : 0;
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate uint Callback_StreamRead(IntPtr cPtr, IntPtr buffer, uint bufferSize);
        private static Callback_StreamRead _streamRead = StreamRead;

        [MonoPInvokeCallback(typeof(Callback_StreamRead))]
        private static uint StreamRead(IntPtr cPtr, IntPtr buffer, uint bufferSize)
        {
            try
            {
                var stream = (System.IO.Stream)GetExtendInstance(cPtr);
                if (stream != null)
                {
                    byte[] bytes = new byte[bufferSize];
                    int readBytes = stream.Read(bytes, 0, (int)bufferSize);
                    System.Runtime.InteropServices.Marshal.Copy(bytes, 0, buffer, (int)bufferSize);
                    return (uint)readBytes;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return 0;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_StreamClose(IntPtr cPtr);
        private static Callback_StreamClose _streamClose = StreamClose;

        [MonoPInvokeCallback(typeof(Callback_StreamClose))]
        private static void StreamClose(IntPtr cPtr)
        {
            try
            {
                var stream = (System.IO.Stream)GetExtendInstance(cPtr);
                if (stream != null)
                {
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_ProviderLoadXaml(IntPtr cPtr, IntPtr filename);
        private static Callback_ProviderLoadXaml _providerLoadXaml = ProviderLoadXaml;

        [MonoPInvokeCallback(typeof(Callback_ProviderLoadXaml))]
        private static IntPtr ProviderLoadXaml(IntPtr cPtr, IntPtr filename)
        {
            try
            {
                XamlProvider provider = (XamlProvider)GetExtendInstance(cPtr);
                if (provider != null)
                {
                    string filename_ = StringFromNativeUtf8(filename);
                    Uri uri = new Uri(filename_, UriKind.RelativeOrAbsolute);
                    System.IO.Stream stream = provider.LoadXaml(uri);
                    HandleRef handle = GetInstanceHandle(stream);
                    BaseComponent.AddReference(handle.Handle); // released by C++
                    return handle.Handle;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ProviderTextureInfo(IntPtr cPtr, IntPtr filename,
            ref uint x, ref uint y, ref uint width, ref uint height, ref float dpiScale);
        private static Callback_ProviderTextureInfo _providerTextureInfo = ProviderTextureInfo;

        [MonoPInvokeCallback(typeof(Callback_ProviderTextureInfo))]
        private static void ProviderTextureInfo(IntPtr cPtr, IntPtr filename,
            ref uint x, ref uint y, ref uint width, ref uint height, ref float dpiScale)
        {
            width = height = 0;

            try
            {
                TextureProvider provider = (TextureProvider)GetExtendInstance(cPtr);
                if (provider != null)
                {
                    string filename_ = StringFromNativeUtf8(filename);
                    Uri uri = new Uri(filename_, UriKind.RelativeOrAbsolute);
                    TextureProvider.TextureInfo info = provider.GetTextureInfo(uri);

                    x = (uint)info.Rect.X;
                    y = (uint)info.Rect.Y;
                    width = (uint)info.Rect.Width;
                    height = (uint)info.Rect.Height;
                    dpiScale = info.DpiScale;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_ProviderTextureLoad(IntPtr cPtr, IntPtr filename);
        private static Callback_ProviderTextureLoad _providerTextureLoad = ProviderTextureLoad;

        [MonoPInvokeCallback(typeof(Callback_ProviderTextureLoad))]
        private static IntPtr ProviderTextureLoad(IntPtr cPtr, IntPtr filename)
        {
            try
            {
                TextureProvider provider = (TextureProvider)GetExtendInstance(cPtr);
                if (provider != null)
                {
                    string filename_ = StringFromNativeUtf8(filename);
                    Uri uri = new Uri(filename_, UriKind.RelativeOrAbsolute);
                    Texture texture = provider.LoadTexture(uri);
                    HandleRef handle = GetInstanceHandle(texture);
                    BaseComponent.AddReference(handle.Handle); // released by C++
                    return handle.Handle;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_ProviderTextureOpen(IntPtr cPtr, IntPtr filename);
        private static Callback_ProviderTextureOpen _providerTextureOpen = ProviderTextureOpen;

        [MonoPInvokeCallback(typeof(Callback_ProviderTextureOpen))]
        private static IntPtr ProviderTextureOpen(IntPtr cPtr, IntPtr filename)
        {
            try
            {
                FileTextureProvider provider = (FileTextureProvider)GetExtendInstance(cPtr);
                if (provider != null)
                {
                    string filename_ = StringFromNativeUtf8(filename);
                    Uri uri = new Uri(filename_, UriKind.RelativeOrAbsolute);
                    System.IO.Stream stream = provider.OpenStream(uri);
                    HandleRef handle = GetInstanceHandle(stream);
                    BaseComponent.AddReference(handle.Handle); // released by C++
                    return handle.Handle;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_ProviderMatchFont(IntPtr cPtr, IntPtr baseUri, IntPtr familyName,
            ref int weight, ref int stretch, ref int style, ref uint index);
        private static Callback_ProviderMatchFont _providerMatchFont = ProviderMatchFont;

        [MonoPInvokeCallback(typeof(Callback_ProviderMatchFont))]
        private static IntPtr ProviderMatchFont(IntPtr cPtr, IntPtr baseUri, IntPtr familyName,
            ref int weight, ref int stretch, ref int style, ref uint index)
        {
            try
            {
                FontProvider provider = (FontProvider)GetExtendInstance(cPtr);
                if (provider != null)
                {
                    Uri baseUri_ = new Uri(StringFromNativeUtf8(baseUri), UriKind.RelativeOrAbsolute);
                    string familyName_ = StringFromNativeUtf8(familyName);

                    FontWeight weight_ = (FontWeight)weight;
                    FontStretch stretch_ = (FontStretch)stretch;
                    FontStyle style_ = (FontStyle)style;
                    FontProvider.FontSource font = provider.MatchFont(baseUri_, familyName_,
                        ref weight_, ref stretch_, ref style_);

                    weight = (int)weight_;
                    stretch = (int)stretch_;
                    style = (int)style_;
                    index = font.faceIndex;

                    HandleRef handle = GetInstanceHandle(font.file);
                    BaseComponent.AddReference(handle.Handle); // released by C++
                    return handle.Handle;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_ProviderFamilyExists(IntPtr cPtr, IntPtr baseUri, IntPtr familyName);
        private static Callback_ProviderFamilyExists _providerFamilyExists = ProviderFamilyExists;

        [MonoPInvokeCallback(typeof(Callback_ProviderFamilyExists))]
        [return: MarshalAs(UnmanagedType.U1)]
        private static bool ProviderFamilyExists(IntPtr cPtr, IntPtr baseUri, IntPtr familyName)
        {
            try
            {
                FontProvider provider = (FontProvider)GetExtendInstance(cPtr);
                if (provider != null)
                {
                    Uri baseUri_ = new Uri(StringFromNativeUtf8(baseUri), UriKind.RelativeOrAbsolute);
                    string familyName_ = StringFromNativeUtf8(familyName);

                    return provider.FamilyExists(baseUri_, familyName_);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ProviderScanFolder(IntPtr cPtr, IntPtr folder);
        private static Callback_ProviderScanFolder _providerScanFolder = ProviderScanFolder;

        [MonoPInvokeCallback(typeof(Callback_ProviderScanFolder))]
        private static void ProviderScanFolder(IntPtr cPtr, IntPtr folder)
        {
            try
            {
                FontProvider provider = (FontProvider)GetExtendInstance(cPtr);
                if (provider != null)
                {
                    string folder_ = StringFromNativeUtf8(folder);
                    Uri uri = new Uri(folder_, UriKind.RelativeOrAbsolute);
                    provider.ScanFolder(uri);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_ProviderOpenFont(IntPtr cPtr, IntPtr folder, IntPtr id);
        private static Callback_ProviderOpenFont _providerOpenFont = ProviderOpenFont;

        [MonoPInvokeCallback(typeof(Callback_ProviderOpenFont))]
        private static IntPtr ProviderOpenFont(IntPtr cPtr, IntPtr folder, IntPtr filename)
        {
            try
            {
                FontProvider provider = (FontProvider)GetExtendInstance(cPtr);
                if (provider != null)
                {
                    string folder_ = StringFromNativeUtf8(folder);
                    string filename_ = StringFromNativeUtf8(filename);
                    Uri uri = new Uri(folder_, UriKind.RelativeOrAbsolute);
                    System.IO.Stream stream = provider.OpenFont(uri, filename_);
                    HandleRef handle = GetInstanceHandle(stream);
                    BaseComponent.AddReference(handle.Handle); // released by C++
                    return handle.Handle;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoBringIntoView(IntPtr cPtr, int index);
        private static Callback_ScrollInfoBringIntoView _scrollInfoBringIntoView = ScrollInfoBringIntoView;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoBringIntoView))]
        private static void ScrollInfoBringIntoView(IntPtr cPtr, int index)
        {
            try
            {
                VirtualizingPanel panel = (VirtualizingPanel)GetExtendInstance(cPtr);
                if (panel != null)
                {
                    panel.BringIndexIntoViewPublic(index);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_ScrollInfoGetCanHorizontalScroll(IntPtr cPtr);
        private static Callback_ScrollInfoGetCanHorizontalScroll _scrollInfoGetCanHorizontalScroll = ScrollInfoGetCanHorizontalScroll;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoGetCanHorizontalScroll))]
        [return: MarshalAs(UnmanagedType.U1)]
        private static bool ScrollInfoGetCanHorizontalScroll(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    return scrollInfo.CanHorizontallyScroll;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoSetCanHorizontalScroll(IntPtr cPtr,
            [MarshalAs(UnmanagedType.U1)] bool canScroll);
        private static Callback_ScrollInfoSetCanHorizontalScroll _scrollInfoSetCanHorizontalScroll = ScrollInfoSetCanHorizontalScroll;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoSetCanHorizontalScroll))]
        private static void ScrollInfoSetCanHorizontalScroll(IntPtr cPtr, bool canScroll)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.CanHorizontallyScroll = canScroll;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_ScrollInfoGetCanVerticalScroll(IntPtr cPtr);
        private static Callback_ScrollInfoGetCanVerticalScroll _scrollInfoGetCanVerticalScroll = ScrollInfoGetCanVerticalScroll;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoGetCanVerticalScroll))]
        [return: MarshalAs(UnmanagedType.U1)]
        private static bool ScrollInfoGetCanVerticalScroll(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    return scrollInfo.CanVerticallyScroll;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoSetCanVerticalScroll(IntPtr cPtr,
            [MarshalAs(UnmanagedType.U1)] bool canScroll);
        private static Callback_ScrollInfoSetCanVerticalScroll _scrollInfoSetCanVerticalScroll = ScrollInfoSetCanVerticalScroll;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoSetCanVerticalScroll))]
        private static void ScrollInfoSetCanVerticalScroll(IntPtr cPtr, bool canScroll)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.CanVerticallyScroll = canScroll;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate float Callback_ScrollInfoGetExtentWidth(IntPtr cPtr);
        private static Callback_ScrollInfoGetExtentWidth _scrollInfoGetExtentWidth = ScrollInfoGetExtentWidth;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoGetExtentWidth))]
        private static float ScrollInfoGetExtentWidth(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    return scrollInfo.ExtentWidth;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return 0.0f;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate float Callback_ScrollInfoGetExtentHeight(IntPtr cPtr);
        private static Callback_ScrollInfoGetExtentHeight _scrollInfoGetExtentHeight = ScrollInfoGetExtentHeight;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoGetExtentHeight))]
        private static float ScrollInfoGetExtentHeight(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    return scrollInfo.ExtentHeight;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return 0.0f;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate float Callback_ScrollInfoGetViewportWidth(IntPtr cPtr);
        private static Callback_ScrollInfoGetViewportWidth _scrollInfoGetViewportWidth = ScrollInfoGetViewportWidth;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoGetViewportWidth))]
        private static float ScrollInfoGetViewportWidth(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    return scrollInfo.ViewportWidth;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return 0.0f;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate float Callback_ScrollInfoGetViewportHeight(IntPtr cPtr);
        private static Callback_ScrollInfoGetViewportHeight _scrollInfoGetViewportHeight = ScrollInfoGetViewportHeight;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoGetViewportHeight))]
        private static float ScrollInfoGetViewportHeight(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    return scrollInfo.ViewportHeight;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return 0.0f;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate float Callback_ScrollInfoGetHorizontalOffset(IntPtr cPtr);
        private static Callback_ScrollInfoGetHorizontalOffset _scrollInfoGetHorizontalOffset = ScrollInfoGetHorizontalOffset;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoGetHorizontalOffset))]
        private static float ScrollInfoGetHorizontalOffset(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    return scrollInfo.HorizontalOffset;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return 0.0f;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate float Callback_ScrollInfoGetVerticalOffset(IntPtr cPtr);
        private static Callback_ScrollInfoGetVerticalOffset _scrollInfoGetVerticalOffset = ScrollInfoGetVerticalOffset;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoGetVerticalOffset))]
        private static float ScrollInfoGetVerticalOffset(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    return scrollInfo.VerticalOffset;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return 0.0f;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_ScrollInfoGetScrollOwner(IntPtr cPtr);
        private static Callback_ScrollInfoGetScrollOwner _scrollInfoGetScrollOwner = ScrollInfoGetScrollOwner;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoGetScrollOwner))]
        private static IntPtr ScrollInfoGetScrollOwner(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    ScrollViewer scrollViewer = scrollInfo.ScrollOwner;
                    return GetInstanceHandle(scrollViewer).Handle;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return IntPtr.Zero;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoSetScrollOwner(IntPtr cPtr, IntPtr typeOwner,
            IntPtr cPtrOwner);
        private static Callback_ScrollInfoSetScrollOwner _scrollInfoSetScrollOwner = ScrollInfoSetScrollOwner;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoSetScrollOwner))]
        private static void ScrollInfoSetScrollOwner(IntPtr cPtr, IntPtr typeOwner, IntPtr cPtrOwner)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    ScrollViewer owner = (ScrollViewer)GetProxy(typeOwner, cPtrOwner, false);
                    scrollInfo.ScrollOwner = owner;
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoLineLeft(IntPtr cPtr);
        private static Callback_ScrollInfoLineLeft _scrollInfoLineLeft = ScrollInfoLineLeft;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoLineLeft))]
        private static void ScrollInfoLineLeft(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.LineLeft();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoLineRight(IntPtr cPtr);
        private static Callback_ScrollInfoLineRight _scrollInfoLineRight = ScrollInfoLineRight;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoLineRight))]
        private static void ScrollInfoLineRight(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.LineRight();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoLineUp(IntPtr cPtr);
        private static Callback_ScrollInfoLineUp _scrollInfoLineUp = ScrollInfoLineUp;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoLineUp))]
        private static void ScrollInfoLineUp(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.LineUp();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoLineDown(IntPtr cPtr);
        private static Callback_ScrollInfoLineDown _scrollInfoLineDown = ScrollInfoLineDown;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoLineDown))]
        private static void ScrollInfoLineDown(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.LineDown();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoPageLeft(IntPtr cPtr);
        private static Callback_ScrollInfoPageLeft _scrollInfoPageLeft = ScrollInfoPageLeft;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoPageLeft))]
        private static void ScrollInfoPageLeft(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.PageLeft();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoPageRight(IntPtr cPtr);
        private static Callback_ScrollInfoPageRight _scrollInfoPageRight = ScrollInfoPageRight;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoPageRight))]
        private static void ScrollInfoPageRight(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.PageRight();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoPageUp(IntPtr cPtr);
        private static Callback_ScrollInfoPageUp _scrollInfoPageUp = ScrollInfoPageUp;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoPageUp))]
        private static void ScrollInfoPageUp(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.PageUp();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoPageDown(IntPtr cPtr);
        private static Callback_ScrollInfoPageDown _scrollInfoPageDown = ScrollInfoPageDown;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoPageDown))]
        private static void ScrollInfoPageDown(IntPtr cPtr)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.PageDown();
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoMouseWheelLeft(IntPtr cPtr, float delta);
        private static Callback_ScrollInfoMouseWheelLeft _scrollInfoMouseWheelLeft = ScrollInfoMouseWheelLeft;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoMouseWheelLeft))]
        private static void ScrollInfoMouseWheelLeft(IntPtr cPtr, float delta)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.MouseWheelLeft(delta);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoMouseWheelRight(IntPtr cPtr, float delta);
        private static Callback_ScrollInfoMouseWheelRight _scrollInfoMouseWheelRight = ScrollInfoMouseWheelRight;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoMouseWheelRight))]
        private static void ScrollInfoMouseWheelRight(IntPtr cPtr, float delta)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.MouseWheelRight(delta);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoMouseWheelUp(IntPtr cPtr, float delta);
        private static Callback_ScrollInfoMouseWheelUp _scrollInfoMouseWheelUp = ScrollInfoMouseWheelUp;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoMouseWheelUp))]
        private static void ScrollInfoMouseWheelUp(IntPtr cPtr, float delta)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.MouseWheelUp(delta);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoMouseWheelDown(IntPtr cPtr, float delta);
        private static Callback_ScrollInfoMouseWheelDown _scrollInfoMouseWheelDown = ScrollInfoMouseWheelDown;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoMouseWheelDown))]
        private static void ScrollInfoMouseWheelDown(IntPtr cPtr, float delta)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.MouseWheelDown(delta);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoSetHorizontalOffset(IntPtr cPtr, float offset);
        private static Callback_ScrollInfoSetHorizontalOffset _scrollInfoSetHorizontalOffset = ScrollInfoSetHorizontalOffset;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoSetHorizontalOffset))]
        private static void ScrollInfoSetHorizontalOffset(IntPtr cPtr, float offset)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.SetHorizontalOffset(offset);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoSetVerticalOffset(IntPtr cPtr, float offset);
        private static Callback_ScrollInfoSetVerticalOffset _scrollInfoSetVerticalOffset = ScrollInfoSetVerticalOffset;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoSetVerticalOffset))]
        private static void ScrollInfoSetVerticalOffset(IntPtr cPtr, float offset)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    scrollInfo.SetVerticalOffset(offset);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ScrollInfoMakeVisible(IntPtr cPtr, IntPtr visualType,
            IntPtr visualPtr, ref Rect rectangle, ref Rect result);
        private static Callback_ScrollInfoMakeVisible _scrollInfoMakeVisible = ScrollInfoMakeVisible;

        [MonoPInvokeCallback(typeof(Callback_ScrollInfoMakeVisible))]
        private static void ScrollInfoMakeVisible(IntPtr cPtr, IntPtr visualType, IntPtr visualPtr,
            ref Rect rectangle, ref Rect result)
        {
            try
            {
                IScrollInfo scrollInfo = (IScrollInfo)GetExtendInstance(cPtr);
                if (scrollInfo != null)
                {
                    Visual visual = (Visual)GetProxy(visualType, visualPtr, false);
                    result = scrollInfo.MakeVisible(visual, rectangle);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_MarkupExtensionProvideValue(IntPtr cPtr, IntPtr provider);
        private static Callback_MarkupExtensionProvideValue _markupExtensionProvideValue = MarkupExtensionProvideValue;

        [MonoPInvokeCallback(typeof(Callback_MarkupExtensionProvideValue))]
        private static IntPtr MarkupExtensionProvideValue(IntPtr cPtr, IntPtr provider)
        {
            try
            {
                var extension = (MarkupExtension)GetExtendInstance(cPtr);
                MarkupExtensionProvider provider_ = new MarkupExtensionProvider(provider);
                object value = extension != null ? extension.ProvideValue(provider_) : null;
                HandleRef itemPtr = GetInstanceHandle(value);
                BaseComponent.AddReference(itemPtr.Handle); // released by native bindings
                return itemPtr.Handle;
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return IntPtr.Zero;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_AnimationTimelineGetType(IntPtr cPtr);
        private static Callback_AnimationTimelineGetType _animationTimelineGetType = AnimationTimelineGetType;

        [MonoPInvokeCallback(typeof(Callback_AnimationTimelineGetType))]
        private static IntPtr AnimationTimelineGetType(IntPtr cPtr)
        {
            try
            {
                var animation = (AnimationTimeline)GetExtendInstance(cPtr);
                Type type = animation.TargetPropertyType;
                return EnsureNativeType(type);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return IntPtr.Zero;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_AnimationTimelineGetValue(IntPtr cPtr,
            IntPtr srcType, IntPtr srcPtr, IntPtr dstType, IntPtr dstPtr,
            IntPtr clockType, IntPtr clockPtr);
        private static Callback_AnimationTimelineGetValue _animationTimelineGetValue = AnimationTimelineGetValue;

        [MonoPInvokeCallback(typeof(Callback_AnimationTimelineGetValue))]
        private static IntPtr AnimationTimelineGetValue(IntPtr cPtr,
            IntPtr srcType, IntPtr srcPtr, IntPtr dstType, IntPtr dstPtr,
            IntPtr clockType, IntPtr clockPtr)
        {
            try
            {
                var animation = (AnimationTimeline)GetExtendInstance(cPtr);
                object src = GetProxy(srcType, srcPtr, false);
                object dst = GetProxy(dstType, dstPtr, false);
                AnimationClock clock = (AnimationClock)GetProxy(clockType, clockPtr, false);
                object value = animation.GetCurrentValue(src, dst, clock);
                HandleRef valuePtr = GetInstanceHandle(value);
                BaseComponent.AddReference(valuePtr.Handle); // released by native bindings
                return valuePtr.Handle;
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return IntPtr.Zero;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate bool Callback_BooleanAnimationGetValue(IntPtr cPtr,
            bool src, bool dst, IntPtr clockType, IntPtr clockPtr);
        private static Callback_BooleanAnimationGetValue _booleanAnimationGetValue = BooleanAnimationGetValue;

        [MonoPInvokeCallback(typeof(Callback_BooleanAnimationGetValue))]
        [return: MarshalAs(UnmanagedType.U1)]
        private static bool BooleanAnimationGetValue(IntPtr cPtr,
            bool src, bool dst, IntPtr clockType, IntPtr clockPtr)
        {
            try
            {
                var animation = (BooleanAnimationBase)GetExtendInstance(cPtr);
                AnimationClock clock = (AnimationClock)GetProxy(clockType, clockPtr, false);
                return animation.GetCurrentValueCore(src, dst, clock);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return false;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate short Callback_Int16AnimationGetValue(IntPtr cPtr,
            short src, short dst, IntPtr clockType, IntPtr clockPtr);
        private static Callback_Int16AnimationGetValue _int16AnimationGetValue = Int16AnimationGetValue;

        [MonoPInvokeCallback(typeof(Callback_Int16AnimationGetValue))]
        private static short Int16AnimationGetValue(IntPtr cPtr,
            short src, short dst, IntPtr clockType, IntPtr clockPtr)
        {
            try
            {
                var animation = (Int16AnimationBase)GetExtendInstance(cPtr);
                AnimationClock clock = (AnimationClock)GetProxy(clockType, clockPtr, false);
                return animation.GetCurrentValueCore(src, dst, clock);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate int Callback_Int32AnimationGetValue(IntPtr cPtr,
            int src, int dst, IntPtr clockType, IntPtr clockPtr);
        private static Callback_Int32AnimationGetValue _int32AnimationGetValue = Int32AnimationGetValue;

        [MonoPInvokeCallback(typeof(Callback_Int32AnimationGetValue))]
        private static int Int32AnimationGetValue(IntPtr cPtr,
            int src, int dst, IntPtr clockType, IntPtr clockPtr)
        {
            try
            {
                var animation = (Int32AnimationBase)GetExtendInstance(cPtr);
                AnimationClock clock = (AnimationClock)GetProxy(clockType, clockPtr, false);
                return animation.GetCurrentValueCore(src, dst, clock);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate long Callback_Int64AnimationGetValue(IntPtr cPtr,
            long src, long dst, IntPtr clockType, IntPtr clockPtr);
        private static Callback_Int64AnimationGetValue _int64AnimationGetValue = Int64AnimationGetValue;

        [MonoPInvokeCallback(typeof(Callback_Int64AnimationGetValue))]
        private static long Int64AnimationGetValue(IntPtr cPtr,
            long src, long dst, IntPtr clockType, IntPtr clockPtr)
        {
            try
            {
                var animation = (Int64AnimationBase)GetExtendInstance(cPtr);
                AnimationClock clock = (AnimationClock)GetProxy(clockType, clockPtr, false);
                return animation.GetCurrentValueCore(src, dst, clock);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate float Callback_DoubleAnimationGetValue(IntPtr cPtr,
            float src, float dst, IntPtr clockType, IntPtr clockPtr);
        private static Callback_DoubleAnimationGetValue _doubleAnimationGetValue = DoubleAnimationGetValue;

        [MonoPInvokeCallback(typeof(Callback_DoubleAnimationGetValue))]
        private static float DoubleAnimationGetValue(IntPtr cPtr,
            float src, float dst, IntPtr clockType, IntPtr clockPtr)
        {
            try
            {
                var animation = (DoubleAnimationBase)GetExtendInstance(cPtr);
                AnimationClock clock = (AnimationClock)GetProxy(clockType, clockPtr, false);
                return animation.GetCurrentValueCore(src, dst, clock);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ColorAnimationGetValue(IntPtr cPtr,
            ref Color src, ref Color dst, IntPtr clockType, IntPtr clockPtr, ref Color value);
        private static Callback_ColorAnimationGetValue _colorAnimationGetValue = ColorAnimationGetValue;

        [MonoPInvokeCallback(typeof(Callback_ColorAnimationGetValue))]
        private static void ColorAnimationGetValue(IntPtr cPtr,
            ref Color src, ref Color dst, IntPtr clockType, IntPtr clockPtr, ref Color value)
        {
            try
            {
                var animation = (ColorAnimationBase)GetExtendInstance(cPtr);
                AnimationClock clock = (AnimationClock)GetProxy(clockType, clockPtr, false);
                value = animation.GetCurrentValueCore(src, dst, clock);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_PointAnimationGetValue(IntPtr cPtr,
            ref Point src, ref Point dst, IntPtr clockType, IntPtr clockPtr, ref Point value);
        private static Callback_PointAnimationGetValue _pointAnimationGetValue = PointAnimationGetValue;

        [MonoPInvokeCallback(typeof(Callback_PointAnimationGetValue))]
        private static void PointAnimationGetValue(IntPtr cPtr,
            ref Point src, ref Point dst, IntPtr clockType, IntPtr clockPtr, ref Point value)
        {
            try
            {
                var animation = (PointAnimationBase)GetExtendInstance(cPtr);
                AnimationClock clock = (AnimationClock)GetProxy(clockType, clockPtr, false);
                value = animation.GetCurrentValueCore(src, dst, clock);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_RectAnimationGetValue(IntPtr cPtr,
            ref Rect src, ref Rect dst, IntPtr clockType, IntPtr clockPtr, ref Rect value);
        private static Callback_RectAnimationGetValue _rectAnimationGetValue = RectAnimationGetValue;

        [MonoPInvokeCallback(typeof(Callback_RectAnimationGetValue))]
        private static void RectAnimationGetValue(IntPtr cPtr,
            ref Rect src, ref Rect dst, IntPtr clockType, IntPtr clockPtr, ref Rect value)
        {
            try
            {
                var animation = (RectAnimationBase)GetExtendInstance(cPtr);
                AnimationClock clock = (AnimationClock)GetProxy(clockType, clockPtr, false);
                value = animation.GetCurrentValueCore(src, dst, clock);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_SizeAnimationGetValue(IntPtr cPtr,
            ref Size src, ref Size dst, IntPtr clockType, IntPtr clockPtr, ref Size value);
        private static Callback_SizeAnimationGetValue _sizeAnimationGetValue = SizeAnimationGetValue;

        [MonoPInvokeCallback(typeof(Callback_SizeAnimationGetValue))]
        private static void SizeAnimationGetValue(IntPtr cPtr,
            ref Size src, ref Size dst, IntPtr clockType, IntPtr clockPtr, ref Size value)
        {
            try
            {
                var animation = (SizeAnimationBase)GetExtendInstance(cPtr);
                AnimationClock clock = (AnimationClock)GetProxy(clockType, clockPtr, false);
                value = animation.GetCurrentValueCore(src, dst, clock);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_ThicknessAnimationGetValue(IntPtr cPtr,
            ref Thickness src, ref Thickness dst, IntPtr clockType, IntPtr clockPtr, ref Thickness value);
        private static Callback_ThicknessAnimationGetValue _thicknessAnimationGetValue = ThicknessAnimationGetValue;

        [MonoPInvokeCallback(typeof(Callback_ThicknessAnimationGetValue))]
        private static void ThicknessAnimationGetValue(IntPtr cPtr,
            ref Thickness src, ref Thickness dst, IntPtr clockType, IntPtr clockPtr, ref Thickness value)
        {
            try
            {
                var animation = (ThicknessAnimationBase)GetExtendInstance(cPtr);
                AnimationClock clock = (AnimationClock)GetProxy(clockType, clockPtr, false);
                value = animation.GetCurrentValueCore(src, dst, clock);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_ObjectAnimationGetValue(IntPtr cPtr,
            IntPtr srcType, IntPtr srcPtr, IntPtr dstType, IntPtr dstPtr,
            IntPtr clockType, IntPtr clockPtr);
        private static Callback_ObjectAnimationGetValue _objectAnimationGetValue = ObjectAnimationGetValue;

        [MonoPInvokeCallback(typeof(Callback_ObjectAnimationGetValue))]
        private static IntPtr ObjectAnimationGetValue(IntPtr cPtr,
            IntPtr srcType, IntPtr srcPtr, IntPtr dstType, IntPtr dstPtr,
            IntPtr clockType, IntPtr clockPtr)
        {
            try
            {
                var animation = (ObjectAnimationBase)GetExtendInstance(cPtr);
                object src = GetProxy(srcType, srcPtr, false);
                object dst = GetProxy(dstType, dstPtr, false);
                AnimationClock clock = (AnimationClock)GetProxy(clockType, clockPtr, false);
                object value = animation.GetCurrentValueCore(src, dst, clock);
                HandleRef valuePtr = GetInstanceHandle(value);
                BaseComponent.AddReference(valuePtr.Handle); // released by native bindings
                return valuePtr.Handle;
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return IntPtr.Zero;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate IntPtr Callback_StringAnimationGetValue(IntPtr cPtr,
            IntPtr srcPtr, IntPtr dstPtr,
            IntPtr clockType, IntPtr clockPtr);
        private static Callback_StringAnimationGetValue _stringAnimationGetValue = StringAnimationGetValue;

        [MonoPInvokeCallback(typeof(Callback_StringAnimationGetValue))]
        private static IntPtr StringAnimationGetValue(IntPtr cPtr,
            IntPtr srcPtr, IntPtr dstPtr,
            IntPtr clockType, IntPtr clockPtr)
        {
            try
            {
                var animation = (StringAnimationBase)GetExtendInstance(cPtr);
                string src = StringFromNativeUtf8(srcPtr);
                string dst = StringFromNativeUtf8(dstPtr);
                AnimationClock clock = (AnimationClock)GetProxy(clockType, clockPtr, false);
                string value = animation.GetCurrentValueCore(src, dst, clock);
                return Marshal.StringToHGlobalUni(value != null ? value : string.Empty);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return IntPtr.Zero;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_MatrixAnimationGetValue(IntPtr cPtr,
            ref Matrix src, ref Matrix dst, IntPtr clockType, IntPtr clockPtr, ref Matrix value);
        private static Callback_MatrixAnimationGetValue _matrixAnimationGetValue = MatrixAnimationGetValue;

        [MonoPInvokeCallback(typeof(Callback_MatrixAnimationGetValue))]
        private static void MatrixAnimationGetValue(IntPtr cPtr,
            ref Matrix src, ref Matrix dst, IntPtr clockType, IntPtr clockPtr, ref Matrix value)
        {
            try
            {
                var animation = (MatrixAnimationBase)GetExtendInstance(cPtr);
                AnimationClock clock = (AnimationClock)GetProxy(clockType, clockPtr, false);
                value = animation.GetCurrentValueCore(src, dst, clock);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private enum NativePropertyType
        {
            Bool,
            Float,
            Double,
            Long,
            ULong,
            Int,
            UInt,
            Short,
            UShort,
            Color,
            Point,
            Rect,
            Int32Rect,
            Size,
            Thickness,
            CornerRadius,
            TimeSpan,
            Duration,
            KeyTime,
            NullableBool,
            NullableFloat,
            NullableDouble,
            NullableLong,
            NullableULong,
            NullableInt,
            NullableUInt,
            NullableShort,
            NullableUShort,
            NullableColor,
            NullablePoint,
            NullableRect,
            NullableInt32Rect,
            NullableSize,
            NullableThickness,
            NullableCornerRadius,
            NullableTimeSpan,
            NullableDuration,
            NullableKeyTime,
            Enum,
            String,
            Uri,
            Type,
            BaseComponent,
            Event
        };

        ////////////////////////////////////////////////////////////////////////////////////////////////
        internal static int GetNativePropertyType(Type type)
        {
            if (type.Equals(typeof(string)))
            {
                return (int)NativePropertyType.String;
            }

            if (type.Equals(typeof(bool)))
            {
                return (int)NativePropertyType.Bool;
            }

            if (type.Equals(typeof(float)))
            {
                return (int)NativePropertyType.Float;
            }

            if (type.Equals(typeof(double)) ||
                type.Equals(typeof(decimal)))
            {
                return (int)NativePropertyType.Double;
            }

            if (type.Equals(typeof(int)) ||
                type.Equals(typeof(long)))
            {
                return (int)NativePropertyType.Int;
            }

            if (type.Equals(typeof(uint)) ||
                type.Equals(typeof(ulong)) ||
                type.Equals(typeof(char)))
            {
                return (int)NativePropertyType.UInt;
            }

            if (type.Equals(typeof(short)) ||
                type.Equals(typeof(sbyte)))
            {
                return (int)NativePropertyType.Short;
            }

            if (type.Equals(typeof(ushort)) ||
                type.Equals(typeof(byte)))
            {
                return (int)NativePropertyType.UShort;
            }

            if (type.Equals(typeof(Color)))
            {
                return (int)NativePropertyType.Color;
            }

            if (type.Equals(typeof(Point)))
            {
                return (int)NativePropertyType.Point;
            }

            if (type.Equals(typeof(Rect)))
            {
                return (int)NativePropertyType.Rect;
            }

            if (type.Equals(typeof(Int32Rect)))
            {
                return (int)NativePropertyType.Int32Rect;
            }

            if (type.Equals(typeof(Size)))
            {
                return (int)NativePropertyType.Size;
            }

            if (type.Equals(typeof(Thickness)))
            {
                return (int)NativePropertyType.Thickness;
            }

            if (type.Equals(typeof(Noesis.CornerRadius)))
            {
                return (int)NativePropertyType.CornerRadius;
            }

            if (type.Equals(typeof(System.TimeSpan)))
            {
                return (int)NativePropertyType.TimeSpan;
            }

            if (type.Equals(typeof(Noesis.Duration)))
            {
                return (int)NativePropertyType.Duration;
            }

            if (type.Equals(typeof(Noesis.KeyTime)))
            {
                return (int)NativePropertyType.KeyTime;
            }

            if (type.Equals(typeof(bool?)))
            {
                return (int)NativePropertyType.NullableBool;
            }

            if (type.Equals(typeof(float?)))
            {
                return (int)NativePropertyType.NullableFloat;
            }

            if (type.Equals(typeof(double?)) ||
                type.Equals(typeof(decimal?)))
            {
                return (int)NativePropertyType.NullableDouble;
            }

            if (type.Equals(typeof(int?)) ||
                type.Equals(typeof(long?)))
            {
                return (int)NativePropertyType.NullableInt;
            }

            if (type.Equals(typeof(uint?)) ||
                type.Equals(typeof(ulong?)) ||
                type.Equals(typeof(char?)))
            {
                return (int)NativePropertyType.NullableUInt;
            }

            if (type.Equals(typeof(short?)) ||
                type.Equals(typeof(sbyte?)))
            {
                return (int)NativePropertyType.NullableShort;
            }

            if (type.Equals(typeof(ushort?)) ||
                type.Equals(typeof(byte?)))
            {
                return (int)NativePropertyType.NullableUShort;
            }

            if (type.Equals(typeof(Color?)))
            {
                return (int)NativePropertyType.NullableColor;
            }

            if (type.Equals(typeof(Point?)))
            {
                return (int)NativePropertyType.NullablePoint;
            }

            if (type.Equals(typeof(Rect?)))
            {
                return (int)NativePropertyType.NullableRect;
            }

            if (type.Equals(typeof(Int32Rect?)))
            {
                return (int)NativePropertyType.NullableInt32Rect;
            }

            if (type.Equals(typeof(Size?)))
            {
                return (int)NativePropertyType.NullableSize;
            }

            if (type.Equals(typeof(Thickness?)))
            {
                return (int)NativePropertyType.NullableThickness;
            }

            if (type.Equals(typeof(Noesis.CornerRadius?)))
            {
                return (int)NativePropertyType.NullableCornerRadius;
            }

            if (type.Equals(typeof(System.TimeSpan?)))
            {
                return (int)NativePropertyType.NullableTimeSpan;
            }

            if (type.Equals(typeof(Noesis.Duration?)))
            {
                return (int)NativePropertyType.NullableDuration;
            }

            if (type.Equals(typeof(Noesis.KeyTime?)))
            {
                return (int)NativePropertyType.NullableKeyTime;
            }

            if (type.GetTypeInfo().IsEnum)
            {
                EnsureNativeType(type);
                return (int)NativePropertyType.Enum;
            }

            return (int)NativePropertyType.BaseComponent;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static PropertyAccessor GetProperty(IntPtr nativeType, int propertyIndex)
        {
            NativeTypePropsInfo info = (NativeTypePropsInfo)GetNativeTypeInfo(nativeType);
            PropertyAccessor prop = info.Properties[propertyIndex];
            return prop;
        }

        private static T GetPropertyValue<T>(PropertyAccessor prop, object instance)
        {
            return instance != null ? ((PropertyAccessorT<T>)prop).Get(instance) : default(T);
        }

        private static T GetPropertyValueNullable<T>(PropertyAccessor prop, object instance,
            out bool isNull) where T: struct
        {
            if (!prop.IsNullable)
            {
                isNull = false;
                return GetPropertyValue<T>(prop, instance);
            }
            else
            {
                T? value = GetPropertyValue<T?>(prop, instance);
                isNull = !value.HasValue;
                return value.GetValueOrDefault();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool Callback_GetPropertyValue_Bool(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Bool _getPropertyValue_Bool = GetPropertyValue_Bool;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Bool))]
        private static bool GetPropertyValue_Bool(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref bool isNull)
        {
            try
            {
                return GetPropertyValueNullable<bool>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return false;
            }
        }

        private delegate float Callback_GetPropertyValue_Float(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Float _getPropertyValue_Float = GetPropertyValue_Float;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Float))]
        private static float GetPropertyValue_Float(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref bool isNull)
        {
            try
            {
                return GetPropertyValueNullable<float>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0.0f;
            }
        }

        private delegate double Callback_GetPropertyValue_Double(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Double _getPropertyValue_Double = GetPropertyValue_Double;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Double))]
        private static double GetPropertyValue_Double(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref bool isNull)
        {
            try
            {
                return GetPropertyValueNullable<double>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0.0f;
            }
        }

        private delegate long Callback_GetPropertyValue_Int64(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Int64 _getPropertyValue_Int64 = GetPropertyValue_Int64;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Int64))]
        private static long GetPropertyValue_Int64(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref bool isNull)
        {
            try
            {
                return GetPropertyValueNullable<long>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0;
            }
        }

        private delegate ulong Callback_GetPropertyValue_UInt64(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_UInt64 _getPropertyValue_UInt64 = GetPropertyValue_UInt64;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_UInt64))]
        private static ulong GetPropertyValue_UInt64(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref bool isNull)
        {
            try
            {
                return GetPropertyValueNullable<ulong>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0;
            }
        }

        private delegate int Callback_GetPropertyValue_Int(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Int _getPropertyValue_Int = GetPropertyValue_Int;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Int))]
        private static int GetPropertyValue_Int(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref bool isNull)
        {
            try
            {
                return GetPropertyValueNullable<int>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0;
            }
        }

        private delegate uint Callback_GetPropertyValue_UInt(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_UInt _getPropertyValue_UInt = GetPropertyValue_UInt;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_UInt))]
        private static uint GetPropertyValue_UInt(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref bool isNull)
        {
            try
            {
                return GetPropertyValueNullable<uint>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0;
            }
        }

        private delegate short Callback_GetPropertyValue_Short(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Short _getPropertyValue_Short = GetPropertyValue_Short;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Short))]
        private static short GetPropertyValue_Short(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref bool isNull)
        {
            try
            {
                return GetPropertyValueNullable<short>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0;
            }
        }

        private delegate ushort Callback_GetPropertyValue_UShort(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_UShort _getPropertyValue_UShort = GetPropertyValue_UShort;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_UShort))]
        private static ushort GetPropertyValue_UShort(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull)
        {
            try
            {
                return GetPropertyValueNullable<ushort>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0;
            }
        }

        private delegate IntPtr Callback_GetPropertyValue_String(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr);
        private static Callback_GetPropertyValue_String _getPropertyValue_String = GetPropertyValue_String;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_String))]
        private static IntPtr GetPropertyValue_String(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr)
        {
            try
            {
                string str = GetPropertyValue<string>(GetProperty(nativeType, propertyIndex), GetExtendInstance(cPtr));
                return Marshal.StringToHGlobalUni(str != null ? str : string.Empty);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return Marshal.StringToHGlobalUni(string.Empty);
            }
        }

        private delegate IntPtr Callback_GetPropertyValue_Uri(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr);
        private static Callback_GetPropertyValue_Uri _getPropertyValue_Uri = GetPropertyValue_Uri;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Uri))]
        private static IntPtr GetPropertyValue_Uri(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr)
        {
            try
            {
                Uri uri = GetPropertyValue<Uri>(GetProperty(nativeType, propertyIndex), GetExtendInstance(cPtr));
                return Marshal.StringToHGlobalUni(uri != null ? uri.OriginalString : string.Empty);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return Marshal.StringToHGlobalUni(string.Empty);
            }
        }

        private delegate void Callback_GetPropertyValue_Color(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Color value, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Color _getPropertyValue_Color = GetPropertyValue_Color;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Color))]
        private static void GetPropertyValue_Color(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Color value, ref bool isNull)
        {
            try
            {
                value = GetPropertyValueNullable<Color>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_Point(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Point value, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Point _getPropertyValue_Point = GetPropertyValue_Point;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Point))]
        private static void GetPropertyValue_Point(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Point value, ref bool isNull)
        {
            try
            {
                value = GetPropertyValueNullable<Point>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_Rect(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Rect value, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Rect _getPropertyValue_Rect = GetPropertyValue_Rect;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Rect))]
        private static void GetPropertyValue_Rect(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Rect value, ref bool isNull)
        {
            try
            {
                value = GetPropertyValueNullable<Rect>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_Int32Rect(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Int32Rect value, [MarshalAs(UnmanagedType.U1)] ref bool isNull);
        private static Callback_GetPropertyValue_Int32Rect _getPropertyValue_Int32Rect = GetPropertyValue_Int32Rect;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Int32Rect))]
        private static void GetPropertyValue_Int32Rect(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Int32Rect value, ref bool isNull)
        {
            try
            {
                value = GetPropertyValueNullable<Int32Rect>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_Size(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Size value, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Size _getPropertyValue_Size = GetPropertyValue_Size;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Size))]
        private static void GetPropertyValue_Size(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Size value, ref bool isNull)
        {
            try
            {
                value = GetPropertyValueNullable<Size>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_Thickness(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Thickness value, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Thickness _getPropertyValue_Thickness = GetPropertyValue_Thickness;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Thickness))]
        private static void GetPropertyValue_Thickness(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Thickness value, ref bool isNull)
        {
            try
            {
                value = GetPropertyValueNullable<Thickness>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_CornerRadius(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref CornerRadius value, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_CornerRadius _getPropertyValue_CornerRadius = GetPropertyValue_CornerRadius;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_CornerRadius))]
        private static void GetPropertyValue_CornerRadius(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref CornerRadius value, ref bool isNull)
        {
            try
            {
                value = GetPropertyValueNullable<CornerRadius>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_TimeSpan(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref TimeSpanStruct value, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_TimeSpan _getPropertyValue_TimeSpan = GetPropertyValue_TimeSpan;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_TimeSpan))]
        private static void GetPropertyValue_TimeSpan(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref TimeSpanStruct value, ref bool isNull)
        {
            try
            {
                value = GetPropertyValueNullable<TimeSpanStruct>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_Duration(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Duration value, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Duration _getPropertyValue_Duration = GetPropertyValue_Duration;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Duration))]
        private static void GetPropertyValue_Duration(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Duration value, ref bool isNull)
        {
            try
            {
                value = GetPropertyValueNullable<Duration>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_KeyTime(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref KeyTime value, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_KeyTime _getPropertyValue_KeyTime = GetPropertyValue_KeyTime;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_KeyTime))]
        private static void GetPropertyValue_KeyTime(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref KeyTime value, ref bool isNull)
        {
            try
            {
                value = GetPropertyValueNullable<KeyTime>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate IntPtr Callback_GetPropertyValue_Type(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr);
        private static Callback_GetPropertyValue_Type _getPropertyValue_Type = GetPropertyValue_Type;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Type))]
        private static IntPtr GetPropertyValue_Type(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr)
        {
            try
            {
                Type type = GetPropertyValue<Type>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr));
                return GetNativeType(type);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return IntPtr.Zero;
            }
        }

        private delegate IntPtr Callback_GetPropertyValue_BaseComponent(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr);
        private static Callback_GetPropertyValue_BaseComponent _getPropertyValue_BaseComponent = GetPropertyValue_BaseComponent;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_BaseComponent))]
        private static IntPtr GetPropertyValue_BaseComponent(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr)
        {
            try
            {
                object obj = GetPropertyValue<object>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr));
                return GetInstanceHandle(obj).Handle;
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return IntPtr.Zero;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static void SetPropertyValue<T>(PropertyAccessor prop, object instance, T value)
        {
            if (instance != null)
            {
                ((PropertyAccessorT<T>)prop).Set(instance, value);
            }
        }

        private static void SetPropertyValueNullable<T>(PropertyAccessor prop, object instance,
            T value, bool isNull) where T: struct
        {
            if (!prop.IsNullable)
            {
                SetPropertyValue<T>(prop, instance, value);
            }
            else
            {
                SetPropertyValue<T?>(prop, instance, isNull ? null : (T?)value);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_SetPropertyValue_Bool(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, [MarshalAs(UnmanagedType.U1)] bool val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Bool _setPropertyValue_Bool = SetPropertyValue_Bool;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Bool))]
        private static void SetPropertyValue_Bool(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, bool val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<bool>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Float(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, float val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Float _setPropertyValue_Float = SetPropertyValue_Float;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Float))]
        private static void SetPropertyValue_Float(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, float val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<float>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Double(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, double val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Double _setPropertyValue_Double = SetPropertyValue_Double;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Double))]
        private static void SetPropertyValue_Double(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, double val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<double>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Int64(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, long val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Int64 _setPropertyValue_Int64 = SetPropertyValue_Int64;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Int64))]
        private static void SetPropertyValue_Int64(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, long val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<long>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_UInt64(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ulong val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_UInt64 _setPropertyValue_UInt64 = SetPropertyValue_UInt64;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_UInt64))]
        private static void SetPropertyValue_UInt64(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ulong val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<ulong>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Int(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, int val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Int _setPropertyValue_Int = SetPropertyValue_Int;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Int))]
        private static void SetPropertyValue_Int(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, int val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<int>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_UInt(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, uint val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_UInt _setPropertyValue_UInt = SetPropertyValue_UInt;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_UInt))]
        private static void SetPropertyValue_UInt(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, uint val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<uint>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Short(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, short val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Short _setPropertyValue_Short = SetPropertyValue_Short;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Short))]
        private static void SetPropertyValue_Short(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, short val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<short>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_UShort(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ushort val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_UShort _setPropertyValue_UShort = SetPropertyValue_UShort;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_UShort))]
        private static void SetPropertyValue_UShort(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ushort val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<ushort>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_String(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val);
        private static Callback_SetPropertyValue_String _setPropertyValue_String = SetPropertyValue_String;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_String))]
        private static void SetPropertyValue_String(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val)
        {
            try
            {
                SetPropertyValue<string>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), StringFromNativeUtf8(val));
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Uri(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val);
        private static Callback_SetPropertyValue_Uri _setPropertyValue_Uri = SetPropertyValue_Uri;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Uri))]
        private static void SetPropertyValue_Uri(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val)
        {
            try
            {
                Uri uri = new Uri(StringFromNativeUtf8(val), UriKind.RelativeOrAbsolute);
                SetPropertyValue<Uri>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), uri);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Color(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Color val, [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Color _setPropertyValue_Color = SetPropertyValue_Color;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Color))]
        private static void SetPropertyValue_Color(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Color val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<Color>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Point(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Point val, [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Point _setPropertyValue_Point = SetPropertyValue_Point;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Point))]
        private static void SetPropertyValue_Point(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Point val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<Point>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Rect(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Rect val, [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Rect _setPropertyValue_Rect = SetPropertyValue_Rect;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Rect))]
        private static void SetPropertyValue_Rect(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Rect val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<Rect>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Int32Rect(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Int32Rect val, [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Int32Rect _setPropertyValue_Int32Rect = SetPropertyValue_Int32Rect;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Int32Rect))]
        private static void SetPropertyValue_Int32Rect(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Int32Rect val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<Int32Rect>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Size(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Size val, [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Size _setPropertyValue_Size = SetPropertyValue_Size;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Size))]
        private static void SetPropertyValue_Size(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Size val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<Size>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Thickness(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Thickness val, [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Thickness _setPropertyValue_Thickness = SetPropertyValue_Thickness;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Thickness))]
        private static void SetPropertyValue_Thickness(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Thickness val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<Thickness>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_CornerRadius(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref CornerRadius val, [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_CornerRadius _setPropertyValue_CornerRadius = SetPropertyValue_CornerRadius;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_CornerRadius))]
        private static void SetPropertyValue_CornerRadius(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref CornerRadius val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<CornerRadius>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_TimeSpan(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref TimeSpanStruct val, [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_TimeSpan _setPropertyValue_TimeSpan = SetPropertyValue_TimeSpan;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_TimeSpan))]
        private static void SetPropertyValue_TimeSpan(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref TimeSpanStruct val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<TimeSpanStruct>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Duration(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Duration val, [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Duration _setPropertyValue_Duration = SetPropertyValue_Duration;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Duration))]
        private static void SetPropertyValue_Duration(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref Duration val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<Duration>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_KeyTime(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref KeyTime val, [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_KeyTime _setPropertyValue_KeyTime = SetPropertyValue_KeyTime;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_KeyTime))]
        private static void SetPropertyValue_KeyTime(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, ref KeyTime val, bool isNull)
        {
            try
            {
                SetPropertyValueNullable<KeyTime>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Type(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val);
        private static Callback_SetPropertyValue_Type _setPropertyValue_Type = SetPropertyValue_Type;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Type))]
        private static void SetPropertyValue_Type(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val)
        {
            try
            {
                NativeTypeInfo info = GetNativeTypeInfo(cPtr);
                SetPropertyValue<Type>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), info.Type);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_BaseComponent(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valType, IntPtr val);
        private static Callback_SetPropertyValue_BaseComponent _setPropertyValue_BaseComponent = SetPropertyValue_BaseComponent;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_BaseComponent))]
        private static void SetPropertyValue_BaseComponent(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valType, IntPtr val)
        {
            try
            {
                SetPropertyValue<object>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), GetProxy(valType, val, false));
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [ThreadStatic]
        private static IntPtr _cPtr = IntPtr.Zero;
        [ThreadStatic]
        private static Type _extendType = null;

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool NeedsCreateCPtr(Type extendType)
        {
            return _cPtr == IntPtr.Zero || _extendType != extendType;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static IntPtr GetCPtr(BaseComponent instance, Type extendType)
        {
            if (_cPtr == IntPtr.Zero)
            {
                throw new InvalidOperationException("cPtr is null");
            }

            if (_extendType != extendType)
            {
                throw new InvalidOperationException("Invalid extend type");
            }

            return _cPtr;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static IntPtr NewCPtr(System.Type type)
        {
            // Ensure native type is registered
            IntPtr nativeType = EnsureNativeType(type);

            // This function is called when a Extend object is created from C#
            // so we create the corresponding C++ proxy
            IntPtr cPtr = Noesis_InstantiateExtend(nativeType);

            if (cPtr == IntPtr.Zero)
            {
                throw new System.Exception(String.Format("Unable to create an instance of '{0}'",
                    type.FullName));
            }

            return cPtr;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_CreateInstance(IntPtr nativeType, IntPtr cPtr);
        private static Callback_CreateInstance _createInstance = CreateInstance;

        [MonoPInvokeCallback(typeof(Callback_CreateInstance))]
        private static void CreateInstance(IntPtr nativeType, IntPtr cPtr)
        {
            try
            {
                NativeTypeExtendedInfo info = (NativeTypeExtendedInfo)GetNativeTypeInfo(nativeType);

                bool isBaseComponent = typeof(Noesis.BaseComponent).GetTypeInfo().IsAssignableFrom(info.Type.GetTypeInfo());
                if (isBaseComponent)
                {
                    _cPtr = cPtr;
                    _extendType = info.Type;
                }

                // This function is called when a Extend object is created from C++ so we create the
                // corresponding C# proxy
                if (info.Creator == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "Can't create an instance of {0}, default constructor is not available",
                        info.Type.FullName));
                }
                object instance = info.Creator();

                if (isBaseComponent)
                {
                    _cPtr = IntPtr.Zero;
                    _extendType = null;
                }
                else
                {
                    // For Extended objects not inheriting from BaseComponent that are created from
                    // C++ side, we need to manually add the reference that is holded by this object
                    AddExtendInfo(cPtr, instance);
                    BaseComponent.AddReference(cPtr);
                    RegisterInterfaces(instance);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_DeleteInstance(IntPtr cPtr);
        private static Callback_DeleteInstance _deleteInstance = DeleteInstance;

        [MonoPInvokeCallback(typeof(Callback_DeleteInstance))]
        private static void DeleteInstance(IntPtr cPtr)
        {
            try
            {
                RemoveExtendInfo(cPtr);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_GrabInstance(IntPtr cPtr, [MarshalAs(UnmanagedType.U1)] bool grab);
        private static Callback_GrabInstance _grabInstance = GrabInstance;

        [MonoPInvokeCallback(typeof(Callback_GrabInstance))]
        private static void GrabInstance(IntPtr cPtr, bool grab)
        {
            try
            {
                if (Initialized)
                {
                    ExtendInfo extend = GetExtendInfo(cPtr);
                    if (extend != null)
                    {
                        if (grab)
                        {
                            extend.instance = extend.weak.Target;
                        }
                        else
                        {
                            extend.instance = null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool IsGrabbed(object o)
        {
            lock (_extends)
                return _extends.Any(x => Object.ReferenceEquals(x.Value.instance, o));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void RegisterExtendInstance(BaseComponent instance)
        {
            IntPtr cPtr = BaseComponent.getCPtr(instance).Handle;
            AddExtendInfo(cPtr, instance);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static object GetExtendInstance(IntPtr cPtr)
        {
            ExtendInfo extend = GetExtendInfo(cPtr);
            return extend != null ? extend.weak.Target : null;
        }

        public static object GetExtendInstance(IntPtr cPtr, bool ownMemory)
        {
            object instance = null;

            ExtendInfo extend = GetExtendInfo(cPtr);
            if (extend != null)
            {
                instance = extend.weak.Target;
            }

            if (ownMemory)
            {
                BaseComponent.Release(cPtr);
            }

            return instance;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static ExtendInfo GetExtendInfo(IntPtr cPtr)
        {
            ExtendInfo extend;

            lock (_extends)
            {
                if (!_extends.TryGetValue(cPtr.ToInt64(), out extend))
                {
                    if (Initialized)
                    {
                        Log.Error("Extend already removed");
                    }
                    return null;
                }
                else if (extend == null || extend.weak.Target == null)
                {
                    if (Initialized)
                    {
                        Log.Error("Extend already destroyed");
                    }
                    return null;
                }
            }

            return extend;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static void AddExtendInfo(IntPtr cPtr, object instance)
        {
            if (cPtr == IntPtr.Zero)
            {
                throw new InvalidOperationException(
                    "Native pointer of registered extend instance is null");
            }

            ExtendInfo extend = new ExtendInfo { weak = new WeakReference(instance) };

            lock (_extends)
            {
                _extends.Add(cPtr.ToInt64(), extend);

                if (!(instance is BaseComponent))
                {
                    _weakExtends.Add(instance, new ExtendNotifier { cPtr = cPtr });
                }
            }
        }

        private static void RemoveExtendInfo(IntPtr cPtr)
        {
            lock (_extends)
            {
                _extends.Remove(cPtr.ToInt64());
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private class ExtendInfo
        {
            public object instance;
            public WeakReference weak;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static Dictionary<long, ExtendInfo> _extends = new Dictionary<long, ExtendInfo>();

        private class ExtendNotifier
        {
            public IntPtr cPtr;

            ~ExtendNotifier()
            {
                // There is a weird situation in NETSTANDARD (a possible bug) that destroys the
                // value even when the key is still alive. The workaround is to re-register the
                // key instance in the ConditionalWeakTable until the key is really destroyed
                object instance = null;
                lock (_extends)
                {
                    if (_extends.TryGetValue(cPtr.ToInt64(), out ExtendInfo extend) &&
                        extend.weak.IsAlive)
                    {
                        instance = extend.weak.Target;
                    }
                }

                if (instance != null)
                {
                    _weakExtends.Remove(instance);
                    _weakExtends.Add(instance, this);
                    GC.ReRegisterForFinalize(this);
                }
                else
                {
                    AddPendingRelease(cPtr);
                }
            }
        }

        private static ConditionalWeakTable<object, ExtendNotifier> _weakExtends =
            new ConditionalWeakTable<object, ExtendNotifier>();

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static BaseComponent GetProxyInstance(IntPtr cPtr, bool ownMemory, NativeTypeInfo info)
        {
            if (cPtr == DependencyProperty.UnsetValuePtr)
            {
                return (BaseComponent)DependencyProperty.UnsetValue;
            }
            if (cPtr == Binding.DoNothingPtr)
            {
                return (BaseComponent)Binding.DoNothing;
            }

            lock (_proxies)
            {
                WeakReference wr;
                long ptr = cPtr.ToInt64();
                if (_proxies.TryGetValue(ptr, out wr))
                {
                    if (wr != null)
                    {
                        BaseComponent component = (BaseComponent)wr.Target;
                        if (component == null)
                        {
                            _proxies.Remove(ptr);
                        }
                        else
                        {
                            if (ownMemory)
                            {
                                BaseComponent.Release(cPtr);
                            }

                            return component;
                        }
                    }
                }
            }

            if (BaseComponent.GetNumReferences(cPtr) > 0)
            {
                return ((NativeTypeComponentInfo)info).Creator(cPtr, ownMemory);
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static BaseComponent AddProxy(BaseComponent instance)
        {
            IntPtr cPtr = BaseComponent.getCPtr(instance).Handle;
            long ptr = cPtr.ToInt64();
            lock (_proxies)
            {
                if (_proxies.ContainsKey(ptr))
                {
                    _proxies.Remove(ptr);
                }
                _proxies.Add(ptr, new WeakReference(instance));
            }

            return instance;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void RemoveProxy(IntPtr cPtr)
        {
            long ptr = cPtr.ToInt64();
            lock (_proxies)
            {
                WeakReference wr;
                if (_proxies.TryGetValue(ptr, out wr))
                {
                    // A  new proxy may be created for the same cPtr while the previous proxy is
                    // pending for GC. When its Finalizer is called, we have to make sure that a new
                    // proxy is not stored before removing the entry
                    if (wr != null)
                    {
                        if (wr.Target == null)
                        {
                            _proxies.Remove(ptr);
                        }
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        static Dictionary<long, WeakReference> _proxies = new Dictionary<long, WeakReference>();

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static HandleRef GetInstanceHandle(object instance)
        {
            if (instance != null)
            {
                IntPtr cPtr = Box(instance);

                if (cPtr == IntPtr.Zero)
                {
                    cPtr = FindInstancePtr(instance);
                    if (cPtr == IntPtr.Zero && Initialized)
                    {
                        cPtr = NewCPtr(instance.GetType());
                        AddExtendInfo(cPtr, instance);
                        RegisterInterfaces(instance);

                        #if UNITY_5_3_OR_NEWER
                        // Automatic conversion from Unity's Texture to a TextureSource proxy
                        if (instance is UnityEngine.Texture texture)
                        {
                            Noesis.TextureSource.SetTexture(cPtr, texture);
                        }
                        // Automatic conversion from Unity's Sprite to a CroppedBitmap proxy
                        else if (instance is UnityEngine.Sprite sprite && sprite != null)
                        {
                            UnityEngine.Rect spriteRect = sprite.packed && sprite.packingMode == UnityEngine.SpritePackingMode.Rectangle ?
                                sprite.textureRect : sprite.rect;

                            Int32Rect rect = new Int32Rect(
                                (int)spriteRect.x,
                                (int)(sprite.texture.height - spriteRect.height - spriteRect.y),
                                (int)spriteRect.width,
                                (int)spriteRect.height);

                            HandleRef bmp = new HandleRef(instance, cPtr);
                            NoesisGUI_PINVOKE.CroppedBitmap_Source_set(bmp, GetInstanceHandle(sprite.texture));
                            NoesisGUI_PINVOKE.CroppedBitmap_SourceRect_set(bmp, ref rect);
                        }
                        #endif
                    }
                }

                return new HandleRef(instance, cPtr);
            }
            else
            {
                return new HandleRef(null, IntPtr.Zero);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static IntPtr FindInstancePtr(object instance)
        {
            IntPtr cPtr = IntPtr.Zero;

            if (instance is BaseComponent)
            {
                cPtr = BaseComponent.getCPtr((BaseComponent)instance).Handle;
            }
            else
            {
                cPtr = FindWeakInstancePtr(instance);
            }

            return cPtr;
        }

        private static IntPtr FindWeakInstancePtr(object instance)
        {
            IntPtr cPtr = IntPtr.Zero;

            ExtendNotifier notifier;
            if (_weakExtends.TryGetValue(instance, out notifier))
            {
                cPtr = notifier.cPtr;
            }

            return cPtr;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static List<IntPtr> _pendingRelease = new List<IntPtr>(256);

        public static void AddPendingRelease(IntPtr cPtr)
        {
            lock (_pendingRelease)
            {
                _pendingRelease.Add(cPtr);
            }
        }

        private static void ReleasePending()
        {
            lock (_pendingRelease)
            {
                int numPending = _pendingRelease.Count;
                for (int i = 0; i < numPending; ++i)
                {
                    BaseComponent.Release(_pendingRelease[i]);
                }
                _pendingRelease.RemoveRange(0, numPending);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void Update()
        {
            ReleasePending();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void RegisterInterfaces(object instance)
        {
            // For INotifyPropertyChanged objects, we need to hook to the PropertyChanged event
            // so we can notify C++ side when a property is changed in Mono
            System.ComponentModel.INotifyPropertyChanged notifyP =
                instance as System.ComponentModel.INotifyPropertyChanged;
            if (notifyP != null)
            {
                notifyP.PropertyChanged += NotifyPropertyChanged;
            }

            // For INotifyCollectionChanged objects, we need to hook to the CollectionChanged event
            // so we can notify C++ side when collection has changed in Mono
            System.Collections.Specialized.INotifyCollectionChanged notifyC =
                instance as System.Collections.Specialized.INotifyCollectionChanged;
            if (notifyC != null)
            {
                notifyC.CollectionChanged += NotifyCollectionChanged;
            }

            // For ICommand objects, we need to hook to the CanExecuteChanged event so we can
            // notify C++ side when command.CanExecute has changed in Mono
            ICommand command = instance as ICommand;
            if (command != null)
            {
                command.CanExecuteChanged += NotifyCanExecuteChanged;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static void NotifyPropertyChanged(object sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            // We don't want to raise more property change notifications after shutdown is called
            if (Initialized)
            {
                IntPtr nativeType = EnsureNativeType(sender.GetType());

                Noesis_LaunchPropertyChangedEvent(nativeType, GetInstanceHandle(sender).Handle,
                    e.PropertyName != null ? e.PropertyName : string.Empty);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static void NotifyCollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // We don't want to raise more property change notifications after shutdown is called
            if (Initialized)
            {
                IntPtr nativeType = EnsureNativeType(sender.GetType());

                object newItem = (e.NewItems != null && e.NewItems.Count > 0 ? e.NewItems[0] : null);
                object oldItem = (e.OldItems != null && e.OldItems.Count > 0 ? e.OldItems[0] : null);

                Noesis_LaunchCollectionChangedEvent(nativeType, GetInstanceHandle(sender).Handle,
                    (int)e.Action, GetInstanceHandle(newItem).Handle, GetInstanceHandle(oldItem).Handle,
                    e.NewStartingIndex, e.OldStartingIndex);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static void NotifyCanExecuteChanged(object sender, System.EventArgs e)
        {
            if (Initialized)
            {
                IntPtr nativeType = EnsureNativeType(sender.GetType());

                Noesis_LaunchCanExecuteChangedEvent(nativeType, GetInstanceHandle(sender).Handle);
            }
        }
    }
}
