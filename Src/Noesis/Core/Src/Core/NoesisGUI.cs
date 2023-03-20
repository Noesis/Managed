using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Noesis
{
    /// <summary> Called when a text control requires to show or hide software keyboard </summary>
    public delegate void SoftwareKeyboardCallback(UIElement focused, bool open);

    /// <summary> Called when View requires to update its cursor </summary>
    public delegate void UpdateCursorCallback(View view, Cursor cursor);

    /// <summary> Called when an element requires to open an URL </summary>
    public delegate void OpenUrlCallback(string url);

    /// <summary> Called when an element wants to play a sound </summary>
    public delegate void PlayAudioCallback(Uri uri, float volume);

    /// <summary> Called for every xaml dependency found </summary>
    public delegate void XamlDependencyCallback(Uri uri, XamlDependencyType type);

    /// <summary> Called for every font face found in the ttf </summary>
    public delegate void FontFaceInfoCallback(int index, string familyName, FontWeight weight,
        FontStyle style, FontStretch stretch);

    /// <summary> Called everytime an assembly is requested by xaml loading</summary>
    public delegate void LoadAssemblyCallback(string assembly);

    public static class GUI
    {
        /// <summary>
        /// Returns the build version, for example "1.2.6f5".
        /// </summary>
        public static string GetBuildVersion()
        {
            IntPtr version = Noesis_GetBuildVersion();
            return Extend.StringFromNativeUtf8(version);
        }

        /// <summary>
        /// Disables Hot Reload feature. It is enabled by default and takes a bit of extra memory
        /// Must be invoked before Noesis.GUI.Init()
        /// </summary>
        public static void DisableHotReload()
        {
            Noesis_DisableHotReload();
        }

        /// <summary>
        /// If sockets are already initialized before Noesis, use this function to avoid double init
        /// Must be invoked before Noesis.GUI.Init()
        /// </summary>
        public static void DisableSocketInit()
        {
            Noesis_DisableSocketInit();
        }

        /// <summary>
        /// Disables all connections from the remote Inspector tool.
        /// Must be invoked before Noesis.GUI.Init()
        /// </summary>
        public static void DisableInspector()
        {
            Noesis_DisableInspector();
        }

        /// <summary>
        /// Returns whether the remote Inspector is currently connected.
        /// </summary>
        public static bool IsInspectorConnected
        {
            get { return Noesis_IsInspectorConnected(); }
        }

        /// <summary>
        /// Keeps alive the Inspector connection. Only needed if Inspector is connected before any
        /// view is created. Views call this function internally when updated
        /// </summary>
        public static void UpdateInspector()
        {
            Noesis_UpdateInspector();
        }

        /// <summary>
        /// Sets the active license.
        /// </summary>
        public static void SetLicense(string name, string key)
        {
            Noesis_SetLicense(name, key);
        }

        private static bool _initialized = false;

        /// <summary>
        /// Initializes NoesisGUI. Use Name and Key provided when you purchased your NoesisGUI license.
        /// </summary>
        public static void Init()
        {
            if (!_initialized)
            {
                _initialized = true;

                UriHelper.RegisterPack();
                Extend.RegisterCallbacks();

                Noesis_Init();

                Extend.Init();
            }
        }

        /// <summary>
        /// Shuts down NoesisGUI library.
        /// </summary>
        public static void Shutdown()
        {
            Noesis_SetPlayAudioCallback(null);
            Noesis_SetOpenUrlCallback(null);
            Noesis_SetCursorCallback(null);
            Noesis_SetSoftwareKeyboardCallback(null);
            Noesis_SetLoadAssemblyCallback(null);

            Extend.Shutdown();

            Noesis_Shutdown();

            Extend.UnregisterCallbacks();

            _initialized = false;
        }

        /// <summary>
        /// Sets XamlProvider to load XAML resources.
        /// </summary>
        public static void SetXamlProvider(XamlProvider provider)
        {
            Noesis_SetXamlProvider(Extend.GetInstanceHandle(provider));
        }

        /// <summary>
        /// Sets TextureProvider to load texture resources.
        /// </summary>
        public static void SetTextureProvider(TextureProvider provider)
        {
            Noesis_SetTextureProvider(Extend.GetInstanceHandle(provider));
        }

        /// <summary>
        /// Sets FontProvider to load font resources.
        /// </summary>
        public static void SetFontProvider(FontProvider provider)
        {
            Noesis_SetFontProvider(Extend.GetInstanceHandle(provider));
        }

        /// <summary>
        /// Sets the family names to be used by the global font fallback mechanism. These fonts are used
        /// whenever the FontFamily of an element does not contain needed glyphs. Fallback sequence can
        /// specify font locations, for example { "./Fonts/#Pericles Light", "Tahoma" , "Verdana" } 
        /// </summary>
        public static void SetFontFallbacks(string[] familyNames)
        {
            if (familyNames == null)
            {
                throw new ArgumentNullException("familyNames");
            }

            Noesis_SetFontFallbacks(familyNames, familyNames.Length);
        }

        /// <summary>
        /// Sets default font properties to be used when not specified in an element
        /// </summary>
        public static void SetFontDefaultProperties(float size, FontWeight weight, FontStretch stretch,
            FontStyle style)
        {
            Noesis_SetFontDefaultProperties(size, (int)weight, (int)stretch, (int)style);
        }

        /// <summary>
        /// Loads the specified dictionary file as the application-scope resources.
        /// </summary>
        public static void LoadApplicationResources(string filename)
        {
            ResourceDictionary app = new ResourceDictionary();
            SetApplicationResources(app);
            app.Source = new Uri(filename, UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Sets a collection of application-scope resources, such as styles and brushes. Provides a
        /// simple way to support a consistent theme across your application.
        /// </summary>
        /// <param name="resources">Application resources.</param>
        public static void SetApplicationResources(ResourceDictionary resources)
        {
            Noesis_SetApplicationResources(Extend.GetInstanceHandle(resources));
        }

        /// <summary>
        /// Gets the application resource dictionary.
        /// </summary>
        public static ResourceDictionary GetApplicationResources()
        {
            IntPtr cPtr = Noesis_GetApplicationResources();
            return (ResourceDictionary)Noesis.Extend.GetProxy(cPtr, false);
        }

        /// <summary>
        /// Sets a callback that is called when a text control gets focus.
        /// </summary>
        public static void SetSoftwareKeyboardCallback(SoftwareKeyboardCallback callback)
        {
            _softwareKeyboardCallback = callback;
            Noesis_SetSoftwareKeyboardCallback(callback != null ? _softwareKeyboard : null);
        }

        /// <summary>
        /// Sets a callback that is called each time cursor icon needs to be updated.
        /// </summary>
        public static void SetCursorCallback(UpdateCursorCallback callback)
        {
            _updateCursorCallback = callback;
            Noesis_SetCursorCallback(callback != null ? _updateCursor : null);
        }

        /// <summary>
        /// Callback for opening URL in a browser.
        /// </summary>
        public static void SetOpenUrlCallback(OpenUrlCallback callback)
        {
            _openUrlCallback = callback;
            Noesis_SetOpenUrlCallback(callback != null ? _openUrl : null);
        }

        /// <summary>
        /// Opens the specified URL with the provided callback.
        /// </summary>
        public static void OpenUrl(string url)
        {
            if (_openUrlCallback != null)
            {
                _openUrlCallback(url);
            }
        }

        /// <summary>
        /// Callback for playing audio.
        /// </summary>
        public static void SetPlayAudioCallback(PlayAudioCallback callback)
        {
            _playAudioCallback = callback;
            Noesis_SetPlayAudioCallback(callback != null ? _playAudio : null);
        }

        /// <summary>
        /// Plays the specified sound with the provided callback.
        /// </summary>
        public static void PlayAudio(Uri uri, float volume)
        {
            if (_playAudioCallback != null)
            {
                _playAudioCallback(uri, volume);
            }
        }

        /// <summary>
        /// Finds dependencies to other XAMLS and resources (fonts, textures, sounds...).
        /// </summary>
        /// <param name="xaml">Stream with xaml content.</param>
        /// <param name="baseUri">Xaml file path used as base uri for dependencies.</param>
        /// <param name="callback">Called for each dependency found.</param>
        public static void GetXamlDependencies(Stream xaml, string baseUri,
            XamlDependencyCallback callback)
        {
            Deps deps = new Deps { Callback = callback };
            int callbackId = deps.GetHashCode();
            _depsCallbacks[callbackId] = deps;
            Noesis_GetXamlDependencies(Extend.GetInstanceHandle(xaml), baseUri, callbackId, _xamlDep);
            _depsCallbacks.Remove(callbackId);
        }

        /// <summary>
        /// Loads a XAML resource from a Stream.
        /// </summary>
        /// <param name="stream">Stream with xaml contents.</param>
        /// <param name="filename">Path to the resource.</param>
        /// <returns>Root of the loaded XAML.</returns>
        public static object LoadXaml(Stream stream, string filename)
        {
            IntPtr root = Noesis_LoadStreamXaml(Extend.GetInstanceHandle(stream), filename ?? string.Empty);
            return Extend.GetProxy(root, true);
        }


        /// <summary>
        /// Loads a XAML resource.
        /// </summary>
        /// <param name="filename">Path to the resource.</param>
        /// <returns>Root of the loaded XAML.</returns>
        public static object LoadXaml(string filename)
        {
            IntPtr root = Noesis_LoadXaml(filename ?? string.Empty);
            return Extend.GetProxy(root, true);
        }

        /// <summary>
        /// Parses a well-formed XAML fragment and creates the corresponding object tree.
        /// </summary>
        public static object ParseXaml(string xamlText)
        {
            IntPtr root = Noesis_ParseXaml(xamlText);
            return Extend.GetProxy(root, true);
        }

        /// <summary>
        /// Loads a XAML resource, like an audio, at the given uniform resource identifier.
        /// </summary>
        public static Stream LoadXamlResource(string filename)
        {
            IntPtr stream = Noesis_LoadXamlResource(filename ?? string.Empty);
            return (Stream)Extend.GetProxy(stream, true);
        }

        /// <summary>
        /// Enumerates all the faces found in the provided font stream resource
        /// </summary>
        public static void EnumFontFaces(Stream font, FontFaceInfoCallback callback)
        {
            Faces faces = new Faces { Callback = callback };
            int callbackId = faces.GetHashCode();
            _facesCallbacks[callbackId] = faces;
            Noesis_EnumFontFaces(Extend.GetInstanceHandle(font), callbackId, _fontFaces);
            _facesCallbacks.Remove(callbackId);
        }

        /// <summary>
        /// Loads contents of the specified component from a XAML.
        /// Used from InitializeComponent; supplied component must match the type of the XAML root
        /// </summary>
        public static void LoadComponent(object component, string filename)
        {
            Noesis_LoadComponent(Extend.GetInstanceHandle(component), filename ?? string.Empty);
        }

        /// <summary>
        /// Callback for assembly loading. It will be called for each assembly referenced in a XAML,
        /// for example when a xmlns attribute specifies an assembly:
        ///   <UserControl xmlns:controls="clr-namespace:Noesis.Controls;assembly=Noesis.GUI.Controls"/>
        /// or when a resource Uri specifies an assembly:
        ///   <Image Source="/SharedResources;component/Images/account.png"/>
        /// </summary>
        public static void SetLoadAssemblyCallback(LoadAssemblyCallback callback)
        {
            _loadAssemblyCallback = callback;
            Noesis_SetLoadAssemblyCallback(callback != null ? _loadAssembly : null);
        }

        /// <summary>
        /// Creates a View that manages a tree of UI elements.
        /// </summary>
        /// <param name="content">Root of the UI tree.</param>
        /// <returns>A new View for the specified content.</returns>
        public static View CreateView(FrameworkElement content)
        {
            return new View(content);
        }

        /// <summary>
        /// Unregisters the native types generated for managed extend classes, so they can be
        /// modified and updated without unloading NoesisGUI.
        /// </summary>
        public static void UnregisterNativeTypes()
        {
            Extend.UnregisterNativeTypes();
        }

        #region Software Keyboard
        private static SoftwareKeyboardCallback _softwareKeyboardCallback;

        private delegate void NoesisSoftwareKeyboardCallback(IntPtr cPtrFocused, bool open);
        private static NoesisSoftwareKeyboardCallback _softwareKeyboard = OnSoftwareKeyboard;
        [MonoPInvokeCallback(typeof(NoesisSoftwareKeyboardCallback))]
        private static void OnSoftwareKeyboard(IntPtr cPtrFocused, bool open)
        {
            try
            {
                if (_initialized)
                {
                    UIElement focused = Extend.GetProxy(cPtrFocused, false) as UIElement;
                    _softwareKeyboardCallback(focused, open);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }
        #endregion

        #region Cursor
        private static UpdateCursorCallback _updateCursorCallback;

        private delegate void NoesisUpdateCursorCallback(IntPtr cPtrView, IntPtr cursorPtr);
        private static NoesisUpdateCursorCallback _updateCursor = OnUpdateCursor;
        [MonoPInvokeCallback(typeof(NoesisUpdateCursorCallback))]
        private static void OnUpdateCursor(IntPtr cPtrView, IntPtr cursorPtr)
        {
            try
            {
                if (_initialized)
                {
                    View view = (View)Extend.GetProxy(cPtrView, false);
                    Cursor cursor = (Cursor)Extend.GetProxy(cursorPtr, false);
                    _updateCursorCallback(view, cursor);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }
        #endregion

        #region OpenURL
        private static OpenUrlCallback _openUrlCallback;

        private delegate void NoesisOpenUrlCallback(IntPtr url);
        private static NoesisOpenUrlCallback _openUrl = OnOpenUrl;
        [MonoPInvokeCallback(typeof(NoesisOpenUrlCallback))]
        private static void OnOpenUrl(IntPtr url)
        {
            try
            {
                if (_initialized)
                {
                    _openUrlCallback(Extend.StringFromNativeUtf8(url));
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }
        #endregion

        #region PlayAudio
        private static PlayAudioCallback _playAudioCallback;

        private delegate void NoesisPlayAudioCallback(IntPtr sound, float volume);
        private static NoesisPlayAudioCallback _playAudio = OnPlayAudio;
        [MonoPInvokeCallback(typeof(NoesisPlayAudioCallback))]
        private static void OnPlayAudio(IntPtr sound, float volume)
        {
            try
            {
                if (_initialized)
                {
                    string uriStr = Extend.StringFromNativeUtf8(sound);
                    _playAudioCallback(new Uri(uriStr, UriKind.RelativeOrAbsolute), volume);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }
        #endregion

        #region Xaml Dependencies
        private struct Deps
        {
            public XamlDependencyCallback Callback { get; set; }
        }

        private delegate void NoesisXamlDependencyCallback(int callbackId, IntPtr uri, int type);
        private static NoesisXamlDependencyCallback _xamlDep = OnXamlDependency;
        [MonoPInvokeCallback(typeof(NoesisXamlDependencyCallback))]
        private static void OnXamlDependency(int callbackId, IntPtr uri, int type)
        {
            try
            {
                if (_initialized)
                {
                    Deps deps = _depsCallbacks[callbackId];
                    string uriStr = Extend.StringFromNativeUtf8(uri);
                    deps.Callback(new Uri(uriStr, UriKind.RelativeOrAbsolute), (XamlDependencyType)type);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private static Dictionary<int, Deps> _depsCallbacks = new Dictionary<int, Deps>();
        #endregion

        #region Enum Font Faces
        private struct Faces
        {
            public FontFaceInfoCallback Callback { get; set; }
        }

        private delegate void NoesisFontFaceInfoCallback(int callbackId, int index,
            IntPtr familyName, int weight, int style, int stretch);
        private static NoesisFontFaceInfoCallback _fontFaces = OnFontFace;
        [MonoPInvokeCallback(typeof(NoesisFontFaceInfoCallback))]
        private static void OnFontFace(int callbackId, int index, IntPtr familyName,
            int weight, int style, int stretch)
        {
            try
            {
                if (_initialized)
                {
                    Faces faces = _facesCallbacks[callbackId];
                    faces.Callback(index, Extend.StringFromNativeUtf8(familyName),
                        (FontWeight)weight, (FontStyle)style, (FontStretch)stretch);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private static Dictionary<int, Faces> _facesCallbacks = new Dictionary<int, Faces>();
        #endregion

        #region Load Assembly
        private static LoadAssemblyCallback _loadAssemblyCallback;

        private delegate void NoesisLoadAssemblyCallback(IntPtr assembly);
        private static NoesisLoadAssemblyCallback _loadAssembly = OnLoadAssembly;
        [MonoPInvokeCallback(typeof(NoesisLoadAssemblyCallback))]
        private static void OnLoadAssembly(IntPtr assemblyPtr)
        {
            try
            {
                if (_initialized)
                {
                    string assembly = Extend.StringFromNativeUtf8(assemblyPtr);
                    _loadAssemblyCallback(assembly);
                }
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }
        #endregion

        #region Imports
        [DllImport(Library.Name)]
        static extern IntPtr Noesis_GetBuildVersion();

        [DllImport(Library.Name)]
        static extern void Noesis_DisableHotReload();

        [DllImport(Library.Name)]
        static extern void Noesis_DisableSocketInit();

        [DllImport(Library.Name)]
        static extern void Noesis_DisableInspector();

        [DllImport(Library.Name)]
        [return: MarshalAs(UnmanagedType.U1)]
        static extern bool Noesis_IsInspectorConnected();

        [DllImport(Library.Name)]
        static extern void Noesis_UpdateInspector();

        [DllImport(Library.Name)]
        static extern void Noesis_SetLicense([MarshalAs(UnmanagedType.LPStr)] string Name,
            [MarshalAs(UnmanagedType.LPStr)] string key);

        [DllImport(Library.Name)]
        static extern void Noesis_Init();

        [DllImport(Library.Name)]
        static extern void Noesis_Shutdown();

        [DllImport(Library.Name)]
        static extern void Noesis_SetXamlProvider(HandleRef provider);

        [DllImport(Library.Name)]
        static extern void Noesis_SetTextureProvider(HandleRef provider);

        [DllImport(Library.Name)]
        static extern void Noesis_SetFontProvider(HandleRef provider);

        [DllImport(Library.Name)]
        static extern void Noesis_SetFontFallbacks(string[] familyNames, int count);

        [DllImport(Library.Name)]
        static extern void Noesis_SetFontDefaultProperties(float size, int weight, int stretch,
            int style);

        [DllImport(Library.Name)]
        static extern void Noesis_SetApplicationResources(HandleRef resources);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_GetApplicationResources();

        [DllImport(Library.Name)]
        static extern void Noesis_SetSoftwareKeyboardCallback(
            NoesisSoftwareKeyboardCallback callback);

        [DllImport(Library.Name)]
        static extern void Noesis_SetCursorCallback(NoesisUpdateCursorCallback callback);

        [DllImport(Library.Name)]
        static extern void Noesis_SetOpenUrlCallback(NoesisOpenUrlCallback callback);

        [DllImport(Library.Name)]
        static extern void Noesis_SetPlayAudioCallback(NoesisPlayAudioCallback callback);

        [DllImport(Library.Name)]
        static extern void Noesis_GetXamlDependencies(HandleRef stream,
            [MarshalAs(UnmanagedType.LPStr)] string baseUri, int callbackId,
            NoesisXamlDependencyCallback callback);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_LoadStreamXaml(HandleRef stream,
            [MarshalAs(UnmanagedType.LPStr)] string filename);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_LoadXaml([MarshalAs(UnmanagedType.LPStr)] string filename);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_ParseXaml([MarshalAs(UnmanagedType.LPStr)] string xamlText);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_LoadXamlResource(
            [MarshalAs(UnmanagedType.LPStr)] string filename);

        [DllImport(Library.Name)]
        static extern void Noesis_EnumFontFaces(HandleRef stream, int callbackId,
            NoesisFontFaceInfoCallback callback);

        [DllImport(Library.Name)]
        static extern void Noesis_LoadComponent(HandleRef component,
            [MarshalAs(UnmanagedType.LPStr)] string filename);

        [DllImport(Library.Name)]
        static extern void Noesis_SetLoadAssemblyCallback(NoesisLoadAssemblyCallback callback);
        #endregion
    }
}
