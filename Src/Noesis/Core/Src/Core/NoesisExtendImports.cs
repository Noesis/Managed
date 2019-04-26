using System;
using System.Runtime.InteropServices;

namespace Noesis
{
    internal partial class Extend
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_RegisterEnumType([MarshalAs(UnmanagedType.LPStr)]string typeName,
            int numEnums, IntPtr enumsData);

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        private static extern void Noesis_FillExtendType(ref ExtendTypeData typeData,
            int numProps, IntPtr propsData);

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_InstantiateExtend(IntPtr nativeType);

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        private static extern void Noesis_LaunchPropertyChangedEvent(IntPtr nativeType, IntPtr cPtr,
            [MarshalAs(UnmanagedType.LPStr)]string propertyName);

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        private static extern void Noesis_LaunchCollectionChangedEvent(IntPtr nativeType, IntPtr cPtr,
            int action, IntPtr newItem, IntPtr oldItem, int newIndex, int oldIndex);

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        private static extern void Noesis_LaunchCanExecuteChangedEvent(IntPtr nativeType, IntPtr cPtr);

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_GetResourceKeyType(IntPtr nativeType);

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        private static extern void Noesis_EnableExtend(bool enable);

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        private static extern void Noesis_ClearExtendTypes();

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        static extern void Noesis_RegisterReflectionCallbacks(
            Callback_FreeString callback_FreeString,
            Callback_RegisterType callback_RegisterType,
            Callback_OnPostInit callback_OnPostInit,
            Callback_DependencyPropertyChanged callback_DependencyPropertyChanged,
            Callback_FrameworkElementMeasure callback_FrameworkElementMeasure,
            Callback_FrameworkElementArrange callback_FrameworkElementArrange,
            Callback_FrameworkElementConnectEvent callback_FrameworkElementConnectEvent,
            Callback_FreezableClone callback_FreezableClone,
            Callback_ToString callback_ToString,
            Callback_Equals callback_Equals,
            Callback_GetHashCode callback_GetHashCode,
            Callback_CommandCanExecute callback_CommandCanExecute,
            Callback_CommandExecute callback_CommandExecute,
            Callback_ConverterConvert callback_ConverterConvert,
            Callback_ConverterConvertBack callback_ConverterConvertBack,
            Callback_ListCount callback_ListCount,
            Callback_ListGet callback_ListGet,
            Callback_ListSet callback_ListSet,
            Callback_ListAdd callback_ListAdd,
            Callback_ListIndexOf callback_ListIndexOf,
            Callback_DictionaryFind callback_DictionaryFind,
            Callback_DictionarySet callback_DictionarySet,
            Callback_DictionaryAdd callback_DictionaryAdd,
            Callback_ListIndexerTryGet callback_ListIndexerTryGet,
            Callback_ListIndexerTrySet callback_ListIndexerTrySet,
            Callback_DictionaryIndexerTryGet callback_DictionaryIndexerTryGet,
            Callback_DictionaryIndexerTrySet callback_DictionaryIndexerTrySet,
            Callback_SelectTemplate callback_SelectTemplate,
            Callback_StreamSetPosition callback_StreamSetPosition,
            Callback_StreamGetPosition callback_StreamGetPosition,
            Callback_StreamGetLength callback_StreamGetLength,
            Callback_StreamRead callback_StreamRead,
            Callback_ProviderLoadXaml callback_ProviderLoadXaml,
            Callback_ProviderTextureInfo callback_ProviderTextureInfo,
            Callback_ProviderTextureLoad callback_ProviderTextureLoad,
            Callback_ProviderTextureOpen callback_ProviderTextureOpen,
            Callback_ProviderScanFolder callback_ProviderScanFolder,
            Callback_ProviderOpenFont callback_ProviderOpenFont,
            Callback_GetPropertyInfo callback_GetPropertyInfo,
            Callback_GetPropertyValue_Bool callback_GetPropertyValue_Bool,
            Callback_GetPropertyValue_Float callback_GetPropertyValue_Float,
            Callback_GetPropertyValue_Double callback_GetPropertyValue_Double,
            Callback_GetPropertyValue_Int callback_GetPropertyValue_Int,
            Callback_GetPropertyValue_UInt callback_GetPropertyValue_UInt,
            Callback_GetPropertyValue_Short callback_GetPropertyValue_Short,
            Callback_GetPropertyValue_UShort callback_GetPropertyValue_UShort,
            Callback_GetPropertyValue_String callback_GetPropertyValue_String,
            Callback_GetPropertyValue_Uri callback_GetPropertyValue_Uri,
            Callback_GetPropertyValue_Color callback_GetPropertyValue_Color,
            Callback_GetPropertyValue_Point callback_GetPropertyValue_Point,
            Callback_GetPropertyValue_Rect callback_GetPropertyValue_Rect,
            Callback_GetPropertyValue_Size callback_GetPropertyValue_Size,
            Callback_GetPropertyValue_Thickness callback_GetPropertyValue_Thickness,
            Callback_GetPropertyValue_CornerRadius callback_GetPropertyValue_CornerRadius,
            Callback_GetPropertyValue_TimeSpan callback_GetPropertyValue_TimeSpan,
            Callback_GetPropertyValue_Duration callback_GetPropertyValue_Duration,
            Callback_GetPropertyValue_KeyTime callback_GetPropertyValue_KeyTime,
            Callback_GetPropertyValue_BaseComponent callback_GetPropertyValue_BaseComponent,
            Callback_SetPropertyValue_Bool callback_SetPropertyValue_Bool,
            Callback_SetPropertyValue_Float callback_SetPropertyValue_Float,
            Callback_SetPropertyValue_Double callback_SetPropertyValue_Double,
            Callback_SetPropertyValue_Int callback_SetPropertyValue_Int,
            Callback_SetPropertyValue_UInt callback_SetPropertyValue_UInt,
            Callback_SetPropertyValue_Short callback_SetPropertyValue_Short,
            Callback_SetPropertyValue_UShort callback_SetPropertyValue_UShort,
            Callback_SetPropertyValue_String callback_SetPropertyValue_String,
            Callback_SetPropertyValue_Uri callback_SetPropertyValue_Uri,
            Callback_SetPropertyValue_Color callback_SetPropertyValue_Color,
            Callback_SetPropertyValue_Point callback_SetPropertyValue_Point,
            Callback_SetPropertyValue_Rect callback_SetPropertyValue_Rect,
            Callback_SetPropertyValue_Size callback_SetPropertyValue_Size,
            Callback_SetPropertyValue_Thickness callback_SetPropertyValue_Thickness,
            Callback_SetPropertyValue_CornerRadius callback_SetPropertyValue_CornerRadius,
            Callback_SetPropertyValue_TimeSpan callback_SetPropertyValue_TimeSpan,
            Callback_SetPropertyValue_Duration callback_SetPropertyValue_Duration,
            Callback_SetPropertyValue_KeyTime callback_SetPropertyValue_KeyTime,
            Callback_SetPropertyValue_BaseComponent callback_SetPropertyValue_BaseComponent,
            Callback_CreateInstance callback_CreateInstance,
            Callback_DeleteInstance callback_DeleteInstance,
            Callback_GrabInstance callback_GrabInstance);
    }
}

