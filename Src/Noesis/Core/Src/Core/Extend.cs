using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Input;

namespace Noesis
{
    ////////////////////////////////////////////////////////////////////////////////////////////////
    // Manages Noesis Extensibility
    ////////////////////////////////////////////////////////////////////////////////////////////////
    internal partial class Extend
    {
        static Extend()
        {
            Noesis.GUI.Init();
        }

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

                    EventHandlerStore.Clear(_extends
                        .Where(kv => kv.Value.weak.Target is EventHandlerStore)
                        .Select(kv => kv.Value.weak.Target)
                        .Cast<EventHandlerStore>()
                        .ToArray());

                    Noesis.GUI.SetApplicationResources(null);

                    var pendingReleases = new List<KeyValuePair<long, WeakReference>>();

                    pendingReleases.AddRange(_proxies);

                    foreach (var kv in _extends)
                    {
                        pendingReleases.Add(new KeyValuePair<long, WeakReference>(kv.Key, kv.Value.weak));

                        if (kv.Value.instance != null && !(kv.Value.instance is BaseComponent))
                        {
                            WeakInfo info = new WeakInfo
                            {
                                hash = RuntimeHelpers.GetHashCode(kv.Value.instance),
                                weak = kv.Value.weak
                            };
                            _weakExtends.Add(kv.Key, info);

                            _weakExtendsHash.Remove(info.hash);
                            _weakExtendsHash.Add(info.hash, kv.Key);

                            _extendPtrs.Remove(kv.Value.instance);
                        }

                        kv.Value.instance = null;
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    AddDestroyedExtends();

                    foreach (var kv in pendingReleases)
                    {
                        object instance = kv.Value.Target;
                        if (instance != null)
                        {
                            BaseComponent.ForceRelease(instance, new IntPtr(kv.Key));
                        }
                    }

                    ReleasePending();
                }
                catch (Exception)
                {
                    // clear errors generated releasing all C# proxies, throwing during
                    // assembly unload will close Unity without notifying
                }

                Noesis_ClearExtendTypes();

                ClearTables();

                Noesis_EnableExtend(false);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void RegisterCallbacks()
        {
            // register callbacks
            Noesis_RegisterReflectionCallbacks(

                _freeString,

                _registerType,

                _onPostInit,
                _dependencyPropertyChanged,
                _frameworkElementMeasure,
                _frameworkElementArrange,
                _frameworkElementConnectEvent,
                _freezableClone,

                _toString,
                _equals,
                _getHashCode,

                _commandCanExecute,
                _commandExecute,

                _converterConvert,
                _converterConvertBack,

                _listCount,
                _listGet,
                _listSet,
                _listAdd,
                _listIndexOf,

                _dictionaryFind,
                _dictionarySet,
                _dictionaryAdd,

                _listIndexerTryGet,
                _listIndexerTrySet,

                _dictionaryIndexerTryGet,
                _dictionaryIndexerTrySet,

                _selectTemplate,

                _streamSetPosition,
                _streamGetPosition,
                _streamGetLength,
                _streamRead,

                _providerLoadXaml,
                _providerTextureInfo,
                _providerTextureLoad,
                _providerTextureOpen,
                _providerScanFolder,
                _providerOpenFont,

                _getPropertyInfo,

                _getPropertyValue_Bool,
                _getPropertyValue_Float,
                _getPropertyValue_Double,
                _getPropertyValue_Int,
                _getPropertyValue_UInt,
                _getPropertyValue_Short,
                _getPropertyValue_UShort,
                _getPropertyValue_String,
                _getPropertyValue_Uri,
                _getPropertyValue_Color,
                _getPropertyValue_Point,
                _getPropertyValue_Rect,
                _getPropertyValue_Size,
                _getPropertyValue_Thickness,
                _getPropertyValue_CornerRadius,
                _getPropertyValue_TimeSpan,
                _getPropertyValue_Duration,
                _getPropertyValue_KeyTime,
                _getPropertyValue_BaseComponent,

                _setPropertyValue_Bool,
                _setPropertyValue_Float,
                _setPropertyValue_Double,
                _setPropertyValue_Int,
                _setPropertyValue_UInt,
                _setPropertyValue_Short,
                _setPropertyValue_UShort,
                _setPropertyValue_String,
                _setPropertyValue_Uri,
                _setPropertyValue_Color,
                _setPropertyValue_Point,
                _setPropertyValue_Rect,
                _setPropertyValue_Size,
                _setPropertyValue_Thickness,
                _setPropertyValue_CornerRadius,
                _setPropertyValue_TimeSpan,
                _setPropertyValue_Duration,
                _setPropertyValue_KeyTime,
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
                null, null, null, null, null, null,
                null, null, null,
                null, null,
                null, null,
                null, null, null, null, null,
                null, null, null,
                null, null,
                null, null,
                null,
                null, null, null, null,
                null, null, null, null, null, null,
                null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static void ClearTables()
        {
            _nativeTypes.Clear();
            _managedTypes.Clear();
            _extends.Clear();
            _extendPtrs.Clear();
            _weakExtends.Clear();
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

        public class PropertyChangedInfo
        {
            public Action<object, DependencyProperty> OnPropertyChanged { get; private set; }

            object[] _dp = new object[1];
            private object[] DP(DependencyProperty dp)
            {
                _dp[0] = dp;
                return _dp;
            }

            public PropertyChangedInfo(System.Type type, MethodInfo propChanged)
            {
#if !ENABLE_IL2CPP && !UNITY_IOS
                if (Platform.ID != PlatformID.iPhone)
                {
                    var obj = System.Linq.Expressions.Expression.Parameter(typeof(object), "obj");
                    var instance = System.Linq.Expressions.Expression.Convert(obj, type);
                    var dp = System.Linq.Expressions.Expression.Parameter(typeof(DependencyProperty), "dp");
                    var call = System.Linq.Expressions.Expression.Call(instance, propChanged,
                        new ParameterExpression[] { dp });
                    var lambda = System.Linq.Expressions.Expression.Lambda<Action<object, DependencyProperty>>(
                        call, new ParameterExpression[] { obj, dp });

                    OnPropertyChanged = lambda.Compile();
                }
                else
#endif
                {
                    OnPropertyChanged = (instance, dp) => propChanged.Invoke(instance, DP(dp));
                }
            }
        }

        public interface INativeTypeDO
        {
            PropertyChangedInfo PropertyChangedInfo { get; }
        }

        public class NativeTypeDOExtendedInfo: NativeTypeExtendedInfo, INativeTypeDO
        {
            public PropertyChangedInfo PropertyChangedInfo { get; private set; }

            public NativeTypeDOExtendedInfo(NativeTypeKind kind, System.Type type, Func<object> creator,
                MethodInfo propChanged): base(kind, type, creator)
            {
                if (propChanged != null)
                {
                    PropertyChangedInfo = new PropertyChangedInfo(type, propChanged);
                }
            }
        }

        public class NativeTypeDOPropsInfo: NativeTypePropsInfo, INativeTypeDO
        {
            public PropertyChangedInfo PropertyChangedInfo { get; private set; }

            public NativeTypeDOPropsInfo(NativeTypeKind kind, System.Type type, Func<object> creator,
                MethodInfo propChanged): base(kind, type, creator)
            {
                if (propChanged != null)
                {
                    PropertyChangedInfo = new PropertyChangedInfo(type, propChanged);
                }
            }
        }

        public class NativeTypeDOIndexerInfo: NativeTypeIndexerInfo, INativeTypeDO
        {
            public PropertyChangedInfo PropertyChangedInfo { get; private set; }

            public NativeTypeDOIndexerInfo(NativeTypeKind kind, System.Type type, Func<object> creator,
                IndexerAccessor indexer, MethodInfo propChanged): base(kind, type, creator, indexer)
            {
                if (propChanged != null)
                {
                    PropertyChangedInfo = new PropertyChangedInfo(type, propChanged);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        static Dictionary<long, NativeTypeInfo> _nativeTypes = new Dictionary<long, NativeTypeInfo>(512);
        static Dictionary<Type, IntPtr> _managedTypes = new Dictionary<Type, IntPtr>(512);

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static void AddNativeType(IntPtr nativeType, NativeTypeInfo info)
        {
            AddNativeType(nativeType, info, true);
        }

        private static void AddNativeType(IntPtr nativeType, NativeTypeInfo info, bool checkType)
        {
            // Check we are calling GetStaticType on the correct native type
            NativeTypeInfo current;
            if (_nativeTypes.TryGetValue(nativeType.ToInt64(), out current))
            {
                Log.Error(string.Format(
                    "Native type already registered for type {0}, trying to register {1}",
                    current.Type.FullName, info.Type.FullName));
            }

            _nativeTypes[nativeType.ToInt64()] = info;

            if (checkType)
            {
                IntPtr currentType;
                if (_managedTypes.TryGetValue(info.Type, out currentType))
                {
                    Log.Error(string.Format("Native type already registered for type {0}",
                        info.Type.FullName));
                }

                _managedTypes[info.Type] = nativeType;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static IntPtr TryGetNativeType(Type type)
        {
            IntPtr nativeType;
            if (_managedTypes.TryGetValue(type, out nativeType))
            {
                return nativeType;
            }
            else
            {
                return IntPtr.Zero;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static IntPtr GetNativeType(Type type)
        {
            IntPtr nativeType;
            if (!_managedTypes.TryGetValue(type, out nativeType))
            {
                throw new InvalidOperationException("Native type is not registered: " + type.Name);
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

                throw new InvalidOperationException("Native type is not registered");
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
                        return Unbox(cPtr, info);

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
                        return Unbox(cPtr, info);

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
            AddNativeType(NoesisGUI_.String_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(string)));
            AddNativeType(NoesisGUI_.Bool_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(bool)));
            AddNativeType(NoesisGUI_.Float_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(float)));
            AddNativeType(NoesisGUI_.Double_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(double)));
            AddNativeType(NoesisGUI_.Int_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(int)));
            AddNativeType(NoesisGUI_.UInt_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(uint)));
            AddNativeType(NoesisGUI_.Short_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(short)));
            AddNativeType(NoesisGUI_.UShort_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(ushort)));
            AddNativeType(NoesisGUI_.Long_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(long)));
            AddNativeType(NoesisGUI_.ULong_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(ulong)));
            AddNativeType(NoesisGUI_.Color_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.Color)));
            AddNativeType(NoesisGUI_.Point_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.Point)));
            AddNativeType(NoesisGUI_.Rect_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.Rect)));
            AddNativeType(NoesisGUI_.Size_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.Size)));
            AddNativeType(NoesisGUI_.Thickness_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.Thickness)));
            AddNativeType(NoesisGUI_.CornerRadius_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.CornerRadius)));
            AddNativeType(NoesisGUI_.GridLength_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.GridLength)));
            AddNativeType(NoesisGUI_.Duration_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.Duration)));
            AddNativeType(NoesisGUI_.KeyTime_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.KeyTime)));
            AddNativeType(NoesisGUI_.TimeSpan_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.TimeSpan)));
            AddNativeType(NoesisGUI_.VirtualizationCacheLength_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.VirtualizationCacheLength)));

            AddNativeType(NoesisGUI_.NullableBool_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<bool>)));
            AddNativeType(NoesisGUI_.NullableFloat_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<float>)));
            AddNativeType(NoesisGUI_.NullableDouble_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<double>)));
            AddNativeType(NoesisGUI_.NullableInt32_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<int>)));
            AddNativeType(NoesisGUI_.NullableUInt32_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<uint>)));
            AddNativeType(NoesisGUI_.NullableInt16_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<short>)));
            AddNativeType(NoesisGUI_.NullableUInt16_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<ushort>)));
            AddNativeType(NoesisGUI_.NullableInt64_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<long>)));
            AddNativeType(NoesisGUI_.NullableUInt64_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<ulong>)));
            AddNativeType(NoesisGUI_.NullableColor_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<Noesis.Color>)));
            AddNativeType(NoesisGUI_.NullablePoint_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<Noesis.Point>)));
            AddNativeType(NoesisGUI_.NullableRect_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<Noesis.Rect>)));
            AddNativeType(NoesisGUI_.NullableSize_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<Noesis.Size>)));
            AddNativeType(NoesisGUI_.NullableThickness_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<Noesis.Thickness>)));
            AddNativeType(NoesisGUI_.NullableCornerRadius_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<Noesis.CornerRadius>)));
            AddNativeType(NoesisGUI_.NullableDuration_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<Noesis.Duration>)));
            AddNativeType(NoesisGUI_.NullableKeyTime_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<Noesis.KeyTime>)));
            AddNativeType(NoesisGUI_.NullableTimeSpan_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(System.Nullable<System.TimeSpan>)));

            AddNativeType(NoesisGUI_.AlignmentX_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.AlignmentX)));
            AddNativeType(NoesisGUI_.AlignmentY_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.AlignmentY)));
            AddNativeType(NoesisGUI_.AutoToolTipPlacement_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.AutoToolTipPlacement)));
            AddNativeType(NoesisGUI_.BindingMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.BindingMode)));
            AddNativeType(NoesisGUI_.BitmapScalingMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.BitmapScalingMode)));
            AddNativeType(NoesisGUI_.BrushMappingMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.BrushMappingMode)));
            AddNativeType(NoesisGUI_.CharacterCasing_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.CharacterCasing)));
            AddNativeType(NoesisGUI_.ClickMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.ClickMode)));
            AddNativeType(NoesisGUI_.ColorInterpolationMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.ColorInterpolationMode)));
            AddNativeType(NoesisGUI_.Cursor_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.Cursor)));
            AddNativeType(NoesisGUI_.Dock_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.Dock)));
            AddNativeType(NoesisGUI_.ExpandDirection_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.ExpandDirection)));
            AddNativeType(NoesisGUI_.FillRule_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.FillRule)));
            AddNativeType(NoesisGUI_.FlowDirection_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.FlowDirection)));
            AddNativeType(NoesisGUI_.FontStretch_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.FontStretch)));
            AddNativeType(NoesisGUI_.FontStyle_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.FontStyle)));
            AddNativeType(NoesisGUI_.FontWeight_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.FontWeight)));
            AddNativeType(NoesisGUI_.GeometryCombineMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.GeometryCombineMode)));
            AddNativeType(NoesisGUI_.GradientSpreadMethod_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.GradientSpreadMethod)));
            AddNativeType(NoesisGUI_.HorizontalAlignment_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.HorizontalAlignment)));
            AddNativeType(NoesisGUI_.KeyboardNavigationMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.KeyboardNavigationMode)));
            AddNativeType(NoesisGUI_.LineStackingStrategy_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.LineStackingStrategy)));
            AddNativeType(NoesisGUI_.ListSortDirection_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.ListSortDirection)));
            AddNativeType(NoesisGUI_.MenuItemRole_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.MenuItemRole)));
            AddNativeType(NoesisGUI_.Orientation_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.Orientation)));
            AddNativeType(NoesisGUI_.OverflowMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.OverflowMode)));
            AddNativeType(NoesisGUI_.PenLineCap_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.PenLineCap)));
            AddNativeType(NoesisGUI_.PenLineJoin_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.PenLineJoin)));
            AddNativeType(NoesisGUI_.PlacementMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.PlacementMode)));
            AddNativeType(NoesisGUI_.PopupAnimation_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.PopupAnimation)));
            AddNativeType(NoesisGUI_.RelativeSourceMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.RelativeSourceMode)));
            AddNativeType(NoesisGUI_.SelectionMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.SelectionMode)));
            AddNativeType(NoesisGUI_.ScrollBarVisibility_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.ScrollBarVisibility)));
            AddNativeType(NoesisGUI_.Stretch_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.Stretch)));
            AddNativeType(NoesisGUI_.StretchDirection_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.StretchDirection)));
            AddNativeType(NoesisGUI_.TextDecorations_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.TextDecorations)));
            AddNativeType(NoesisGUI_.TextAlignment_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.TextAlignment)));
            AddNativeType(NoesisGUI_.TextTrimming_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.TextTrimming)));
            AddNativeType(NoesisGUI_.TextWrapping_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.TextWrapping)));
            AddNativeType(NoesisGUI_.TickBarPlacement_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.TickBarPlacement)));
            AddNativeType(NoesisGUI_.TickPlacement_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.TickPlacement)));
            AddNativeType(NoesisGUI_.TileMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.TileMode)));
            AddNativeType(NoesisGUI_.VerticalAlignment_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.VerticalAlignment)));
            AddNativeType(NoesisGUI_.Visibility_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.Visibility)));
            AddNativeType(NoesisGUI_.ClockState_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.ClockState)));
            AddNativeType(NoesisGUI_.EasingMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.EasingMode)));
            AddNativeType(NoesisGUI_.SlipBehavior_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.SlipBehavior)));
            AddNativeType(NoesisGUI_.FillBehavior_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.FillBehavior)));
            AddNativeType(NoesisGUI_.GridViewColumnHeaderRole_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.GridViewColumnHeaderRole)));
            AddNativeType(NoesisGUI_.HandoffBehavior_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.HandoffBehavior)));
            AddNativeType(NoesisGUI_.PanningMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.PanningMode)));
            AddNativeType(NoesisGUI_.UpdateSourceTrigger_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.UpdateSourceTrigger)));
            AddNativeType(NoesisGUI_.ScrollUnit_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.ScrollUnit)));
            AddNativeType(NoesisGUI_.VirtualizationMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.VirtualizationMode)));
            AddNativeType(NoesisGUI_.VirtualizationCacheLengthUnit_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.VirtualizationCacheLengthUnit)));
            AddNativeType(NoesisGUI_.PPAAMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Basic, typeof(Noesis.PPAAMode)));

            AddNativeType(NoesisGUI_.Boxed_String_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<string>)));
            AddNativeType(NoesisGUI_.Boxed_Bool_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<bool>)));
            AddNativeType(NoesisGUI_.Boxed_Float_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<float>)));
            AddNativeType(NoesisGUI_.Boxed_Double_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<double>)));
            AddNativeType(NoesisGUI_.Boxed_Int_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<int>)));
            AddNativeType(NoesisGUI_.Boxed_UInt_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<uint>)));
            AddNativeType(NoesisGUI_.Boxed_Short_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<short>)));
            AddNativeType(NoesisGUI_.Boxed_UShort_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<ushort>)));
            AddNativeType(NoesisGUI_.Boxed_Long_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<long>)));
            AddNativeType(NoesisGUI_.Boxed_ULong_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<ulong>)));
            AddNativeType(NoesisGUI_.Boxed_Color_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.Color>)));
            AddNativeType(NoesisGUI_.Boxed_Point_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.Point>)));
            AddNativeType(NoesisGUI_.Boxed_Rect_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.Rect>)));
            AddNativeType(NoesisGUI_.Boxed_Size_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.Size>)));
            AddNativeType(NoesisGUI_.Boxed_Thickness_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.Thickness>)));
            AddNativeType(NoesisGUI_.Boxed_CornerRadius_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.CornerRadius>)));
            AddNativeType(NoesisGUI_.Boxed_GridLength_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.GridLength>)));
            AddNativeType(NoesisGUI_.Boxed_Duration_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.Duration>)));
            AddNativeType(NoesisGUI_.Boxed_KeyTime_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.KeyTime>)));
            AddNativeType(NoesisGUI_.Boxed_TimeSpan_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<System.TimeSpan>)));
            AddNativeType(NoesisGUI_.Boxed_VirtualizationCacheLength_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.VirtualizationCacheLength>)));

            AddNativeType(NoesisGUI_.Boxed_AlignmentX_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.AlignmentX>)));
            AddNativeType(NoesisGUI_.Boxed_AlignmentY_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.AlignmentY>)));
            AddNativeType(NoesisGUI_.Boxed_AutoToolTipPlacement_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.AutoToolTipPlacement>)));
            AddNativeType(NoesisGUI_.Boxed_BindingMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.BindingMode>)));
            AddNativeType(NoesisGUI_.Boxed_BitmapScalingMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.BitmapScalingMode>)));
            AddNativeType(NoesisGUI_.Boxed_BrushMappingMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.BrushMappingMode>)));
            AddNativeType(NoesisGUI_.Boxed_CharacterCasing_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.CharacterCasing>)));
            AddNativeType(NoesisGUI_.Boxed_ClickMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.ClickMode>)));
            AddNativeType(NoesisGUI_.Boxed_ColorInterpolationMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.ColorInterpolationMode>)));
            AddNativeType(NoesisGUI_.Boxed_Cursor_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.Cursor>)));
            AddNativeType(NoesisGUI_.Boxed_Dock_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.Dock>)));
            AddNativeType(NoesisGUI_.Boxed_ExpandDirection_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.ExpandDirection>)));
            AddNativeType(NoesisGUI_.Boxed_FillRule_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.FillRule>)));
            AddNativeType(NoesisGUI_.Boxed_FlowDirection_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.FlowDirection>)));
            AddNativeType(NoesisGUI_.Boxed_FontStretch_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.FontStretch>)));
            AddNativeType(NoesisGUI_.Boxed_FontStyle_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.FontStyle>)));
            AddNativeType(NoesisGUI_.Boxed_FontWeight_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.FontWeight>)));
            AddNativeType(NoesisGUI_.Boxed_GeometryCombineMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.GeometryCombineMode>)));
            AddNativeType(NoesisGUI_.Boxed_GradientSpreadMethod_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.GradientSpreadMethod>)));
            AddNativeType(NoesisGUI_.Boxed_HorizontalAlignment_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.HorizontalAlignment>)));
            AddNativeType(NoesisGUI_.Boxed_KeyboardNavigationMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.KeyboardNavigationMode>)));
            AddNativeType(NoesisGUI_.Boxed_LineStackingStrategy_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.LineStackingStrategy>)));
            AddNativeType(NoesisGUI_.Boxed_ListSortDirection_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.ListSortDirection>)));
            AddNativeType(NoesisGUI_.Boxed_MenuItemRole_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.MenuItemRole>)));
            AddNativeType(NoesisGUI_.Boxed_Orientation_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.Orientation>)));
            AddNativeType(NoesisGUI_.Boxed_OverflowMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.OverflowMode>)));
            AddNativeType(NoesisGUI_.Boxed_PenLineCap_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.PenLineCap>)));
            AddNativeType(NoesisGUI_.Boxed_PenLineJoin_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.PenLineJoin>)));
            AddNativeType(NoesisGUI_.Boxed_PlacementMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.PlacementMode>)));
            AddNativeType(NoesisGUI_.Boxed_PopupAnimation_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.PopupAnimation>)));
            AddNativeType(NoesisGUI_.Boxed_RelativeSourceMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.RelativeSourceMode>)));
            AddNativeType(NoesisGUI_.Boxed_SelectionMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.SelectionMode>)));
            AddNativeType(NoesisGUI_.Boxed_ScrollBarVisibility_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.ScrollBarVisibility>)));
            AddNativeType(NoesisGUI_.Boxed_Stretch_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.Stretch>)));
            AddNativeType(NoesisGUI_.Boxed_StretchDirection_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.StretchDirection>)));
            AddNativeType(NoesisGUI_.Boxed_TextDecorations_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.TextDecorations>)));
            AddNativeType(NoesisGUI_.Boxed_TextAlignment_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.TextAlignment>)));
            AddNativeType(NoesisGUI_.Boxed_TextTrimming_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.TextTrimming>)));
            AddNativeType(NoesisGUI_.Boxed_TextWrapping_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.TextWrapping>)));
            AddNativeType(NoesisGUI_.Boxed_TickBarPlacement_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.TickBarPlacement>)));
            AddNativeType(NoesisGUI_.Boxed_TickPlacement_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.TickPlacement>)));
            AddNativeType(NoesisGUI_.Boxed_TileMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.TileMode>)));
            AddNativeType(NoesisGUI_.Boxed_VerticalAlignment_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.VerticalAlignment>)));
            AddNativeType(NoesisGUI_.Boxed_Visibility_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.Visibility>)));
            AddNativeType(NoesisGUI_.Boxed_ClockState_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.ClockState>)));
            AddNativeType(NoesisGUI_.Boxed_EasingMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.EasingMode>)));
            AddNativeType(NoesisGUI_.Boxed_SlipBehavior_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.SlipBehavior>)));
            AddNativeType(NoesisGUI_.Boxed_FillBehavior_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.FillBehavior>)));
            AddNativeType(NoesisGUI_.Boxed_GridViewColumnHeaderRole_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.GridViewColumnHeaderRole>)));
            AddNativeType(NoesisGUI_.Boxed_HandoffBehavior_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.HandoffBehavior>)));
            AddNativeType(NoesisGUI_.Boxed_PanningMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.PanningMode>)));
            AddNativeType(NoesisGUI_.Boxed_UpdateSourceTrigger_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.UpdateSourceTrigger>)));
            AddNativeType(NoesisGUI_.Boxed_ScrollUnit_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.ScrollUnit>)));
            AddNativeType(NoesisGUI_.Boxed_VirtualizationMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.VirtualizationMode>)));
            AddNativeType(NoesisGUI_.Boxed_VirtualizationCacheLengthUnit_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.VirtualizationCacheLengthUnit>)));
            AddNativeType(NoesisGUI_.Boxed_PPAAMode_GetStaticType(), new NativeTypeInfo(NativeTypeKind.Boxed, typeof(Boxed<Noesis.PPAAMode>)));

            AddNativeType(Noesis.BaseComponent.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.BaseComponent), Noesis.BaseComponent.CreateProxy));
            AddNativeType(Noesis.DispatcherObject.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DispatcherObject), Noesis.DispatcherObject.CreateProxy));
            AddNativeType(Noesis.DependencyObject.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DependencyObject), Noesis.DependencyObject.CreateProxy));
            AddNativeType(Noesis.DependencyProperty.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DependencyProperty), Noesis.DependencyProperty.CreateProxy));
            AddNativeType(Noesis.Freezable.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Freezable), Noesis.Freezable.CreateProxy));
            AddNativeType(Noesis.PropertyMetadata.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.PropertyMetadata), Noesis.PropertyMetadata.CreateProxy));
            AddNativeType(Noesis.Expression.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Expression), Noesis.Expression.CreateProxy));

            AddNativeType(Noesis.View.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.View), Noesis.View.CreateProxy));
            AddNativeType(Noesis.Renderer.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Renderer), Noesis.Renderer.CreateProxy));

            AddNativeType(Noesis.AdornerDecorator.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.AdornerDecorator), Noesis.AdornerDecorator.CreateProxy));
            AddNativeType(Noesis.Animatable.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Animatable), Noesis.Animatable.CreateProxy));
            AddNativeType(Noesis.BindingBase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.BindingBase), Noesis.BindingBase.CreateProxy));
            AddNativeType(Noesis.BindingExpressionBase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.BindingExpressionBase), Noesis.BindingExpressionBase.CreateProxy));
            AddNativeType(Noesis.BindingExpression.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.BindingExpression), Noesis.BindingExpression.CreateProxy));
            AddNativeType(Noesis.Bold.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Bold), Noesis.Bold.CreateProxy));
            AddNativeType(Noesis.ButtonBase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ButtonBase), Noesis.ButtonBase.CreateProxy));
            AddNativeType(Noesis.DefinitionBase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DefinitionBase), Noesis.DefinitionBase.CreateProxy));
            AddNativeType(Noesis.MenuBase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.MenuBase), Noesis.MenuBase.CreateProxy));
            AddNativeType(Noesis.SetterBase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SetterBase), Noesis.SetterBase.CreateProxy));
            AddNativeType(Noesis.SetterBaseCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SetterBaseCollection), Noesis.SetterBaseCollection.CreateProxy));
            AddNativeType(Noesis.TextBoxBase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TextBoxBase), Noesis.TextBoxBase.CreateProxy));
            AddNativeType(Noesis.TriggerBase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TriggerBase), Noesis.TriggerBase.CreateProxy));
            AddNativeType(Noesis.Binding.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Binding), Noesis.Binding.CreateProxy));
            AddNativeType(Noesis.BitmapImage.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.BitmapImage), Noesis.BitmapImage.CreateProxy));
            AddNativeType(Noesis.BitmapSource.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.BitmapSource), Noesis.BitmapSource.CreateProxy));
            AddNativeType(Noesis.Border.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Border), Noesis.Border.CreateProxy));
            AddNativeType(Noesis.Brush.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Brush), Noesis.Brush.CreateProxy));
            AddNativeType(Noesis.BulletDecorator.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.BulletDecorator), Noesis.BulletDecorator.CreateProxy));
            AddNativeType(Noesis.Button.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Button), Noesis.Button.CreateProxy));
            AddNativeType(Noesis.Canvas.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Canvas), Noesis.Canvas.CreateProxy));
            AddNativeType(Noesis.CheckBox.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.CheckBox), Noesis.CheckBox.CreateProxy));
            AddNativeType(Noesis.BaseUICollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.BaseUICollection), Noesis.BaseUICollection.CreateProxy));
            AddNativeType(Noesis.CollectionView.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.CollectionView), Noesis.CollectionView.CreateProxy));
            AddNativeType(Noesis.CollectionViewSource.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.CollectionViewSource), Noesis.CollectionViewSource.CreateProxy));
            AddNativeType(Noesis.ColumnDefinition.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ColumnDefinition), Noesis.ColumnDefinition.CreateProxy));
            AddNativeType(Noesis.ColumnDefinitionCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ColumnDefinitionCollection), Noesis.ColumnDefinitionCollection.CreateProxy));
            AddNativeType(Noesis.CombinedGeometry.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.CombinedGeometry), Noesis.CombinedGeometry.CreateProxy));
            AddNativeType(Noesis.ComboBox.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ComboBox), Noesis.ComboBox.CreateProxy));
            AddNativeType(Noesis.ComboBoxItem.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ComboBoxItem), Noesis.ComboBoxItem.CreateProxy));
            AddNativeType(Noesis.CommandBinding.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.CommandBinding), Noesis.CommandBinding.CreateProxy));
            AddNativeType(Noesis.CommandBindingCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.CommandBindingCollection), Noesis.CommandBindingCollection.CreateProxy));
            AddNativeType(Noesis.CompositeTransform.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.CompositeTransform), Noesis.CompositeTransform.CreateProxy));
            AddNativeType(Noesis.Condition.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Condition), Noesis.Condition.CreateProxy));
            AddNativeType(Noesis.ConditionCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ConditionCollection), Noesis.ConditionCollection.CreateProxy));
            AddNativeType(Noesis.ContentControl.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ContentControl), Noesis.ContentControl.CreateProxy));
            AddNativeType(Noesis.ContentPresenter.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ContentPresenter), Noesis.ContentPresenter.CreateProxy));
            AddNativeType(Noesis.ContextMenu.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ContextMenu), Noesis.ContextMenu.CreateProxy));
            AddNativeType(Noesis.Control.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Control), Noesis.Control.CreateProxy));
            AddNativeType(Noesis.ControlTemplate.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ControlTemplate), Noesis.ControlTemplate.CreateProxy));
            AddNativeType(Noesis.DashStyle.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DashStyle), Noesis.DashStyle.CreateProxy));
            AddNativeType(Noesis.DataTemplate.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DataTemplate), Noesis.DataTemplate.CreateProxy));
            AddNativeType(Noesis.DataTemplateSelector.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DataTemplateSelector), Noesis.DataTemplateSelector.CreateProxy));
            AddNativeType(Noesis.HierarchicalDataTemplate.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.HierarchicalDataTemplate), Noesis.HierarchicalDataTemplate.CreateProxy));
            AddNativeType(Noesis.Decorator.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Decorator), Noesis.Decorator.CreateProxy));
            AddNativeType(Noesis.DockPanel.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DockPanel), Noesis.DockPanel.CreateProxy));
            AddNativeType(Noesis.Ellipse.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Ellipse), Noesis.Ellipse.CreateProxy));
            AddNativeType(Noesis.EllipseGeometry.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.EllipseGeometry), Noesis.EllipseGeometry.CreateProxy));
            AddNativeType(Noesis.EventTrigger.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.EventTrigger), Noesis.EventTrigger.CreateProxy));
            AddNativeType(Noesis.Expander.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Expander), Noesis.Expander.CreateProxy));
            AddNativeType(Noesis.FontFamily.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.FontFamily), Noesis.FontFamily.CreateProxy));
            AddNativeType(Noesis.FormattedText.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.FormattedText), Noesis.FormattedText.CreateProxy));
            AddNativeType(Noesis.FrameworkElement.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.FrameworkElement), Noesis.FrameworkElement.CreateProxy));
            AddNativeType(Noesis.FrameworkTemplate.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.FrameworkTemplate), Noesis.FrameworkTemplate.CreateProxy));
            AddNativeType(Noesis.FrameworkPropertyMetadata.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.FrameworkPropertyMetadata), Noesis.FrameworkPropertyMetadata.CreateProxy));
            AddNativeType(Noesis.BaseFreezableCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.BaseFreezableCollection), Noesis.BaseFreezableCollection.CreateProxy));
            AddNativeType(Noesis.Geometry.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Geometry), Noesis.Geometry.CreateProxy));
            AddNativeType(Noesis.GeometryCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.GeometryCollection), Noesis.GeometryCollection.CreateProxy));
            AddNativeType(Noesis.GeometryGroup.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.GeometryGroup), Noesis.GeometryGroup.CreateProxy));
            AddNativeType(Noesis.GradientBrush.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.GradientBrush), Noesis.GradientBrush.CreateProxy));
            AddNativeType(Noesis.GradientStop.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.GradientStop), Noesis.GradientStop.CreateProxy));
            AddNativeType(Noesis.GradientStopCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.GradientStopCollection), Noesis.GradientStopCollection.CreateProxy));
            AddNativeType(Noesis.Grid.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Grid), Noesis.Grid.CreateProxy));
            AddNativeType(Noesis.GroupBox.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.GroupBox), Noesis.GroupBox.CreateProxy));
            AddNativeType(Noesis.HeaderedContentControl.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.HeaderedContentControl), Noesis.HeaderedContentControl.CreateProxy));
            AddNativeType(Noesis.HeaderedItemsControl.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.HeaderedItemsControl), Noesis.HeaderedItemsControl.CreateProxy));
            AddNativeType(Noesis.Hyperlink.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Hyperlink), Noesis.Hyperlink.CreateProxy));
            AddNativeType(Noesis.Image.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Image), Noesis.Image.CreateProxy));
            AddNativeType(Noesis.ImageBrush.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ImageBrush), Noesis.ImageBrush.CreateProxy));
            AddNativeType(Noesis.ImageSource.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ImageSource), Noesis.ImageSource.CreateProxy));
            AddNativeType(Noesis.Inline.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Inline), Noesis.Inline.CreateProxy));
            AddNativeType(Noesis.InlineCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.InlineCollection), Noesis.InlineCollection.CreateProxy));
            AddNativeType(Noesis.InlineUIContainer.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.InlineUIContainer), Noesis.InlineUIContainer.CreateProxy));
            AddNativeType(Noesis.InputBinding.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.InputBinding), Noesis.InputBinding.CreateProxy));
            AddNativeType(Noesis.InputBindingCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.InputBindingCollection), Noesis.InputBindingCollection.CreateProxy));
            AddNativeType(Noesis.InputGesture.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.InputGesture), Noesis.InputGesture.CreateProxy));
            AddNativeType(Noesis.InputGestureCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.InputGestureCollection), Noesis.InputGestureCollection.CreateProxy));
            AddNativeType(Noesis.Italic.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Italic), Noesis.Italic.CreateProxy));
            AddNativeType(Noesis.ItemCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ItemCollection), Noesis.ItemCollection.CreateProxy));
            AddNativeType(Noesis.ItemContainerGenerator.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ItemContainerGenerator), Noesis.ItemContainerGenerator.CreateProxy));
            AddNativeType(Noesis.ItemsControl.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ItemsControl), Noesis.ItemsControl.CreateProxy));
            AddNativeType(Noesis.ItemsPanelTemplate.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ItemsPanelTemplate), Noesis.ItemsPanelTemplate.CreateProxy));
            AddNativeType(Noesis.ItemsPresenter.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ItemsPresenter), Noesis.ItemsPresenter.CreateProxy));
            AddNativeType(Noesis.KeyBinding.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.KeyBinding), Noesis.KeyBinding.CreateProxy));
            AddNativeType(Noesis.Keyboard.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Keyboard), Noesis.Keyboard.CreateProxy));
            AddNativeType(Noesis.KeyboardNavigation.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.KeyboardNavigation), Noesis.KeyboardNavigation.CreateProxy));
            AddNativeType(Noesis.KeyGesture.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.KeyGesture), Noesis.KeyGesture.CreateProxy));
            AddNativeType(Noesis.Label.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Label), Noesis.Label.CreateProxy));
            AddNativeType(Noesis.Line.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Line), Noesis.Line.CreateProxy));
            AddNativeType(Noesis.LinearGradientBrush.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.LinearGradientBrush), Noesis.LinearGradientBrush.CreateProxy));
            AddNativeType(Noesis.LineBreak.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.LineBreak), Noesis.LineBreak.CreateProxy));
            AddNativeType(Noesis.LineGeometry.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.LineGeometry), Noesis.LineGeometry.CreateProxy));
            AddNativeType(Noesis.ListBox.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ListBox), Noesis.ListBox.CreateProxy));
            AddNativeType(Noesis.ListBoxItem.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ListBoxItem), Noesis.ListBoxItem.CreateProxy));
            AddNativeType(Noesis.MarkupExtension.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.MarkupExtension), Noesis.MarkupExtension.CreateProxy));
            AddNativeType(Noesis.Matrix3DProjection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Matrix3DProjection), Noesis.Matrix3DProjection.CreateProxy));
            AddNativeType(Noesis.MatrixTransform.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.MatrixTransform), Noesis.MatrixTransform.CreateProxy));
            AddNativeType(Noesis.Menu.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Menu), Noesis.Menu.CreateProxy));
            AddNativeType(Noesis.MenuItem.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.MenuItem), Noesis.MenuItem.CreateProxy));
            AddNativeType(Noesis.MultiTrigger.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.MultiTrigger), Noesis.MultiTrigger.CreateProxy));
            AddNativeType(Noesis.NameScope.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.NameScope), Noesis.NameScope.CreateProxy));
            AddNativeType(Noesis.Page.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Page), Noesis.Page.CreateProxy));
            AddNativeType(Noesis.Panel.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Panel), Noesis.Panel.CreateProxy));
            AddNativeType(Noesis.PasswordBox.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.PasswordBox), Noesis.PasswordBox.CreateProxy));
            AddNativeType(Noesis.Path.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Path), Noesis.Path.CreateProxy));
            AddNativeType(Noesis.Pen.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Pen), Noesis.Pen.CreateProxy));
            AddNativeType(Noesis.PlaneProjection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.PlaneProjection), Noesis.PlaneProjection.CreateProxy));
            AddNativeType(Noesis.Popup.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Popup), Noesis.Popup.CreateProxy));
            AddNativeType(Noesis.ProgressBar.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ProgressBar), Noesis.ProgressBar.CreateProxy));
            AddNativeType(Noesis.Projection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Projection), Noesis.Projection.CreateProxy));
            AddNativeType(Noesis.PropertyPath.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.PropertyPath), Noesis.PropertyPath.CreateProxy));
            AddNativeType(Noesis.RadialGradientBrush.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RadialGradientBrush), Noesis.RadialGradientBrush.CreateProxy));
            AddNativeType(Noesis.RadioButton.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RadioButton), Noesis.RadioButton.CreateProxy));
            AddNativeType(Noesis.RangeBase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RangeBase), Noesis.RangeBase.CreateProxy));
            AddNativeType(Noesis.Rectangle.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Rectangle), Noesis.Rectangle.CreateProxy));
            AddNativeType(Noesis.RectangleGeometry.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RectangleGeometry), Noesis.RectangleGeometry.CreateProxy));
            AddNativeType(Noesis.RelativeSource.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RelativeSource), Noesis.RelativeSource.CreateProxy));
            AddNativeType(Noesis.RepeatButton.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RepeatButton), Noesis.RepeatButton.CreateProxy));
            AddNativeType(Noesis.ResourceDictionary.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ResourceDictionary), Noesis.ResourceDictionary.CreateProxy));
            AddNativeType(Noesis.ResourceDictionaryCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ResourceDictionaryCollection), Noesis.ResourceDictionaryCollection.CreateProxy));
            AddNativeType(Noesis.ResourceKeyType.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ResourceKeyType), Noesis.ResourceKeyType.CreateProxy));
            AddNativeType(Noesis.RotateTransform.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RotateTransform), Noesis.RotateTransform.CreateProxy));
            AddNativeType(Noesis.RoutedCommand.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RoutedCommand), Noesis.RoutedCommand.CreateProxy));
            AddNativeType(Noesis.RoutedEvent.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RoutedEvent), Noesis.RoutedEvent.CreateProxy));
            AddNativeType(Noesis.RoutedUICommand.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RoutedUICommand), Noesis.RoutedUICommand.CreateProxy));
            AddNativeType(Noesis.RowDefinition.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RowDefinition), Noesis.RowDefinition.CreateProxy));
            AddNativeType(Noesis.RowDefinitionCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RowDefinitionCollection), Noesis.RowDefinitionCollection.CreateProxy));
            AddNativeType(Noesis.Run.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Run), Noesis.Run.CreateProxy));
            AddNativeType(Noesis.ScaleTransform.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ScaleTransform), Noesis.ScaleTransform.CreateProxy));
            AddNativeType(Noesis.ScrollBar.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ScrollBar), Noesis.ScrollBar.CreateProxy));
            AddNativeType(Noesis.ScrollViewer.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ScrollViewer), Noesis.ScrollViewer.CreateProxy));
            AddNativeType(Noesis.ScrollContentPresenter.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ScrollContentPresenter), Noesis.ScrollContentPresenter.CreateProxy));
            AddNativeType(Noesis.Selector.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Selector), Noesis.Selector.CreateProxy));
            AddNativeType(Noesis.Separator.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Separator), Noesis.Separator.CreateProxy));
            AddNativeType(Noesis.Setter.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Setter), Noesis.Setter.CreateProxy));
            AddNativeType(Noesis.Shape.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Shape), Noesis.Shape.CreateProxy));
            AddNativeType(Noesis.SkewTransform.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SkewTransform), Noesis.SkewTransform.CreateProxy));
            AddNativeType(Noesis.Slider.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Slider), Noesis.Slider.CreateProxy));
            AddNativeType(Noesis.SolidColorBrush.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SolidColorBrush), Noesis.SolidColorBrush.CreateProxy));
            AddNativeType(Noesis.Span.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Span), Noesis.Span.CreateProxy));
            AddNativeType(Noesis.StackPanel.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.StackPanel), Noesis.StackPanel.CreateProxy));
            AddNativeType(Noesis.StatusBar.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.StatusBar), Noesis.StatusBar.CreateProxy));
            AddNativeType(Noesis.StatusBarItem.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.StatusBarItem), Noesis.StatusBarItem.CreateProxy));
            AddNativeType(Noesis.StreamGeometry.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.StreamGeometry), Noesis.StreamGeometry.CreateProxy));
            AddNativeType(Noesis.Style.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Style), Noesis.Style.CreateProxy));
            AddNativeType(Noesis.TabControl.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TabControl), Noesis.TabControl.CreateProxy));
            AddNativeType(Noesis.TabItem.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TabItem), Noesis.TabItem.CreateProxy));
            AddNativeType(Noesis.TabPanel.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TabPanel), Noesis.TabPanel.CreateProxy));
            AddNativeType(Noesis.TemplateBindingExpression.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TemplateBindingExpression), Noesis.TemplateBindingExpression.CreateProxy));
            AddNativeType(Noesis.TemplateBindingExtension.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TemplateBindingExtension), Noesis.TemplateBindingExtension.CreateProxy));
            AddNativeType(Noesis.TextBlock.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TextBlock), Noesis.TextBlock.CreateProxy));
            AddNativeType(NoesisGUI_.TextBoxView_GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TextBlock), Noesis.TextBlock.CreateProxy), false);
            AddNativeType(Noesis.TextBox.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TextBox), Noesis.TextBox.CreateProxy));
            AddNativeType(Noesis.TextElement.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TextElement), Noesis.TextElement.CreateProxy));
            AddNativeType(Noesis.TextureSource.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TextureSource), Noesis.TextureSource.CreateProxy));
            AddNativeType(Noesis.Thumb.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Thumb), Noesis.Thumb.CreateProxy));
            AddNativeType(Noesis.TickBar.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TickBar), Noesis.TickBar.CreateProxy));
            AddNativeType(Noesis.TileBrush.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TileBrush), Noesis.TileBrush.CreateProxy));
            AddNativeType(Noesis.ToggleButton.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ToggleButton), Noesis.ToggleButton.CreateProxy));
            AddNativeType(Noesis.ToolBar.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ToolBar), Noesis.ToolBar.CreateProxy));
            AddNativeType(Noesis.ToolBarOverflowPanel.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ToolBarOverflowPanel), Noesis.ToolBarOverflowPanel.CreateProxy));
            AddNativeType(Noesis.ToolBarPanel.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ToolBarPanel), Noesis.ToolBarPanel.CreateProxy));
            AddNativeType(Noesis.ToolBarTray.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ToolBarTray), Noesis.ToolBarTray.CreateProxy));
            AddNativeType(Noesis.ToolTip.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ToolTip), Noesis.ToolTip.CreateProxy));
            AddNativeType(Noesis.Track.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Track), Noesis.Track.CreateProxy));
            AddNativeType(Noesis.TransformGroup.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TransformGroup), Noesis.TransformGroup.CreateProxy));
            AddNativeType(Noesis.TranslateTransform.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TranslateTransform), Noesis.TranslateTransform.CreateProxy));
            AddNativeType(Noesis.Transform.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Transform), Noesis.Transform.CreateProxy));
            AddNativeType(Noesis.TransformCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TransformCollection), Noesis.TransformCollection.CreateProxy));
            AddNativeType(Noesis.TreeView.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TreeView), Noesis.TreeView.CreateProxy));
            AddNativeType(Noesis.TreeViewItem.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TreeViewItem), Noesis.TreeViewItem.CreateProxy));
            AddNativeType(Noesis.TriggerAction.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TriggerAction), Noesis.TriggerAction.CreateProxy));
            AddNativeType(Noesis.Trigger.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Trigger), Noesis.Trigger.CreateProxy));
            AddNativeType(Noesis.TriggerCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TriggerCollection), Noesis.TriggerCollection.CreateProxy));
            AddNativeType(Noesis.UIElement.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.UIElement), Noesis.UIElement.CreateProxy));
            AddNativeType(Noesis.UIElementCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.UIElementCollection), Noesis.UIElementCollection.CreateProxy));
            AddNativeType(Noesis.UIPropertyMetadata.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.UIPropertyMetadata), Noesis.UIPropertyMetadata.CreateProxy));
            AddNativeType(Noesis.Underline.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Underline), Noesis.Underline.CreateProxy));
            AddNativeType(Noesis.UniformGrid.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.UniformGrid), Noesis.UniformGrid.CreateProxy));
            AddNativeType(Noesis.UserControl.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.UserControl), Noesis.UserControl.CreateProxy));
            AddNativeType(Noesis.Viewbox.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Viewbox), Noesis.Viewbox.CreateProxy));
            AddNativeType(Noesis.VirtualizingPanel.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.VirtualizingPanel), Noesis.VirtualizingPanel.CreateProxy));
            AddNativeType(Noesis.VirtualizingStackPanel.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.VirtualizingStackPanel), Noesis.VirtualizingStackPanel.CreateProxy));
            AddNativeType(Noesis.Visual.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Visual), Noesis.Visual.CreateProxy));
            AddNativeType(Noesis.VisualBrush.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.VisualBrush), Noesis.VisualBrush.CreateProxy));
            AddNativeType(Noesis.VisualCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.VisualCollection), Noesis.VisualCollection.CreateProxy));
            AddNativeType(Noesis.WrapPanel.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.WrapPanel), Noesis.WrapPanel.CreateProxy));
            AddNativeType(Noesis.Texture.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Texture), Noesis.Texture.CreateProxy));
            AddNativeType(NoesisGUI_.RootVisual_GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Visual), Noesis.Visual.CreateProxy), false);


            AddNativeType(Noesis.Clock.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Clock), Noesis.Clock.CreateProxy));
            AddNativeType(Noesis.ClockGroup.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ClockGroup), Noesis.ClockGroup.CreateProxy));
            AddNativeType(Noesis.AnimationClock.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.AnimationClock), Noesis.AnimationClock.CreateProxy));

            AddNativeType(Noesis.Timeline.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Timeline), Noesis.Timeline.CreateProxy));
            AddNativeType(Noesis.TimelineCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TimelineCollection), Noesis.TimelineCollection.CreateProxy));
            AddNativeType(Noesis.TimelineGroup.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.TimelineGroup), Noesis.TimelineGroup.CreateProxy));
            AddNativeType(Noesis.ParallelTimeline.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ParallelTimeline), Noesis.ParallelTimeline.CreateProxy));
            AddNativeType(Noesis.AnimationTimeline.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.AnimationTimeline), Noesis.AnimationTimeline.CreateProxy));
            AddNativeType(Noesis.Storyboard.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Storyboard), Noesis.Storyboard.CreateProxy));

            AddNativeType(Noesis.Int16Animation.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Int16Animation), Noesis.Int16Animation.CreateProxy));
            AddNativeType(Noesis.Int32Animation.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Int32Animation), Noesis.Int32Animation.CreateProxy));
            AddNativeType(Noesis.DoubleAnimation.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DoubleAnimation), Noesis.DoubleAnimation.CreateProxy));
            AddNativeType(Noesis.ColorAnimation.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ColorAnimation), Noesis.ColorAnimation.CreateProxy));
            AddNativeType(Noesis.PointAnimation.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.PointAnimation), Noesis.PointAnimation.CreateProxy));
            AddNativeType(Noesis.RectAnimation.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RectAnimation), Noesis.RectAnimation.CreateProxy));
            AddNativeType(Noesis.SizeAnimation.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SizeAnimation), Noesis.SizeAnimation.CreateProxy));
            AddNativeType(Noesis.ThicknessAnimation.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ThicknessAnimation), Noesis.ThicknessAnimation.CreateProxy));

            AddNativeType(Noesis.BooleanAnimationUsingKeyFrames.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.BooleanAnimationUsingKeyFrames), Noesis.BooleanAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(Noesis.Int16AnimationUsingKeyFrames.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Int16AnimationUsingKeyFrames), Noesis.Int16AnimationUsingKeyFrames.CreateProxy));
            AddNativeType(Noesis.Int32AnimationUsingKeyFrames.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Int32AnimationUsingKeyFrames), Noesis.Int32AnimationUsingKeyFrames.CreateProxy));
            AddNativeType(Noesis.DoubleAnimationUsingKeyFrames.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DoubleAnimationUsingKeyFrames), Noesis.DoubleAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(Noesis.ColorAnimationUsingKeyFrames.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ColorAnimationUsingKeyFrames), Noesis.ColorAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(Noesis.PointAnimationUsingKeyFrames.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.PointAnimationUsingKeyFrames), Noesis.PointAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(Noesis.RectAnimationUsingKeyFrames.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RectAnimationUsingKeyFrames), Noesis.RectAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(Noesis.SizeAnimationUsingKeyFrames.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SizeAnimationUsingKeyFrames), Noesis.SizeAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(Noesis.ThicknessAnimationUsingKeyFrames.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ThicknessAnimationUsingKeyFrames), Noesis.ThicknessAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(Noesis.ObjectAnimationUsingKeyFrames.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ObjectAnimationUsingKeyFrames), Noesis.ObjectAnimationUsingKeyFrames.CreateProxy));
            AddNativeType(Noesis.StringAnimationUsingKeyFrames.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.StringAnimationUsingKeyFrames), Noesis.StringAnimationUsingKeyFrames.CreateProxy));

            AddNativeType(Noesis.BooleanKeyFrameCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.BooleanKeyFrameCollection), Noesis.BooleanKeyFrameCollection.CreateProxy));
            AddNativeType(Noesis.Int16KeyFrameCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Int16KeyFrameCollection), Noesis.Int16KeyFrameCollection.CreateProxy));
            AddNativeType(Noesis.Int32KeyFrameCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Int32KeyFrameCollection), Noesis.Int32KeyFrameCollection.CreateProxy));
            AddNativeType(Noesis.DoubleKeyFrameCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DoubleKeyFrameCollection), Noesis.DoubleKeyFrameCollection.CreateProxy));
            AddNativeType(Noesis.ColorKeyFrameCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ColorKeyFrameCollection), Noesis.ColorKeyFrameCollection.CreateProxy));
            AddNativeType(Noesis.PointKeyFrameCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.PointKeyFrameCollection), Noesis.PointKeyFrameCollection.CreateProxy));
            AddNativeType(Noesis.RectKeyFrameCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RectKeyFrameCollection), Noesis.RectKeyFrameCollection.CreateProxy));
            AddNativeType(Noesis.SizeKeyFrameCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SizeKeyFrameCollection), Noesis.SizeKeyFrameCollection.CreateProxy));
            AddNativeType(Noesis.ThicknessKeyFrameCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ThicknessKeyFrameCollection), Noesis.ThicknessKeyFrameCollection.CreateProxy));
            AddNativeType(Noesis.ObjectKeyFrameCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ObjectKeyFrameCollection), Noesis.ObjectKeyFrameCollection.CreateProxy));
            AddNativeType(Noesis.StringKeyFrameCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.StringKeyFrameCollection), Noesis.StringKeyFrameCollection.CreateProxy));

            AddNativeType(Noesis.BooleanKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.BooleanKeyFrame), Noesis.BooleanKeyFrame.CreateProxy));
            AddNativeType(Noesis.DoubleKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DoubleKeyFrame), Noesis.DoubleKeyFrame.CreateProxy));
            AddNativeType(Noesis.Int16KeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Int16KeyFrame), Noesis.Int16KeyFrame.CreateProxy));
            AddNativeType(Noesis.Int32KeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.Int32KeyFrame), Noesis.Int32KeyFrame.CreateProxy));
            AddNativeType(Noesis.ColorKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ColorKeyFrame), Noesis.ColorKeyFrame.CreateProxy));
            AddNativeType(Noesis.PointKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.PointKeyFrame), Noesis.PointKeyFrame.CreateProxy));
            AddNativeType(Noesis.RectKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.RectKeyFrame), Noesis.RectKeyFrame.CreateProxy));
            AddNativeType(Noesis.SizeKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SizeKeyFrame), Noesis.SizeKeyFrame.CreateProxy));
            AddNativeType(Noesis.ThicknessKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ThicknessKeyFrame), Noesis.ThicknessKeyFrame.CreateProxy));
            AddNativeType(Noesis.StringKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.StringKeyFrame), Noesis.StringKeyFrame.CreateProxy));

            AddNativeType(Noesis.DiscreteBooleanKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DiscreteBooleanKeyFrame), Noesis.DiscreteBooleanKeyFrame.CreateProxy));
            AddNativeType(Noesis.DiscreteInt16KeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DiscreteInt16KeyFrame), Noesis.DiscreteInt16KeyFrame.CreateProxy));
            AddNativeType(Noesis.DiscreteInt32KeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DiscreteInt32KeyFrame), Noesis.DiscreteInt32KeyFrame.CreateProxy));
            AddNativeType(Noesis.DiscreteDoubleKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DiscreteDoubleKeyFrame), Noesis.DiscreteDoubleKeyFrame.CreateProxy));
            AddNativeType(Noesis.DiscreteColorKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DiscreteColorKeyFrame), Noesis.DiscreteColorKeyFrame.CreateProxy));
            AddNativeType(Noesis.DiscretePointKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DiscretePointKeyFrame), Noesis.DiscretePointKeyFrame.CreateProxy));
            AddNativeType(Noesis.DiscreteRectKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DiscreteRectKeyFrame), Noesis.DiscreteRectKeyFrame.CreateProxy));
            AddNativeType(Noesis.DiscreteSizeKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DiscreteSizeKeyFrame), Noesis.DiscreteSizeKeyFrame.CreateProxy));
            AddNativeType(Noesis.DiscreteThicknessKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DiscreteThicknessKeyFrame), Noesis.DiscreteThicknessKeyFrame.CreateProxy));
            AddNativeType(Noesis.DiscreteObjectKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DiscreteObjectKeyFrame), Noesis.DiscreteObjectKeyFrame.CreateProxy));
            AddNativeType(Noesis.DiscreteStringKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.DiscreteStringKeyFrame), Noesis.DiscreteStringKeyFrame.CreateProxy));

            AddNativeType(Noesis.LinearInt16KeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.LinearInt16KeyFrame), Noesis.LinearInt16KeyFrame.CreateProxy));
            AddNativeType(Noesis.LinearInt32KeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.LinearInt32KeyFrame), Noesis.LinearInt32KeyFrame.CreateProxy));
            AddNativeType(Noesis.LinearDoubleKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.LinearDoubleKeyFrame), Noesis.LinearDoubleKeyFrame.CreateProxy));
            AddNativeType(Noesis.LinearColorKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.LinearColorKeyFrame), Noesis.LinearColorKeyFrame.CreateProxy));
            AddNativeType(Noesis.LinearPointKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.LinearPointKeyFrame), Noesis.LinearPointKeyFrame.CreateProxy));
            AddNativeType(Noesis.LinearRectKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.LinearRectKeyFrame), Noesis.LinearRectKeyFrame.CreateProxy));
            AddNativeType(Noesis.LinearSizeKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.LinearSizeKeyFrame), Noesis.LinearSizeKeyFrame.CreateProxy));
            AddNativeType(Noesis.LinearThicknessKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.LinearThicknessKeyFrame), Noesis.LinearThicknessKeyFrame.CreateProxy));

            AddNativeType(Noesis.SplineInt16KeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SplineInt16KeyFrame), Noesis.SplineInt16KeyFrame.CreateProxy));
            AddNativeType(Noesis.SplineInt32KeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SplineInt32KeyFrame), Noesis.SplineInt32KeyFrame.CreateProxy));
            AddNativeType(Noesis.SplineDoubleKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SplineDoubleKeyFrame), Noesis.SplineDoubleKeyFrame.CreateProxy));
            AddNativeType(Noesis.SplineColorKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SplineColorKeyFrame), Noesis.SplineColorKeyFrame.CreateProxy));
            AddNativeType(Noesis.SplinePointKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SplinePointKeyFrame), Noesis.SplinePointKeyFrame.CreateProxy));
            AddNativeType(Noesis.SplineRectKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SplineRectKeyFrame), Noesis.SplineRectKeyFrame.CreateProxy));
            AddNativeType(Noesis.SplineSizeKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SplineSizeKeyFrame), Noesis.SplineSizeKeyFrame.CreateProxy));
            AddNativeType(Noesis.SplineThicknessKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SplineThicknessKeyFrame), Noesis.SplineThicknessKeyFrame.CreateProxy));

            AddNativeType(Noesis.KeySpline.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.KeySpline), Noesis.KeySpline.CreateProxy));

            AddNativeType(Noesis.EasingInt16KeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.EasingInt16KeyFrame), Noesis.EasingInt16KeyFrame.CreateProxy));
            AddNativeType(Noesis.EasingInt32KeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.EasingInt32KeyFrame), Noesis.EasingInt32KeyFrame.CreateProxy));
            AddNativeType(Noesis.EasingDoubleKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.EasingDoubleKeyFrame), Noesis.EasingDoubleKeyFrame.CreateProxy));
            AddNativeType(Noesis.EasingColorKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.EasingColorKeyFrame), Noesis.EasingColorKeyFrame.CreateProxy));
            AddNativeType(Noesis.EasingPointKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.EasingPointKeyFrame), Noesis.EasingPointKeyFrame.CreateProxy));
            AddNativeType(Noesis.EasingRectKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.EasingRectKeyFrame), Noesis.EasingRectKeyFrame.CreateProxy));
            AddNativeType(Noesis.EasingSizeKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.EasingSizeKeyFrame), Noesis.EasingSizeKeyFrame.CreateProxy));
            AddNativeType(Noesis.EasingThicknessKeyFrame.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.EasingThicknessKeyFrame), Noesis.EasingThicknessKeyFrame.CreateProxy));

            AddNativeType(Noesis.EasingFunctionBase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.EasingFunctionBase), Noesis.EasingFunctionBase.CreateProxy));
            AddNativeType(Noesis.BackEase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.BackEase), Noesis.BackEase.CreateProxy));
            AddNativeType(Noesis.BounceEase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.BounceEase), Noesis.BounceEase.CreateProxy));
            AddNativeType(Noesis.CircleEase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.CircleEase), Noesis.CircleEase.CreateProxy));
            AddNativeType(Noesis.CubicEase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.CubicEase), Noesis.CubicEase.CreateProxy));
            AddNativeType(Noesis.ElasticEase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ElasticEase), Noesis.ElasticEase.CreateProxy));
            AddNativeType(Noesis.ExponentialEase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ExponentialEase), Noesis.ExponentialEase.CreateProxy));
            AddNativeType(Noesis.PowerEase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.PowerEase), Noesis.PowerEase.CreateProxy));
            AddNativeType(Noesis.QuadraticEase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.QuadraticEase), Noesis.QuadraticEase.CreateProxy));
            AddNativeType(Noesis.QuarticEase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.QuarticEase), Noesis.QuarticEase.CreateProxy));
            AddNativeType(Noesis.QuinticEase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.QuinticEase), Noesis.QuinticEase.CreateProxy));
            AddNativeType(Noesis.SineEase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.SineEase), Noesis.SineEase.CreateProxy));

            AddNativeType(Noesis.ControllableStoryboardAction.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ControllableStoryboardAction), Noesis.ControllableStoryboardAction.CreateProxy));
            AddNativeType(Noesis.BeginStoryboard.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.BeginStoryboard), Noesis.BeginStoryboard.CreateProxy));
            AddNativeType(Noesis.PauseStoryboard.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.PauseStoryboard), Noesis.PauseStoryboard.CreateProxy));
            AddNativeType(Noesis.ResumeStoryboard.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ResumeStoryboard), Noesis.ResumeStoryboard.CreateProxy));
            AddNativeType(Noesis.StopStoryboard.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.StopStoryboard), Noesis.StopStoryboard.CreateProxy));

            AddNativeType(Noesis.VisualStateManager.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.VisualStateManager), Noesis.VisualStateManager.CreateProxy));
            AddNativeType(Noesis.VisualStateGroup.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.VisualStateGroup), Noesis.VisualStateGroup.CreateProxy));
            AddNativeType(Noesis.VisualStateGroupCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.VisualStateGroupCollection), Noesis.VisualStateGroupCollection.CreateProxy));
            AddNativeType(Noesis.VisualTransition.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.VisualTransition), Noesis.VisualTransition.CreateProxy));
            AddNativeType(Noesis.VisualTransitionCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.VisualTransitionCollection), Noesis.VisualTransitionCollection.CreateProxy));
            AddNativeType(Noesis.VisualState.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.VisualState), Noesis.VisualState.CreateProxy));
            AddNativeType(Noesis.VisualStateCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.VisualStateCollection), Noesis.VisualStateCollection.CreateProxy));


            AddNativeType(Noesis.ViewBase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ViewBase), Noesis.ViewBase.CreateProxy));
            AddNativeType(Noesis.GridView.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.GridView), Noesis.GridView.CreateProxy));
            AddNativeType(Noesis.GridViewColumn.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.GridViewColumn), Noesis.GridViewColumn.CreateProxy));
            AddNativeType(Noesis.GridViewColumnCollection.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.GridViewColumnCollection), Noesis.GridViewColumnCollection.CreateProxy));
            AddNativeType(Noesis.GridViewColumnHeader.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.GridViewColumnHeader), Noesis.GridViewColumnHeader.CreateProxy));
            AddNativeType(Noesis.GridViewRowPresenterBase.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.GridViewRowPresenterBase), Noesis.GridViewRowPresenterBase.CreateProxy));
            AddNativeType(Noesis.GridViewRowPresenter.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.GridViewRowPresenter), Noesis.GridViewRowPresenter.CreateProxy));
            AddNativeType(Noesis.GridViewHeaderRowPresenter.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.GridViewHeaderRowPresenter), Noesis.GridViewHeaderRowPresenter.CreateProxy));
            AddNativeType(Noesis.ListView.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ListView), Noesis.ListView.CreateProxy));
            AddNativeType(Noesis.ListViewItem.GetStaticType(), new NativeTypeComponentInfo(NativeTypeKind.Component, typeof(Noesis.ListViewItem), Noesis.ListViewItem.CreateProxy));

            _managedTypes[typeof(object)] = Noesis.BaseComponent.GetStaticType();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void UnregisterNativeTypes()
        {
            Shutdown();

            _assemblies = null;

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

            if (type.GetTypeInfo().IsInterface)
            {
                nativeType = Noesis.BaseComponent.GetStaticType();
                _managedTypes[type] = nativeType;
                return nativeType;
            }
            else if (type == typeof(System.Type))
            {
                nativeType = Noesis.ResourceKeyType.GetStaticType();
                _managedTypes[type] = nativeType;
                return nativeType;
            }
            else if (type.GetTypeInfo().IsEnum)
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

            if (type.GetTypeInfo().IsSubclassOf(typeof(Noesis.BaseComponent)))
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
            PropertyInfo[] props = GetPublicProperties(type);
            NativeTypeInfo info = CreateNativeTypeInfo(type, indexer, props);
            AddNativeType(nativeType, info);

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
            if (typeof(Noesis.DependencyObject).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                #if NETFX_CORE
                MethodInfo propChanged = type.GetRuntimeMethod("DependencyPropertyChanged", new Type[] { typeof(DependencyProperty) });
                #else
                MethodInfo propChanged = type.GetMethod("DependencyPropertyChanged", new Type[] { typeof(DependencyProperty) });
                #endif

                if (indexer != null)
                {
                    return new NativeTypeDOIndexerInfo(NativeTypeKind.Extended, type, TypeCreator(type), indexer, propChanged);
                }
                else if (props.Length > 0)
                {
                    return new NativeTypeDOPropsInfo(NativeTypeKind.Extended, type, TypeCreator(type), propChanged);
                }
                else
                {
                    return new NativeTypeDOExtendedInfo(NativeTypeKind.Extended, type, TypeCreator(type), propChanged);
                }
            }
            else
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
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static ExtendTypeData CreateNativeTypeData(System.Type type, IntPtr nativeType)
        {
            ExtendTypeData typeData = new ExtendTypeData();
            typeData.type = nativeType.ToInt64();
            typeData.baseType = EnsureNativeType(type.GetTypeInfo().BaseType).ToInt64();

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
                    if (p.GetGetMethod() != null && !p.PropertyType.IsPointer &&
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
        private static void RegisterDependencyProperties(System.Type type)
        {
            if (typeof(DependencyObject).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                RunClassConstructor(type);
                return;
            }

#if NETFX_CORE
            var fields = type.GetTypeInfo().DeclaredFields.Where(p => p.IsStatic);
            if (fields.Any())
            {
#else
            var fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (fields.Length > 0)
            {
#endif
                RunClassConstructor(type);
            }
        }

        private static void RunClassConstructor(Type type)
        {
            // Ensure that static constructor is executed, so dependency properties are correctly
            // registered or overridden in Noesis reflection

            if (_constructedTypes.Add(type))
            {
                // New type added to the HashSet - ensure constructor was invoked.
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            }
            else
            {
                // We've already ensured the class constructor was called sometime.
                // With this trick we force CLR to call the constructor again.
#if NETFX_CORE
                ConstructorInfo initializer = type.GetTypeInfo().DeclaredConstructors.Where(x => x.IsStatic).FirstOrDefault();
#else
                ConstructorInfo initializer = type.GetTypeInfo().TypeInitializer;
#endif
                if (initializer != null)
                {
                    initializer.Invoke(null, null);
                }
            }
        }

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
        public static ResourceKeyType GetResourceKeyType(System.Type type)
        {
            if (type != null)
            {
                IntPtr nativeType = EnsureNativeType(type);
                return new ResourceKeyType(Noesis_GetResourceKeyType(nativeType), false);
            }
            else
            {
                return null;
            }
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
                return null;
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
        private static System.Reflection.Assembly[] _assemblies = null;

        [MonoPInvokeCallback(typeof(Callback_RegisterType))]
        private static void RegisterType(string typeName)
        {
            try
            {
                // First, look for in the currently executing assembly and in Mscorlib.dll
                // Note that this step is mandatory in WINRT because in that platform only the
                // assemblies found in InstalledLocation are manually loaded
                Type type = Type.GetType(typeName);

                if (type == null)
                {
                    if (_assemblies == null)
                    {
                        _assemblies = AppDomain.CurrentDomain.GetAssemblies();

                        // We want to use the types from latest loaded assemblies, so we reverse it
                        Array.Reverse(_assemblies);
                    }

                    int assembliesLen = _assemblies.Length;
                    for (int i = 0; i < assembliesLen; ++i)
                    {
                        type = _assemblies[i].GetType(typeName);
                        if (type != null)
                        {
                            break;
                        }
                    }
                }

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
        private delegate uint Callback_GetHashCode(IntPtr cPtr);
        private static Callback_GetHashCode _getHashCode = GetHashCodeEx;

        [MonoPInvokeCallback(typeof(Callback_GetHashCode))]
        private static uint GetHashCodeEx(IntPtr cPtr)
        {
            try
            {
                object proxy = GetExtendInstance(cPtr);
                return proxy != null ? (uint)proxy.GetHashCode() : 0;
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_OnPostInit(IntPtr cPtrType, IntPtr cPtr);
        private static Callback_OnPostInit _onPostInit = OnPostInit;

        [MonoPInvokeCallback(typeof(Callback_OnPostInit))]
        private static void OnPostInit(IntPtr cPtrType, IntPtr cPtr)
        {
            try
            {
                NativeTypeInfo info = GetNativeTypeInfo(cPtrType);
#if NETFX_CORE
                MethodInfo methodInfo = info.Type.GetRuntimeMethod("OnPostInit", new Type[] { });
#else
                MethodInfo methodInfo = info.Type.GetMethod("OnPostInit", new Type[] { });
#endif
                if (methodInfo != null)
                {
                    object instance = GetExtendInstance(cPtr);
                    if (instance != null)
                    {
                        methodInfo.Invoke(instance, null);
                    }
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_DependencyPropertyChanged(IntPtr cPtrType, IntPtr cPtr, IntPtr dpPtr);
        private static Callback_DependencyPropertyChanged _dependencyPropertyChanged = DependencyPropertyChanged;

        [MonoPInvokeCallback(typeof(Callback_DependencyPropertyChanged))]
        private static void DependencyPropertyChanged(IntPtr cPtrType, IntPtr cPtr, IntPtr dpPtr)
        {
            // We don't want to raise more property change notifications after shutdown is called,
            // dependency properties created in Unity are already deleted
            if (Initialized)
            {
                try
                {
                    NativeTypeInfo info = GetNativeTypeInfo(cPtrType);
                    INativeTypeDO dob = (INativeTypeDO)info;
                    if (dob.PropertyChangedInfo != null)
                    {
                        object instance = GetExtendInstance(cPtr);
                        if (instance != null)
                        {
                            DependencyProperty dp = new DependencyProperty(dpPtr, false);
                            dob.PropertyChangedInfo.OnPropertyChanged(instance, dp);
                        }
                    }
                }
                catch (Exception e)
                {
                    Error.UnhandledException(e);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_FrameworkElementMeasure(IntPtr cPtr,
            IntPtr availableSizePtr, IntPtr desiredSizePtr, [MarshalAs(UnmanagedType.U1)]ref bool callBase);
        private static Callback_FrameworkElementMeasure _frameworkElementMeasure = FrameworkElementMeasure;

        [MonoPInvokeCallback(typeof(Callback_FrameworkElementMeasure))]
        private static void FrameworkElementMeasure(IntPtr cPtr,
            IntPtr availableSizePtr, IntPtr desiredSizePtr, ref bool callBase)
        {
            try
            {
                FrameworkElement element = (FrameworkElement)GetExtendInstance(cPtr);
                if (element != null)
                {
                    Size availableSize = Marshal.PtrToStructure<Size>(availableSizePtr);
                    Size desiredSize = element.CallMeasureOverride(availableSize, out callBase);
                    if (desiredSize.IsEmpty) desiredSize = new Size(0, 0);
                    Marshal.StructureToPtr(desiredSize, desiredSizePtr, false);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private delegate void Callback_FrameworkElementArrange(IntPtr cPtr,
            IntPtr finalSizePtr, IntPtr renderSizePtr, [MarshalAs(UnmanagedType.U1)]ref bool callBase);
        private static Callback_FrameworkElementArrange _frameworkElementArrange = FrameworkElementArrange;

        [MonoPInvokeCallback(typeof(Callback_FrameworkElementArrange))]
        private static void FrameworkElementArrange(IntPtr cPtr,
            IntPtr finalSizePtr, IntPtr renderSizePtr, ref bool callBase)
        {
            try
            {
                FrameworkElement element = (FrameworkElement)GetExtendInstance(cPtr);
                if (element != null)
                {
                    Size finalSize = Marshal.PtrToStructure<Size>(finalSizePtr);
                    Size renderSize = element.CallArrangeOverride(finalSize, out callBase);
                    if (renderSize.IsEmpty) renderSize = new Size(0, 0);
                    Marshal.StructureToPtr(renderSize, renderSizePtr, false);
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
                    return element.CallConnectEvent(source, eventName, handlerName);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }

            return false;
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
                var converter = (Noesis.IValueConverter)GetExtendInstance(cPtr);
                if (converter != null)
                {
                    NativeTypeInfo targetType = GetNativeTypeInfo(targetTypePtr);
                    object val = GetProxy(valType, valPtr, false);
                    object param = GetProxy(paramType, paramPtr, false);

                    object obj = converter.Convert(val, targetType.Type, param, CultureInfo.CurrentCulture);

                    if (AreCompatibleTypes(obj, targetType.Type))
                    {
                        result = GetInstanceHandle(obj).Handle;
                        return true;
                    }
                    else
                    {
                        Log.Error(string.Format("{0} Convert() expects {1} and {2} is returned",
                            converter.GetType().FullName, targetType.Type.FullName,
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
                var converter = (Noesis.IValueConverter)GetExtendInstance(cPtr);
                if (converter != null)
                {
                    NativeTypeInfo targetType = GetNativeTypeInfo(targetTypePtr);
                    object val = GetProxy(valType, valPtr, false);
                    object param = GetProxy(paramType, paramPtr, false);

                    object obj = converter.ConvertBack(val, targetType.Type, param, CultureInfo.CurrentCulture);

                    if (AreCompatibleTypes(obj, targetType.Type))
                    {
                        result = GetInstanceHandle(obj).Handle;
                        return true;
                    }
                    else
                    {
                        Log.Error(string.Format("{0} ConvertBack() expects {1} and {2} is returned",
                            converter.GetType().FullName, targetType.Type.FullName,
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
                    System.IO.Stream stream = provider.LoadXaml(filename_);
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
            ref uint width, ref uint height);
        private static Callback_ProviderTextureInfo _providerTextureInfo = ProviderTextureInfo;

        [MonoPInvokeCallback(typeof(Callback_ProviderTextureInfo))]
        private static void ProviderTextureInfo(IntPtr cPtr, IntPtr filename,
            ref uint width, ref uint height)
        {
            width = height = 0;

            try
            {
                TextureProvider provider = (TextureProvider)GetExtendInstance(cPtr);
                if (provider != null)
                {
                    string filename_ = StringFromNativeUtf8(filename);
                    provider.GetTextureInfo(filename_, out width, out height);
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
                    Texture texture = provider.LoadTexture(filename_);
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
                    System.IO.Stream stream = provider.OpenStream(filename_);
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
                    provider.ScanFolder(folder_);
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
        private static IntPtr ProviderOpenFont(IntPtr cPtr, IntPtr folder, IntPtr id)
        {
            try
            {
                FontProvider provider = (FontProvider)GetExtendInstance(cPtr);
                if (provider != null)
                {
                    string folder_ = StringFromNativeUtf8(folder);
                    string id_ = StringFromNativeUtf8(id);
                    System.IO.Stream stream = provider.OpenFont(folder_, id_);
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
        private enum NativePropertyType
        {
            Bool,
            Float,
            Double,
            Int,
            UInt,
            Short,
            UShort,
            Color,
            Point,
            Rect,
            Size,
            Thickness,
            CornerRadius,
            TimeSpan,
            Duration,
            KeyTime,
            NullableBool,
            NullableFloat,
            NullableDouble,
            NullableInt,
            NullableUInt,
            NullableShort,
            NullableUShort,
            NullableColor,
            NullablePoint,
            NullableRect,
            NullableSize,
            NullableThickness,
            NullableCornerRadius,
            NullableTimeSpan,
            NullableDuration,
            NullableKeyTime,
            Enum,
            String,
            Uri,
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
        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool Callback_GetPropertyInfo(IntPtr nativeType, string propName,
            ref int propIndex, ref int propType);
        private static Callback_GetPropertyInfo _getPropertyInfo = GetPropertyInfo;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyInfo))]
        private static bool GetPropertyInfo(IntPtr nativeType, string propName,
            ref int propertyIndex, ref int propertyType)
        {
            try
            {
                NativeTypePropsInfo info = GetNativeTypeInfo(nativeType) as NativeTypePropsInfo;
                if (info != null)
                {
                    propertyIndex = 0;
                    int propsLen = info.Properties.Count;
                    for (int i = 0; i < propsLen; ++i)
                    {
                        var p = info.Properties[i];
                        if (p.Property.Name == propName)
                        {
                            propertyType = GetNativePropertyType(p.Property.PropertyType);
                            return true;
                        }

                        ++propertyIndex;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return false;
            }
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
            if (instance != null)
            {
                if (!prop.IsNullable)
                {
                    isNull = false;
                    return GetPropertyValue<T>(prop, instance);
                }
                else
                {
                    T? value = ((PropertyAccessorT<T?>)prop).Get(instance);
                    isNull = !value.HasValue;
                    return value.GetValueOrDefault();
                }
            }

            isNull = false;
            return default(T);
        }

        private static void GetPropertyValueStruct<T>(PropertyAccessor prop, object instance,
            IntPtr valuePtr, out bool isNull) where T: struct
        {
            if (instance != null)
            {
                if (!prop.IsNullable)
                {
                    isNull = false;
                    Marshal.StructureToPtr(((PropertyAccessorT<T>)prop).Get(instance), valuePtr, false);
                }
                else
                {
                    T? value = ((PropertyAccessorT<T?>)prop).Get(instance);
                    isNull = !value.HasValue;
                    if (value.HasValue)
                    {
                        Marshal.StructureToPtr(value.Value, valuePtr, false);
                    }
                }
            }

            isNull = false;
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
                return Marshal.StringToHGlobalUni(uri != null ? UriHelper.GetPath(uri) : string.Empty);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
                return Marshal.StringToHGlobalUni(string.Empty);
            }
        }

        private delegate void Callback_GetPropertyValue_Color(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Color _getPropertyValue_Color = GetPropertyValue_Color;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Color))]
        private static void GetPropertyValue_Color(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, ref bool isNull)
        {
            try
            {
                GetPropertyValueStruct<Noesis.Color>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), valuePtr, out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_Point(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Point _getPropertyValue_Point = GetPropertyValue_Point;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Point))]
        private static void GetPropertyValue_Point(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, ref bool isNull)
        {
            try
            {
                GetPropertyValueStruct<Noesis.Point>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), valuePtr, out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_Rect(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Rect _getPropertyValue_Rect = GetPropertyValue_Rect;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Rect))]
        private static void GetPropertyValue_Rect(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, ref bool isNull)
        {
            try
            {
                GetPropertyValueStruct<Noesis.Rect>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), valuePtr, out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_Size(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Size _getPropertyValue_Size = GetPropertyValue_Size;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Size))]
        private static void GetPropertyValue_Size(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, ref bool isNull)
        {
            try
            {
                GetPropertyValueStruct<Noesis.Size>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), valuePtr, out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_Thickness(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Thickness _getPropertyValue_Thickness = GetPropertyValue_Thickness;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Thickness))]
        private static void GetPropertyValue_Thickness(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, ref bool isNull)
        {
            try
            {
                GetPropertyValueStruct<Noesis.Thickness>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), valuePtr, out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_CornerRadius(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_CornerRadius _getPropertyValue_CornerRadius = GetPropertyValue_CornerRadius;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_CornerRadius))]
        private static void GetPropertyValue_CornerRadius(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, ref bool isNull)
        {
            try
            {
                GetPropertyValueStruct<Noesis.CornerRadius>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), valuePtr, out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_TimeSpan(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_TimeSpan _getPropertyValue_TimeSpan = GetPropertyValue_TimeSpan;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_TimeSpan))]
        private static void GetPropertyValue_TimeSpan(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, ref bool isNull)
        {
            try
            {
                GetPropertyValueStruct<Noesis.TimeSpanStruct>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), valuePtr, out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_Duration(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_Duration _getPropertyValue_Duration = GetPropertyValue_Duration;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_Duration))]
        private static void GetPropertyValue_Duration(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, ref bool isNull)
        {
            try
            {
                GetPropertyValueStruct<Noesis.Duration>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), valuePtr, out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_GetPropertyValue_KeyTime(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, [MarshalAs(UnmanagedType.U1)]ref bool isNull);
        private static Callback_GetPropertyValue_KeyTime _getPropertyValue_KeyTime = GetPropertyValue_KeyTime;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_KeyTime))]
        private static void GetPropertyValue_KeyTime(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr valuePtr, ref bool isNull)
        {
            try
            {
                GetPropertyValueStruct<Noesis.KeyTime>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), valuePtr, out isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate IntPtr Callback_GetPropertyValue_BaseComponent(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr);
        private static Callback_GetPropertyValue_BaseComponent _getPropertyValue_BaseComponent = GetPropertyValue_BaseComponent;

        [MonoPInvokeCallback(typeof(Callback_GetPropertyValue_BaseComponent))]
        private static IntPtr GetPropertyValue_BaseComponent(IntPtr nativeType, int propertyIndex, IntPtr cPtr)
        {
            try
            {
                object obj = GetPropertyValue<object>(GetProperty(nativeType, propertyIndex), GetExtendInstance(cPtr));
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
            if (instance != null)
            {
                if (!prop.IsNullable)
                {
                    SetPropertyValue<T>(prop, instance, value);
                }
                else
                {
                    ((PropertyAccessorT<T?>)prop).Set(instance, isNull ? (T?)null : (T?)value);
                }
            }
        }

        private static void SetPropertyValueStruct<T>(PropertyAccessor prop, object instance,
            IntPtr valuePtr, bool isNull) where T: struct
        {
            if (instance != null)
            {
                if (!prop.IsNullable)
                {
                    ((PropertyAccessorT<T>)prop).Set(instance, Marshal.PtrToStructure<T>(valuePtr));
                }
                else
                {
                    ((PropertyAccessorT<T?>)prop).Set(instance, isNull ? (T?)null :
                        (T?)Marshal.PtrToStructure<T>(valuePtr));
                }
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
                Uri uri = new Uri(StringFromNativeUtf8(val), UriKind.Relative);
                SetPropertyValue<Uri>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), uri);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Color(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Color _setPropertyValue_Color = SetPropertyValue_Color;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Color))]
        private static void SetPropertyValue_Color(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val, bool isNull)
        {
            try
            {
                SetPropertyValueStruct<Noesis.Color>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Point(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Point _setPropertyValue_Point = SetPropertyValue_Point;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Point))]
        private static void SetPropertyValue_Point(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val, bool isNull)
        {
            try
            {
                SetPropertyValueStruct<Noesis.Point>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Rect(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Rect _setPropertyValue_Rect = SetPropertyValue_Rect;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Rect))]
        private static void SetPropertyValue_Rect(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val, bool isNull)
        {
            try
            {
                SetPropertyValueStruct<Noesis.Rect>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Size(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Size _setPropertyValue_Size = SetPropertyValue_Size;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Size))]
        private static void SetPropertyValue_Size(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val, bool isNull)
        {
            try
            {
                SetPropertyValueStruct<Noesis.Size>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Thickness(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Thickness _setPropertyValue_Thickness = SetPropertyValue_Thickness;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Thickness))]
        private static void SetPropertyValue_Thickness(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val, bool isNull)
        {
            try
            {
                SetPropertyValueStruct<Noesis.Thickness>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_CornerRadius(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_CornerRadius _setPropertyValue_CornerRadius = SetPropertyValue_CornerRadius;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_CornerRadius))]
        private static void SetPropertyValue_CornerRadius(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val, bool isNull)
        {
            try
            {
                SetPropertyValueStruct<Noesis.CornerRadius>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_TimeSpan(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_TimeSpan _setPropertyValue_TimeSpan = SetPropertyValue_TimeSpan;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_TimeSpan))]
        private static void SetPropertyValue_TimeSpan(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val, bool isNull)
        {
            try
            {
                SetPropertyValueStruct<Noesis.TimeSpanStruct>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_Duration(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_Duration _setPropertyValue_Duration = SetPropertyValue_Duration;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_Duration))]
        private static void SetPropertyValue_Duration(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val, bool isNull)
        {
            try
            {
                SetPropertyValueStruct<Noesis.Duration>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private delegate void Callback_SetPropertyValue_KeyTime(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val,
            [MarshalAs(UnmanagedType.U1)] bool isNull);
        private static Callback_SetPropertyValue_KeyTime _setPropertyValue_KeyTime = SetPropertyValue_KeyTime;

        [MonoPInvokeCallback(typeof(Callback_SetPropertyValue_KeyTime))]
        private static void SetPropertyValue_KeyTime(IntPtr nativeType, int propertyIndex,
            IntPtr cPtr, IntPtr val, bool isNull)
        {
            try
            {
                SetPropertyValueStruct<Noesis.KeyTime>(GetProperty(nativeType, propertyIndex),
                    GetExtendInstance(cPtr), val, isNull);
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
        public static IntPtr NewCPtr(System.Type type, object instance)
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
                if (Initialized)
                {
                    RemoveExtendInfo(cPtr);
                }
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

                            RemoveWeakExtend(cPtr);
                            AddExtendPtr(extend.instance, cPtr);
                        }
                        else
                        {
                            RemoveExtendPtr(extend.instance);
                            AddWeakExtend(cPtr, extend.instance, extend.weak);

                            extend.instance = null;
                        }
                    }
                }
                else if (!grab)
                {
                    ExtendInfo extend;
                    if (_extends.TryGetValue(cPtr.ToInt64(), out extend))
                    {
                        object instance = extend.weak.Target;
                        if (instance == null && !_pendingRelease.Contains(cPtr))
                        {
                            // During shutdown C# proxies are all converted to weak references, so
                            // they can be marked for finalize. In this situation we need to force
                            // the release the C++ instance for proxies that are already destroyed

                            BaseComponent.Release(cPtr);
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
            ExtendInfo extend = GetExtendInfo(cPtr);
            if (extend != null)
            {
                object instance = extend.weak.Target;

                if (ownMemory)
                {
                    BaseComponent.Release(cPtr);
                }

                return instance;
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static ExtendInfo GetExtendInfo(IntPtr cPtr)
        {
            ExtendInfo extend;

            lock (_extends)
            {
                if (!_extends.TryGetValue(cPtr.ToInt64(), out extend))
                {
                    Log.Error("Extend already removed");
                    return null;
                }
                else if (extend == null || extend.weak.Target == null)
                {
                    Log.Error("Extend already destroyed");
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
            }

            AddWeakExtend(cPtr, instance, extend.weak);
        }

        private static void RemoveExtendInfo(IntPtr cPtr)
        {
            lock (_extends)
            {
                _extends.Remove(cPtr.ToInt64());
            }

            RemoveWeakExtend(cPtr);
        }

        public static void ForceRemoveExtend(object instance, IntPtr cPtr)
        {
            RemoveWeakExtend(cPtr);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static void AddExtendPtr(object instance, IntPtr cPtr)
        {
            if (!(instance is BaseComponent))
            {
                lock (_extendPtrs)
                {
                    _extendPtrs.Add(instance, cPtr);
                }
            }
        }

        private static void RemoveExtendPtr(object instance)
        {
            if (!(instance is BaseComponent))
            {
                lock (_extendPtrs)
                {
                    _extendPtrs.Remove(instance);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static void AddWeakExtend(IntPtr cPtr, object instance, WeakReference weak)
        {
            if (!(instance is BaseComponent))
            {
                lock (_weakExtends)
                {
                    long ptr = cPtr.ToInt64();

                    WeakInfo info = new WeakInfo
                    {
                        hash = RuntimeHelpers.GetHashCode(instance),
                        weak = weak
                    };

                    _weakExtends.Add(ptr, info);

                    // New instance can have the same hash as an already destroyed old one that is
                    // still referenced in _weakExtends, so we get rid of that old one first
                    _weakExtendsHash.Remove(info.hash);
                    _weakExtendsHash.Add(info.hash, ptr);
                }
            }
        }

        private static void RemoveWeakExtend(IntPtr cPtr)
        {
            lock (_weakExtends)
            {
                long ptr = cPtr.ToInt64();

                // Only remove hashed reference if it wasn't already swapped with a new instance
                WeakInfo info;
                if (_weakExtends.TryGetValue(ptr, out info))
                {
                    long hashedPtr;
                    if (_weakExtendsHash.TryGetValue(info.hash, out hashedPtr))
                    {
                        if (hashedPtr == ptr)
                        {
                            _weakExtendsHash.Remove(info.hash);
                        }
                    }
                }

                _weakExtends.Remove(ptr);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static void AddDestroyedExtends()
        {
            lock (_weakExtends)
            {
                var enumerator = _weakExtends.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.Value.weak.Target == null)
                        {
                            IntPtr cPtr = new IntPtr(enumerator.Current.Key);
                            AddPendingRelease(cPtr);
                        }
                    }
                }
                finally
                {
                    enumerator.Dispose();
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private class ExtendInfo
        {
            public object instance;
            public WeakReference weak;
        }

        private struct WeakInfo
        {
            public int hash;
            public WeakReference weak;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static Dictionary<long, ExtendInfo> _extends = new Dictionary<long, ExtendInfo>();
        private static Dictionary<object, IntPtr> _extendPtrs = new Dictionary<object, IntPtr>();
        private static Dictionary<long, WeakInfo> _weakExtends = new Dictionary<long, WeakInfo>();
        private static Dictionary<int, long> _weakExtendsHash = new Dictionary<int, long>();

        ////////////////////////////////////////////////////////////////////////////////////////////////
        private static BaseComponent GetProxyInstance(IntPtr cPtr, bool ownMemory, NativeTypeInfo info)
        {
            long ptr = cPtr.ToInt64();
            lock (_proxies)
            {
                WeakReference wr;
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
                            return component;
                        }
                    }
                }
            }

            return ((NativeTypeComponentInfo)info).Creator(cPtr, ownMemory);
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
                        cPtr = NewCPtr(instance.GetType(), instance);
                        AddExtendInfo(cPtr, instance);
                        RegisterInterfaces(instance);
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
                lock (_extendPtrs)
                {
                    _extendPtrs.TryGetValue(instance, out cPtr);
                }

                if (cPtr == IntPtr.Zero)
                {
                    cPtr = FindWeakInstancePtr(instance);
                }
            }

            return cPtr;
        }

        private static IntPtr FindWeakInstancePtr(object instance)
        {
            IntPtr cPtr = IntPtr.Zero;

            lock (_weakExtends)
            {
                long ptr;
                if (_weakExtendsHash.TryGetValue(RuntimeHelpers.GetHashCode(instance), out ptr))
                {
                    WeakInfo info;
                    if (_weakExtends.TryGetValue(ptr, out info))
                    {
                        if (info.weak.Target == instance)
                        {
                            cPtr = new IntPtr(ptr);
                        }
                    }
                }
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
            AddDestroyedExtends();
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

                Noesis_LaunchPropertyChangedEvent(nativeType, GetInstanceHandle(sender).Handle, e.PropertyName);
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
