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
    public delegate void PlaySoundCallback(string filename, float volume);

    /// <summary> Called for every xaml dependency found </summary>
    public delegate void XamlDependencyCallback(string uri, XamlDependencyType type);

    /// <summary> Called for every font face found in the ttf </summary>
    public delegate void FontFaceInfoCallback(int index, string familyName, FontWeight weight,
        FontStyle style, FontStretch stretch);

    public static class GUI
    {
        /// <summary>
        /// Returns the build version, for example "1.2.6f5".
        /// </summary>
        public static string GetBuildVersion()
        {
            IntPtr version = Noesis_GetBuildVersion();
            return Noesis.Extend.StringFromNativeUtf8(version);
        }

        private static bool _initialized = false;

        /// <summary>
        /// Initializes NoesisGUI.
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
            Noesis_SetPlaySoundCallback(null);
            Noesis_SetOpenUrlCallback(null);
            Noesis_SetCursorCallback(null);
            Noesis_SetSoftwareKeyboardCallback(null);

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
        /// Sets a collection of application-scope resources, such as styles and brushes. Provides a
        /// simple way to support a consistent theme across your application.
        /// </summary>
        /// <param name="resources">Application resources.</param>
        public static void SetApplicationResources(ResourceDictionary resources)
        {
            Noesis_SetApplicationResources(Extend.GetInstanceHandle(resources));
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
        public static void SetPlaySoundCallback(PlaySoundCallback callback)
        {
            _playSoundCallback = callback;
            Noesis_SetPlaySoundCallback(callback != null ? _playSound : null);
        }

        /// <summary>
        /// Plays the specified sound with the provided callback.
        /// </summary>
        public static void PlaySound(string filename, float volume)
        {
            if (_playSoundCallback != null)
            {
                _playSoundCallback(filename, volume);
            }
        }

        /// <summary>
        /// Finds dependencies to other XAMLS and resources (fonts, textures, sounds...).
        /// </summary>
        /// <param name="xaml">Stream with xaml content.</param>
        /// <param name="folder">Root directory used for relative dependencies.</param>
        /// <param name="callback">Called for each dependency found.</param>
        public static void GetXamlDependencies(Stream xaml, string folder,
            XamlDependencyCallback callback)
        {
            Deps deps = new Deps { Callback = callback };
            int callbackId = deps.GetHashCode();
            _depsCallbacks[callbackId] = deps;
            Noesis_GetXamlDependencies(Extend.GetInstanceHandle(xaml), folder, callbackId, _xamlDep);
            _depsCallbacks.Remove(callbackId);
        }

        /// <summary>
        /// Loads a XAML resource.
        /// </summary>
        /// <param name="filename">Path to the resource.</param>
        /// <returns>Root of the loaded XAML.</returns>
        public static object LoadXaml(string filename)
        {
            IntPtr root = Noesis_LoadXaml(filename);
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
            IntPtr stream = Noesis_LoadXamlResource(filename);
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
            Noesis_LoadComponent(Extend.GetInstanceHandle(component), filename);
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

        private delegate void NoesisUpdateCursorCallback(IntPtr cPtrView, int cursor);
        private static NoesisUpdateCursorCallback _updateCursor = OnUpdateCursor;
        [MonoPInvokeCallback(typeof(NoesisUpdateCursorCallback))]
        private static void OnUpdateCursor(IntPtr cPtrView, int cursor)
        {
            try
            {
                if (_initialized)
                {
                    View view = (View)Extend.GetProxy(cPtrView, false);
                    _updateCursorCallback(view, (Cursor)cursor);
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

        #region PlaySound
        private static PlaySoundCallback _playSoundCallback;

        private delegate void NoesisPlaySoundCallback(IntPtr sound, float volume);
        private static NoesisPlaySoundCallback _playSound = OnPlaySound;
        [MonoPInvokeCallback(typeof(NoesisPlaySoundCallback))]
        private static void OnPlaySound(IntPtr sound, float volume)
        {
            try
            {
                if (_initialized)
                {
                    _playSoundCallback(Extend.StringFromNativeUtf8(sound), volume);
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
                    deps.Callback(Extend.StringFromNativeUtf8(uri), (XamlDependencyType)type);
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

        #region Imports
        [DllImport(Library.Name)]
        static extern IntPtr Noesis_GetBuildVersion();

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
        static extern void Noesis_SetApplicationResources(HandleRef resources);

        [DllImport(Library.Name)]
        static extern void Noesis_SetSoftwareKeyboardCallback(
            NoesisSoftwareKeyboardCallback callback);

        [DllImport(Library.Name)]
        static extern void Noesis_SetCursorCallback(NoesisUpdateCursorCallback callback);

        [DllImport(Library.Name)]
        static extern void Noesis_SetOpenUrlCallback(NoesisOpenUrlCallback callback);

        [DllImport(Library.Name)]
        static extern void Noesis_SetPlaySoundCallback(NoesisPlaySoundCallback callback);

        [DllImport(Library.Name)]
        static extern void Noesis_GetXamlDependencies(HandleRef stream,
            [MarshalAs(UnmanagedType.LPStr)] string folder, int callbackId,
            NoesisXamlDependencyCallback callback);

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
        #endregion
    }
}
