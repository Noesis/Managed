using System;
using Noesis;
using NoesisApp;
using System.Threading;
using System.IO;
using System.Reflection;

namespace NoesisApp
{
    public class StartupEventArgs : System.EventArgs
    {
    }

    public delegate void StartupEventHandler(object sender, StartupEventArgs e);

    public class ExitEventArgs : System.EventArgs
    {
        public int ApplicationExitCode { get; internal set; }
    }

    public delegate void ExitEventHandler(object sender, ExitEventArgs e);


    /// <summary>
    /// Application base class
    /// </summary>
    public abstract class Application
    {
        public static Application Current { get; private set; }

        public Dispatcher Dispatcher { get; private set; }

        public string Uri { get; set; }

        public string StartupUri { get; set; }

        public DateTime StartTime { get; private set; }

        public Display Display { get; set; }

        public Window MainWindow { get; set; }

        private ApplicationProvider _resourceProvider = null;
        public XamlProvider XamlProvider { get { return _resourceProvider?.XamlProvider; } }
        public FontProvider FontProvider { get { return _resourceProvider?.FontProvider; } }
        public TextureProvider TextureProvider { get { return _resourceProvider?.TextureProvider; } }

        private ResourceDictionary _resources = null;
        public ResourceDictionary Resources
        {
            get { return _resources; }
            set
            {
                if (_resources != value)
                {
                    _resources = value;
                    GUI.SetApplicationResources(_resources);
                }
            }
        }

        /// <summary>
        /// Searches for a resource, such as a Style or Brush, with the specified key.
        /// If the requested resource is not found, an Exception is thrown.
        /// </summary>
        public object FindResource(object key)
        {
            object resource = null;
            ResourceDictionary resources = _resources;
            if (resources != null)
            {
                resource = resources[key];
            }
            if (resource == null)
            {
                throw new Exception($"Resource {key} not found");
            }
            return resource;
        }

        /// <summary>
        /// Installs providers that expose resources for Noesis theme.
        /// </summary>
        public static void SetThemeProviders(XamlProvider xamlProvider = null, FontProvider fontProvider = null, TextureProvider textureProvider = null)
        {
            GUI.SetXamlProvider(new EmbeddedXamlProvider(Theme.Assembly, Theme.Namespace, xamlProvider));
            GUI.SetFontProvider(new EmbeddedFontProvider(Theme.Assembly, Theme.Namespace, fontProvider));
            GUI.SetTextureProvider(textureProvider);

            GUI.SetFontFallbacks(Theme.FontFallbacks);
            GUI.SetFontDefaultProperties(Theme.DefaultFontSize, Theme.DefaultFontWeight, Theme.DefaultFontStretch, Theme.DefaultFontStyle);
        }

        public event StartupEventHandler Startup;

        public event ExitEventHandler Exit;

        public Application()
        {
            if (Current != null)
            {
                throw new InvalidOperationException("An Application was already created");
            }

            Current = this;
            Thread.BeginThreadAffinity();
            Dispatcher = Dispatcher.CurrentDispatcher;
            SynchronizationContext.SetSynchronizationContext(Dispatcher.SynchronizationContext);

            _exitCode = 0;

            Log.SetLogCallback(LogCallback);
            Error.SetUnhandledCallback(UnhandledCallback);

            LicenseInfo license = GetLicenseInfo();
            GUI.SetLicense(license.Name, license.Key);
            GUI.Init();
        }

        private struct LicenseInfo
        {
            public string Name;
            public string Key;
        }

        private LicenseInfo GetLicenseInfo()
        {
            Type type = this.GetType();
            Stream stream = GetAssemblyResource(type.Assembly, type.Namespace, "NoesisLicense.txt");
            if (stream != null)
            {
                string name, key;
                using (StreamReader reader = new StreamReader(stream))
                {
                    name = reader.ReadLine();
                    key = name != null ? reader.ReadLine() : null;
                }

                if (name != null && key != null)
                {
                    return new LicenseInfo { Name = name, Key = key };
                }
            }

            return new LicenseInfo { Name = "", Key = "" };
        }

        public static Stream GetAssemblyResource(Assembly assembly, string ns, string filename)
        {
            if (assembly != null && !string.IsNullOrEmpty(ns))
            {
                string resource = ns + "." + filename.Replace("/", ".");
                try
                {
                    return assembly.GetManifestResourceStream(resource);
                }
                catch { }
            }

            return null;
        }

        ///<summary>
        /// Run is called to start an application with a render loop
        /// Once run has been called - an application's OnStartup override and Startup event is
        /// called immediately afterwards.
        ///</summary>
        /// <returns>ExitCode of the application</returns>
        public int Run()
        {
            Start();

            Display.Show();

            StartTime = DateTime.Now;
            Display.Render += (d) =>
            {
                MainWindow.Render((DateTime.Now - StartTime).TotalSeconds);
            };

            Display.EnterMessageLoop(RunInBackground);

            Finish();

            return _exitCode;
        }

        private void Start()
        {
            // Create application display
            Display = CreateDisplay();

            // Redirect integration callbacks to display
            GUI.SetSoftwareKeyboardCallback((UIElement focused, bool open) =>
            {
                if (open)
                {
                    Display.OpenSoftwareKeyboard(focused);
                }
                else
                {
                    Display.CloseSoftwareKeyboard();
                }
            });

            GUI.SetCursorCallback((View view, Cursor cursor) =>
            {
                Display.SetCursor(cursor);
            });

            GUI.SetOpenUrlCallback((string url) =>
            {
                Display.OpenUrl(url);
            });

            GUI.SetPlayAudioCallback((Uri uri, float volume) =>
            {
                Display.PlayAudio(uri, volume);
            });

            // Set resource providers
            _resourceProvider = new ApplicationProvider(this, CreateXamlProvider(), CreateFontProvider(), CreateTextureProvider());

            GUI.SetXamlProvider(XamlProvider);
            GUI.SetFontProvider(FontProvider);
            GUI.SetTextureProvider(TextureProvider);

            // Listen to load assembly callback to add new assemblies to the resource provider
            GUI.SetLoadAssemblyCallback((name) =>
            {
                try
                {
                    Assembly assembly = Assembly.Load(name);
                    if (assembly != null)
                    {
                        _resourceProvider.AddAssembly(assembly, name);
                    }
                }
                catch { }
            });

            // Load App.xaml
            if (string.IsNullOrEmpty(Uri))
            {
                throw new InvalidOperationException("App xaml uri not defined");
            }

            GUI.SetFontFallbacks(GetFontFallbacks());
            GUI.SetFontDefaultProperties(GetDefaultFontSize(), GetDefaultFontWeight(), GetDefaultFontStretch(), GetDefaultFontStyle());

            GUI.LoadComponent(this, Uri);
            GUI.SetApplicationResources(Resources);

            // Load StartupUri xaml as MainWindow
            if (string.IsNullOrEmpty(StartupUri))
            {
                throw new InvalidOperationException("Startup window not defined");
            }

            object root = GUI.LoadXaml(StartupUri);
            MainWindow = root as Window;

            if (MainWindow == null)
            {
                // non window roots are allowed
                MainWindow = new Window();
                MainWindow.Content = root;
                MainWindow.Title = Title;
            }

            bool vsync = VSync;
            bool sRGB = StandardRGB;
            bool ppaa = PPAA;
            bool lcd = LCD;
            bool emulateTouch = EmulateTouch;
            uint holdingTimeThreshold = HoldingTimeThreshold;
            uint holdingDistanceThreshold = HoldingDistanceThreshold;
            uint doubleTapTimeThreshold = DoubleTapTimeThreshold;
            uint doubleTapDistanceThreshold = DoubleTapDistanceThreshold;
            uint samples = Samples;

            RenderContext renderContext = CreateRenderContext();
            renderContext.Init(Display.NativeHandle, Display.NativeWindow, samples, vsync, sRGB);
            renderContext.Device.OffscreenSampleCount = samples;
            renderContext.Device.OffscreenWidth = OffscreenWidth;
            renderContext.Device.OffscreenHeight = OffscreenHeight;
            renderContext.Device.OffscreenDefaultNumSurfaces = OffscreenDefaultNumSurfaces;
            renderContext.Device.OffscreenMaxNumSurfaces = OffscreenMaxNumSurfaces;
            renderContext.Device.GlyphCacheWidth = GlyphCacheWidth;
            renderContext.Device.GlyphCacheHeight = GlyphCacheHeight;

            Display.NativeHandleChanged += (Display display, IntPtr window) =>
            {
                renderContext.SetWindow(window);
            };

            MainWindow.Init(Display, renderContext, samples, ppaa, lcd, emulateTouch,
                holdingTimeThreshold, holdingDistanceThreshold,
                doubleTapTimeThreshold, doubleTapDistanceThreshold);

            StartupEventArgs e = new StartupEventArgs();
            OnStartup(e);
        }

        private void Finish()
        {
            ExitEventArgs e = new ExitEventArgs { ApplicationExitCode = _exitCode };
            try
            {
                OnExit(e);
            }
            finally
            {
                _exitCode = e.ApplicationExitCode;

                // Make sure the View is destroyed after the Window element tree
                View view = MainWindow.View;

                MainWindow.Content = null;
                MainWindow.Shutdown();
                MainWindow = null;

                _resourceProvider = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                view = null;
                Resources = null;
                Current = null;

                // Shut down Noesis
                GUI.Shutdown();

                Thread.EndThreadAffinity();
            }
        }

        /// <summary>
        ///  Shuts down the application. You can handle the Exit event to detect when an application is
        ///  about to stop running, to perform any appropriate processing.
        /// </summary>
        public void Shutdown()
        {
            Shutdown(0);
        }

        /// <summary>
        /// Shuts down the application that returns the specified exit code to the operating system.
        /// </summary>
        /// <param name="exitCode">An integer exit code. The default is 0.</param>
        public void Shutdown(int exitCode)
        {
            _exitCode = exitCode;
            MainWindow.Close();
        }

        /// <summary> Logs an information message </summary>
        public void LogInfo(string message)
        {
            OnLog(LogLevel.Info, "", message);
        }

        /// <summary> Logs a warning message </summary>
        public void LogWarning(string message)
        {
            OnLog(LogLevel.Warning, "", message);
        }

        /// <summary> Logs an error message </summary>
        public void LogError(string message)
        {
            OnLog(LogLevel.Error, "", message);
        }

        private void LogCallback(LogLevel level, string channel, string message)
        {
            OnLog(level, channel, message);
        }

        /// <summary>
        /// Called when Noesis wants to print a message
        /// </summary>
        /// <param name="level">Trace, Debug, Info, Warning or Error</param>
        /// <param name="channel">General channel is the empty string</param>
        /// <param name="message">Message text</param>
        protected virtual void OnLog(LogLevel level, string channel, string message)
        {
            // If channel is not main ("") filter out debug and info messages
            bool filter = !string.IsNullOrEmpty(channel) && level < LogLevel.Warning;

            // Use LogBinding to also enable debug and info for bindings channel
            if (LogBinding && channel == "Binding")
            {
                filter = false;
            }

            if (!filter)
            {
                level = level > LogLevel.Error ? LogLevel.Error : level;
                string[] prefixes = new string[] { "T", "D", "I", "W", "E" };
                string prefix = (int)level < prefixes.Length ? prefixes[(int)level] : " ";
                System.Console.WriteLine("[NOESIS/" + prefix + "] " + message);
            }
        }

        private void UnhandledCallback(Exception exception)
        {
            OnUnhandledException(exception);
        }

        /// <summary>
        /// Called when an unhandled exception occurs in C# during a Native to Managed transition.
        /// No throws should be raised inside this function to avoid stack unwinding problems.
        /// </summary>
        /// <param name="exception">Unhanlded exception generated in managed code</param>
        protected virtual void OnUnhandledException(Exception exception)
        {
            // Exception stack trace stops on managed-native transitions, so we create a StackTrace
            // object that contains information of the full call stack
            System.Diagnostics.StackTrace stack = new System.Diagnostics.StackTrace(3, true);
            System.Console.WriteLine(exception.GetType() + ": " + exception.Message + "\n" +
                exception.StackTrace + "\n" + stack);
        }

        /// <summary>
        /// Creates an operating system Display where the application will be rendered
        /// </summary>
        protected abstract Display CreateDisplay();

        /// <summary>
        /// Creates the RenderContext used to render the application
        /// </summary>
        protected abstract RenderContext CreateRenderContext();

        /// <summary>
        /// Creates the provider used for loading XAML files and resources
        /// </summary>
        protected virtual XamlProvider CreateXamlProvider()
        {
            return null;
        }

        /// <summary>
        /// Creates the provider used for loading textures
        /// </summary>
        protected virtual FileTextureProvider CreateTextureProvider()
        {
            return null;
        }

        /// <summary>
        ///  Creates the provider used for loading fonts
        /// </summary>
        protected virtual FontProvider CreateFontProvider()
        {
            return null;
        }

        protected virtual string[] GetFontFallbacks()
        {
            return Theme.FontFallbacks;
        }

        protected virtual float GetDefaultFontSize()
        {
            return Theme.DefaultFontSize;
        }

        protected virtual FontWeight GetDefaultFontWeight()
        {
            return Theme.DefaultFontWeight;
        }

        protected virtual FontStretch GetDefaultFontStretch()
        {
            return Theme.DefaultFontStretch;
        }

        protected virtual FontStyle GetDefaultFontStyle()
        {
            return Theme.DefaultFontStyle;
        }

        protected virtual string Title
        {
            get { return ""; }
        }

        protected virtual bool LogBinding
        {
            get { return false; }
        }

        protected virtual bool RunInBackground
        {
            get { return false; }
        }

        protected virtual bool VSync
        {
            get { return true; }
        }

        protected virtual bool StandardRGB
        {
            get { return false; }
        }

        protected virtual bool PPAA
        {
            get { return Samples == 1; }
        }

        protected virtual bool LCD
        {
            get { return false; }
        }

        #region Touch configuration
        protected virtual bool EmulateTouch
        {
            get { return false; }
        }

        protected virtual uint HoldingTimeThreshold
        {
            get { return 500; }
        }

        protected virtual uint HoldingDistanceThreshold
        {
            get { return 10; }
        }

        protected virtual uint DoubleTapTimeThreshold
        {
            get { return 500; }
        }

        protected virtual uint DoubleTapDistanceThreshold
        {
            get { return 10; }
        }
        #endregion

        #region RenderDevice configuration
        protected virtual uint Samples
        {
            get { return 1; }
        }

        protected virtual uint OffscreenWidth
        {
            get { return 0; }
        }

        protected virtual uint OffscreenHeight
        {
            get { return 0; }
        }

        protected virtual uint OffscreenDefaultNumSurfaces
        {
            get { return 0; }
        }

        protected virtual uint OffscreenMaxNumSurfaces
        {
            get { return 0; }
        }

        protected virtual uint GlyphCacheWidth
        {
            get { return 1024; }
        }

        protected virtual uint GlyphCacheHeight
        {
            get { return 1024; }
        }
        #endregion

        protected virtual void OnStartup(StartupEventArgs e)
        {
            Startup?.Invoke(this, e);
        }

        protected virtual void OnExit(ExitEventArgs e)
        {
            Exit?.Invoke(this, e);
        }

        #region Private members
        int _exitCode;
        #endregion
    }
}

