using System;
using Noesis;
using NoesisApp;

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

        public string Uri { get; set; }

        public string StartupUri { get; set; }

        public Display Display { get; set; }

        public ResourceDictionary Resources { get; set; }

        public event StartupEventHandler Startup;

        public event ExitEventHandler Exit;

        public Application()
        {
            if (Current != null)
            {
                throw new InvalidOperationException("An Application was already created");
            }

            Current = this;

            _exitCode = 0;

            Log.SetLogCallback(OnLog);
            Error.SetUnhandledCallback(OnUnhandledException);
            GUI.Init();
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

            DateTime startTime = DateTime.Now;
            Display.Render += (d) =>
            {
                _mainWindow.Render((DateTime.Now - startTime).TotalSeconds);
            };

            bool runInBackground = false; // TODO: decide how to get this option
            Display.EnterMessageLoop(runInBackground);

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

            GUI.SetPlaySoundCallback((string filename, float volume) =>
            {
                Display.PlaySound(filename, volume);
            });

            // Set resource providers
            GUI.SetXamlProvider(CreateXamlProvider());
            GUI.SetTextureProvider(CreateTextureProvider());
            GUI.SetFontProvider(CreateFontProvider());

            // Load App.xaml
            if (string.IsNullOrEmpty(Uri))
            {
                throw new InvalidOperationException("App xaml uri not defined");
            }

            GUI.LoadComponent(this, Uri);
            GUI.SetApplicationResources(Resources);

            // Load StartupUri xaml as MainWindow
            if (string.IsNullOrEmpty(StartupUri))
            {
                throw new InvalidOperationException("Startup window not defined");
            }

            object root = GUI.LoadXaml(StartupUri);
            _mainWindow = root as Window;

            if (_mainWindow == null)
            {
                // non window roots are allowed
                _mainWindow = new Window();
                _mainWindow.Content = root;
                _mainWindow.Title = Title;
            }

            uint samples = Samples;
            bool vsync = VSync;
            bool sRGB = StandardRGB;
            bool ppaa = PPAA;

            RenderContext renderContext = CreateRenderContext();
            renderContext.Init(Display.NativeHandle, Display.NativeWindow, samples, vsync, sRGB);
            renderContext.Device.OffscreenSampleCount = samples;

            Display.NativeHandleChanged += (Display display, IntPtr window) =>
            {
                renderContext.SetWindow(window);
            };

            _mainWindow.Init(Display, renderContext, samples, ppaa);

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

                _mainWindow.Shutdown();
                _mainWindow = null;

                Resources = null;

                Current = null;

                // Shut down Noesis
                GUI.Shutdown();
            }
        }

        public void Shutdown()
        {
            Shutdown(0);
        }

        public void Shutdown(int exitCode)
        {
            _exitCode = exitCode;
            _mainWindow.Close();
        }

        private void OnLog(LogLevel level, string channel, string message)
        {
            if (string.IsNullOrEmpty(channel) || channel == "Binding")
            {
                string[] prefixes = new string[] { "T", "D", "I", "W", "E" };
                string prefix = (int)level < prefixes.Length ? prefixes[(int)level] : " ";
                System.Diagnostics.Debug.WriteLine("[NOESIS/" + prefix + "] " + message);
            }
        }

        private void OnUnhandledException(Exception exception)
        {
            // Exception stack trace stops on managed-native transitions, so we create a StackTrace
            // object that contains information of the full call stack
            System.Diagnostics.StackTrace stack = new System.Diagnostics.StackTrace(3, true);
            System.Diagnostics.Debug.WriteLine(exception.GetType() + ": " + exception.Message + "\n" +
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
            return new EmbeddedXamlProvider(this);
        }

        /// <summary>
        /// Creates the provider used for loading textures
        /// </summary>
        /// <returns></returns>
        protected virtual TextureProvider CreateTextureProvider()
        {
            return new EmbeddedTextureProvider(this);
        }

        /// <summary>
        ///  Creates the provider used for loading fonts
        /// </summary>
        protected virtual FontProvider CreateFontProvider()
        {
            return new EmbeddedFontProvider(this);
        }

        protected virtual string Title
        {
            get { return ""; }
        }

        protected virtual uint Samples
        {
            get { return 1; }
        }

        protected virtual bool VSync
        {
            get { return false; }
        }

        protected virtual bool StandardRGB
        {
            get { return false; }
        }

        protected virtual bool PPAA
        {
            get { return Samples == 1; }
        }

        protected virtual void OnStartup(StartupEventArgs e)
        {
            Startup?.Invoke(this, e);
        }

        protected virtual void OnExit(ExitEventArgs e)
        {
            Exit?.Invoke(this, e);
        }

        #region Private members
        Window _mainWindow;
        int _exitCode;
        #endregion
    }
}

